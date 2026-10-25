using System;
using MonsterSurvivor.Construction;
using UnityEngine;

namespace MonsterSurvivor.Combat
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D), typeof(Health))]
    public sealed class EnemyController : MonoBehaviour
    {
        [SerializeField] private EnemyDefinition definition;
        [SerializeField, Min(0f)] private float buildingScanPadding = 0.15f;

        private Rigidbody2D body;
        private Health health;
        private Health goal;
        private Collider2D goalCollider;
        private Transform[] waypoints;
        private int waypointIndex;
        private AttackCooldown cooldown;
        private Health buildingTarget;
        private Collider2D buildingTargetCollider;

        public Vector2 MoveDirection { get; private set; }
        public bool IsCombatActive { get; private set; }
        public bool IsAlive => isActiveAndEnabled && health != null && health.IsAlive;
        public event Action<EnemyController> Removed;
        private bool removalReported;
        public event Action<EnemyController> Killed;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            health = GetComponent<Health>();
            health.Died += HandleDeath;
            if (definition == null)
            {
                Debug.LogError("Assign an enemy definition.", this);
                enabled = false;
                return;
            }
            cooldown = new AttackCooldown(definition.AttackInterval);
        }

        public void Initialize(Health target, Collider2D targetCollider, Transform[] path)
        {
            goal = target;
            goalCollider = targetCollider;
            waypoints = path;
        }

        public void BeginCombat()
        {
            waypointIndex = 0;
            buildingTarget = null;
            buildingTargetCollider = null;
            IsCombatActive = IsAlive && goal != null && goalCollider != null;
        }

        public void StopCombat()
        {
            IsCombatActive = false;
            buildingTarget = null;
            buildingTargetCollider = null;
            MoveDirection = Vector2.zero;
            if (body != null) body.linearVelocity = Vector2.zero;
        }

        public void PrepareForSpawn(Health target, Collider2D targetCollider, Transform[] path)
        {
            StopCombat();
            removalReported = false;
            goal = target;
            goalCollider = targetCollider;
            waypoints = path;
            waypointIndex = 0;
            cooldown = new AttackCooldown(definition.AttackInterval);
            health.ResetHealth();
        }

        private void FixedUpdate()
        {
            if (!IsCombatActive || !IsAlive || goal == null || !goal.IsAlive || goalCollider == null)
            {
                StopCombat();
                return;
            }

            cooldown.Advance(Time.fixedDeltaTime);

            // Keep attacking the current building until it dies or is no longer a valid target.
            if (!IsValidBuildingTarget(buildingTarget, buildingTargetCollider) ||
                DistanceTo(buildingTargetCollider) > definition.AttackRange)
            {
                buildingTarget = null;
                buildingTargetCollider = null;
                FindBuildingTarget();
            }

            if (buildingTarget != null && buildingTargetCollider != null)
            {
                Vector2 targetPoint = buildingTargetCollider.ClosestPoint(body.position);
                if ((targetPoint - body.position).sqrMagnitude <= definition.AttackRange * definition.AttackRange)
                {
                    StopMovement();
                    if (cooldown.TryAttack()) buildingTarget.TakeDamage(definition.Damage);
                    return;
                }
            }

            Vector2 goalPoint = goalCollider.ClosestPoint(body.position);
            if ((goalPoint - body.position).sqrMagnitude <= definition.AttackRange * definition.AttackRange)
            {
                StopMovement();
                if (cooldown.TryAttack()) goal.TakeDamage(definition.Damage);
                return;
            }

            while (waypoints != null && waypointIndex < waypoints.Length)
            {
                Transform point = waypoints[waypointIndex];
                if (point != null && ((Vector2)point.position - body.position).sqrMagnitude > 0.04f) break;
                waypointIndex++;
            }

            Vector2 destination = waypoints != null && waypointIndex < waypoints.Length
                ? (Vector2)waypoints[waypointIndex].position : goalPoint;
            Vector2 delta = destination - body.position;
            MoveDirection = delta.normalized;
            body.linearVelocity = MoveDirection * Mathf.Min(definition.MoveSpeed, delta.magnitude / Time.fixedDeltaTime);
        }

        private void FindBuildingTarget()
        {
            // Include trigger colliders and colliders on child objects; Health/ConstructionSite may be on parents.
            Collider2D[] hits = Physics2D.OverlapCircleAll(body.position, definition.AttackRange + buildingScanPadding);
            float nearestSqrDistance = float.PositiveInfinity;

            foreach (Collider2D hit in hits)
            {
                if (hit == null || hit.transform == transform || hit.transform.IsChildOf(transform)) continue;
                Health candidateHealth = hit.GetComponentInParent<Health>();
                if (candidateHealth == null || candidateHealth == goal || !candidateHealth.IsAlive) continue;

                ConstructionSite site = candidateHealth.GetComponentInParent<ConstructionSite>();
                if (site == null || !site.IsBuilt) continue;

                Vector2 closestPoint = hit.ClosestPoint(body.position);
                float distanceSqr = (closestPoint - body.position).sqrMagnitude;
                if (distanceSqr > definition.AttackRange * definition.AttackRange || distanceSqr >= nearestSqrDistance) continue;

                buildingTarget = candidateHealth;
                buildingTargetCollider = hit;
                nearestSqrDistance = distanceSqr;
            }
        }

        private bool IsValidBuildingTarget(Health target, Collider2D targetCollider)
        {
            if (target == null || targetCollider == null || !target.IsAlive) return false;
            ConstructionSite site = target.GetComponentInParent<ConstructionSite>();
            return site != null && site.IsBuilt;
        }

        private float DistanceTo(Collider2D targetCollider)
        {
            if (targetCollider == null) return float.PositiveInfinity;
            return Vector2.Distance(body.position, targetCollider.ClosestPoint(body.position));
        }

        private void StopMovement()
        {
            MoveDirection = Vector2.zero;
            body.linearVelocity = Vector2.zero;
        }

        public void TakeDamage(int amount)
        {
            if (IsAlive && IsCombatActive) health.TakeDamage(amount);
        }

        private void HandleDeath()
        {
            if (removalReported) return;
            removalReported = true;
            StopCombat();
            Removed?.Invoke(this);
            Killed?.Invoke(this);
        }

        private void OnDisable()
        {
            bool wasInCombat = IsCombatActive;
            StopCombat();
            if (wasInCombat && !removalReported)
            {
                removalReported = true;
                Removed?.Invoke(this);
            }
        }

        private void OnDestroy()
        {
            if (health != null) health.Died -= HandleDeath;
        }
    }
}
