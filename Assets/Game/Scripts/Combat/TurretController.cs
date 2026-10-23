using MonsterSurvivor.Construction;
using UnityEngine;

namespace MonsterSurvivor.Combat
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ConstructionSite), typeof(ShotTracer))]
    public sealed class TurretController : MonoBehaviour
    {
        [SerializeField] private TurretDefinition definition;
        private ConstructionSite site;
        private ShotTracer tracer;
        private IEnemyTargetProvider targets;
        private AttackCooldown cooldown;
        private bool combatEnabled;

        private void Awake()
        {
            site = GetComponent<ConstructionSite>();
            tracer = GetComponent<ShotTracer>();
            if (definition == null)
            {
                Debug.LogError("Assign a turret definition.", this);
                enabled = false;
                return;
            }
            cooldown = new AttackCooldown(definition.AttackInterval);
        }

        public void Initialize(IEnemyTargetProvider targetProvider) => targets = targetProvider;
        public void SetCombatEnabled(bool value) => combatEnabled = value;

        private void Update()
        {
            if (!combatEnabled || !site.isActiveAndEnabled || !site.IsBuilt ||
                targets == null || Time.deltaTime <= 0f)
                return;

            cooldown.Advance(Time.deltaTime);
            EnemyController target = targets.FindNearest(transform.position, definition.Range);
            if (target == null || !cooldown.TryAttack())
                return;

            tracer.Show((Vector2)transform.position + Vector2.up * 0.8f,
                (Vector2)target.transform.position + Vector2.up * 0.5f);
            target.TakeDamage(definition.Damage);
        }

        private void OnDrawGizmosSelected()
        {
            if (definition == null)
                return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, definition.Range);
        }
    }
}
