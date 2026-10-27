using System;
using MonsterSurvivor.Audio;
using MonsterSurvivor.Construction;
using MonsterSurvivor.VFX;
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

        // OPTIMIZATION 1: Shared buffer & filter for modern Zero-Allocation Physics (Unity 6+)
        private static readonly Collider2D[] hitBuffer = new Collider2D[16];
        private static ContactFilter2D scanFilter;
        private static bool filterInitialized;

        // OPTIMIZATION 2: Scan throttling to avoid heavy CPU load every FixedUpdate
        private float scanTimer;
        private const float ScanInterval = 0.25f;

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

            // Inisialisasi ContactFilter2D secara statis sekali saja untuk semua instance
            if (!filterInitialized)
            {
                scanFilter = new ContactFilter2D();
                scanFilter.NoFilter(); // Gunakan default (mendeteksi layer apa saja yang bertabrakan)
                scanFilter.useTriggers = true; // Karena di kode aslimu bisa jadi ada trigger
                filterInitialized = true;
            }

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
            scanTimer = 0f;
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
            scanTimer = 0f;
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

            float dt = Time.fixedDeltaTime;
            cooldown.Advance(dt);

            float attackRangeSqr = definition.AttackRange * definition.AttackRange;

            // Memastikan target saat ini masih hidup dan dalam radius pencarian
            if (!IsValidBuildingTarget(buildingTarget, buildingTargetCollider) ||
                DistanceToSqr(buildingTargetCollider) > attackRangeSqr)
            {
                buildingTarget = null;
                buildingTargetCollider = null;

                // Scan Throttling: Jangan scan setiap frame, tunggu interval habis
                scanTimer -= dt;
                if (scanTimer <= 0f)
                {
                    FindBuildingTarget();
                    scanTimer = ScanInterval;
                }
            }

            // Jika punya target bangunan (Barricade/Turret), serang itu dulu
            if (buildingTarget != null && buildingTargetCollider != null)
            {
                Vector2 targetPoint = buildingTargetCollider.ClosestPoint(body.position);
                Vector2 targetDelta = targetPoint - body.position;
                
                if (targetDelta.sqrMagnitude <= attackRangeSqr)
                {
                    MoveDirection = targetDelta.normalized;
                    body.linearVelocity = Vector2.zero;
                    if (cooldown.TryAttack()) 
                    {
                        VFXManager.Instance.PlayVFX(VFXType.EnemyAttack, targetPoint);
                        AudioManager.Instance.PlaySFX(SFXType.EnemyAttack); // Play damage sound effect when attacking a building
                        buildingTarget.TakeDamage(definition.Damage);
                    }
                    return;
                }

                MoveDirection = targetDelta.normalized;
                body.linearVelocity = MoveDirection * definition.MoveSpeed;
                return;
            }

            // if no building target found, move towards the goal
            Vector2 goalPoint = goalCollider.ClosestPoint(body.position);
            if ((goalPoint - body.position).sqrMagnitude <= attackRangeSqr)
            {
                MoveDirection = (goalPoint - body.position).normalized;
                body.linearVelocity = Vector2.zero;
                if (cooldown.TryAttack()) 
                {
                    VFXManager.Instance.PlayVFX(VFXType.EnemyAttack, goalPoint);
                    AudioManager.Instance.PlaySFX(SFXType.EnemyAttack);
                    goal.TakeDamage(definition.Damage);
                }
                return;
            }

            // Pathfinding menggunakan Waypoints (jika ada)
            while (waypoints != null && waypointIndex < waypoints.Length)
            {
                Transform point = waypoints[waypointIndex];
                if (point != null && ((Vector2)point.position - body.position).sqrMagnitude > 0.04f) break;
                waypointIndex++;
            }

            Vector2 destination = waypoints != null && waypointIndex < waypoints.Length
                ? (Vector2)waypoints[waypointIndex].position : goalPoint;
            
            Vector2 delta = destination - body.position;
            
            // OPTIMIZATION 3: Gunakan sqrMagnitude daripada memanggil .magnitude (operasi Sqrt yang mahal)
            if (delta.sqrMagnitude <= 0.000001f)
            {
                StopMovement();
                return;
            }

            MoveDirection = delta.normalized;
            body.linearVelocity = MoveDirection * definition.MoveSpeed;
        }

        private void FindBuildingTarget()
        {
            int hitCount = Physics2D.OverlapCircle(body.position, definition.AttackRange + buildingScanPadding, scanFilter, hitBuffer);
            float nearestSqrDistance = float.PositiveInfinity;

            for (int i = 0; i < hitCount; i++)
            {
                Collider2D hit = hitBuffer[i];
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
            
            // prevent memory leak by clearing the buffer after use
            Array.Clear(hitBuffer, 0, hitCount);
        }

        private bool IsValidBuildingTarget(Health target, Collider2D targetCollider)
        {
            if (target == null || targetCollider == null || !target.IsAlive) return false;
            ConstructionSite site = target.GetComponentInParent<ConstructionSite>();
            return site != null && site.IsBuilt;
        }

        private float DistanceToSqr(Collider2D targetCollider)
        {
            if (targetCollider == null) return float.PositiveInfinity;
            return (body.position - (Vector2)targetCollider.ClosestPoint(body.position)).sqrMagnitude;
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
            
            AudioManager.Instance.PlaySFX(SFXType.EnemyDeath);
            VFXManager.Instance.PlayVFX(VFXType.EnemyDeath, transform.position);
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