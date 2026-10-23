using System;
using MonsterSurvivor.Player;
using UnityEngine;

namespace MonsterSurvivor.Combat
{
    public enum EncounterState { Ready, Active, Cleared, Defeated }

    [DisallowMultipleComponent]
    public sealed class CombatEncounter : MonoBehaviour
    {
        [SerializeField] private Health generator;
        [SerializeField] private Collider2D generatorCollider;
        [SerializeField] private Collider2D playerCollider;
        [SerializeField] private PlayerBuildInteractor builder;
        [SerializeField] private EnemyController[] enemies;
        [SerializeField] private TurretController[] turrets;
        [SerializeField] private Transform[] waypoints;

        private readonly EnemyRoster roster = new EnemyRoster();
        private bool initialized;

        public EncounterState State { get; private set; }
        public int RemainingEnemies => roster.Count;
        public event Action Changed;

        private void Start()
        {
            generator.Died += HandleGeneratorDeath;
            foreach (EnemyController enemy in enemies)
            {
                enemy.Initialize(generator, generatorCollider, waypoints);
                enemy.Removed += HandleEnemyRemoved;
                // The player cannot indefinitely body-block enemies without taking damage.
                if (playerCollider != null && enemy.TryGetComponent(out Collider2D enemyCollider))
                    Physics2D.IgnoreCollision(playerCollider, enemyCollider);
            }
            foreach (TurretController turret in turrets)
                turret.Initialize(roster);
            initialized = true;
            Changed?.Invoke();
        }

        public void Begin()
        {
            if (!initialized || !isActiveAndEnabled || State != EncounterState.Ready)
                return;
            if (!generator.IsAlive)
            {
                Finish(EncounterState.Defeated);
                return;
            }

            State = EncounterState.Active;
            // Disabling the interactor cancels and refunds any unfinished construction.
            builder.enabled = false;
            foreach (EnemyController enemy in enemies)
                if (enemy != null && enemy.IsAlive)
                {
                    roster.Add(enemy);
                    enemy.BeginCombat();
                }
            foreach (TurretController turret in turrets)
                turret.SetCombatEnabled(true);

            if (roster.Count == 0)
                Finish(EncounterState.Cleared);
            else
                Changed?.Invoke();
        }

        private void HandleEnemyRemoved(EnemyController enemy)
        {
            if (!roster.Remove(enemy) || State != EncounterState.Active)
                return;
            if (!generator.IsAlive)
                Finish(EncounterState.Defeated);
            else if (roster.Count == 0)
                Finish(EncounterState.Cleared);
            else
                Changed?.Invoke();
        }

        private void HandleGeneratorDeath() => Finish(EncounterState.Defeated);

        private void Finish(EncounterState result)
        {
            // Generator death takes priority even if a clear was reported in this frame.
            if (State == EncounterState.Defeated)
                return;
            State = result;
            StopActors();
            builder.enabled = true;
            Changed?.Invoke();
        }

        private void StopActors()
        {
            foreach (EnemyController enemy in enemies)
                if (enemy != null)
                    enemy.StopCombat();
            foreach (TurretController turret in turrets)
                if (turret != null)
                    turret.SetCombatEnabled(false);
        }

        private void OnDisable()
        {
            if (initialized)
            {
                StopActors();
                if (builder != null)
                    builder.enabled = true;
            }
        }

        private void OnDestroy()
        {
            if (!initialized)
                return;
            if (generator != null)
                generator.Died -= HandleGeneratorDeath;
            foreach (EnemyController enemy in enemies)
                if (enemy != null)
                    enemy.Removed -= HandleEnemyRemoved;
        }
    }
}
