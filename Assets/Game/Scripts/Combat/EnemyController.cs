using System;
using UnityEngine;

namespace MonsterSurvivor.Combat
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D), typeof(Health))]
    public sealed class EnemyController : MonoBehaviour
    {
        [SerializeField] private EnemyDefinition definition;
        private Rigidbody2D body;
        private Health health;
        private Health goal;
        private Collider2D goalCollider;
        private Transform[] waypoints;
        private int waypointIndex;
        private AttackCooldown cooldown;

        public Vector2 MoveDirection { get; private set; }
        public bool IsCombatActive { get; private set; }
        public bool IsAlive => isActiveAndEnabled && health.IsAlive;
        public event Action<EnemyController> Removed;

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
            IsCombatActive = IsAlive && goal != null && goalCollider != null;
        }

        public void StopCombat()
        {
            IsCombatActive = false;
            MoveDirection = Vector2.zero;
            if (body != null)
                body.linearVelocity = Vector2.zero;
        }

        private void FixedUpdate()
        {
            if (!IsCombatActive || !IsAlive || goal == null || !goal.IsAlive)
            {
                StopCombat();
                return;
            }

            cooldown.Advance(Time.fixedDeltaTime);
            Vector2 goalPoint = goalCollider.ClosestPoint(body.position);
            if ((goalPoint - body.position).sqrMagnitude <= definition.AttackRange * definition.AttackRange)
            {
                MoveDirection = Vector2.zero;
                body.linearVelocity = Vector2.zero;
                if (cooldown.TryAttack())
                    goal.TakeDamage(definition.Damage);
                return;
            }

            while (waypoints != null && waypointIndex < waypoints.Length)
            {
                Transform point = waypoints[waypointIndex];
                if (point != null && ((Vector2)point.position - body.position).sqrMagnitude > 0.04f)
                    break;
                waypointIndex++;
            }

            Vector2 destination = waypoints != null && waypointIndex < waypoints.Length
                ? (Vector2)waypoints[waypointIndex].position : goalPoint;
            Vector2 delta = destination - body.position;
            MoveDirection = delta.normalized;
            body.linearVelocity = MoveDirection * Mathf.Min(definition.MoveSpeed,
                delta.magnitude / Time.fixedDeltaTime);
        }

        public void TakeDamage(int amount)
        {
            if (IsAlive && IsCombatActive)
                health.TakeDamage(amount);
        }

        private void HandleDeath() => gameObject.SetActive(false);

        private void OnDisable()
        {
            StopCombat();
            Removed?.Invoke(this);
        }

        private void OnDestroy()
        {
            if (health != null)
                health.Died -= HandleDeath;
        }
    }
}
