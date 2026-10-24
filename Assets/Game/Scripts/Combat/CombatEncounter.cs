using System;
using System.Collections.Generic;
using MonsterSurvivor.Player;
using UnityEngine;

namespace MonsterSurvivor.Combat
{
    public enum EncounterState { Ready, Active, Cleared, Defeated, Finished }

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
        [SerializeField] private WaveDirector waveDirector;
        private readonly EnemyRoster roster = new EnemyRoster();
        private readonly HashSet<TurretController> registeredTurrets = new HashSet<TurretController>();
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
                RegisterTurret(turret);
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
            
            foreach (TurretController turret in registeredTurrets)
                turret.SetCombatEnabled(true);

            if (waveDirector != null)
            {
                waveDirector.BeginWaves();
                Changed?.Invoke();
                return;
            }

            foreach (EnemyController enemy in enemies)
            {
                if (enemy != null && enemy.IsAlive)
                {
                    roster.Add(enemy);
                    enemy.BeginCombat();
                }
            }

            if (roster.Count == 0)
                Finish(EncounterState.Cleared);
            else
                Changed?.Invoke();
        }
        public EnemyController SpawnEnemyFromGate(EnemyGate gate)
        {
            if (State != EncounterState.Active ||
                gate == null ||
                generator == null ||
                !generator.IsAlive)
                return null;

            EnemyController enemy = gate.SpawnEnemy(
                generator,
                generatorCollider);

            if (enemy != null)
                RegisterSpawnedEnemy(enemy);

            return enemy;
        }

        public void RegisterSpawnedEnemy(EnemyController enemy)
        {
            if (enemy == null || State != EncounterState.Active)
                return;

            roster.Add(enemy);
            enemy.Removed -= HandleEnemyRemoved;
            enemy.Removed += HandleEnemyRemoved;

            if (playerCollider != null &&
                enemy.TryGetComponent(out Collider2D enemyCollider))
            {
                Physics2D.IgnoreCollision(playerCollider, enemyCollider);
            }

            enemy.BeginCombat();

            Changed?.Invoke();
        }

        public void RegisterTurret(TurretController turret)
        {
            if (turret == null || !registeredTurrets.Add(turret))
                return;

            turret.Initialize(roster);
            turret.SetCombatEnabled(State == EncounterState.Active);
        }

        public void CompleteWaves()
        {
            if (State != EncounterState.Active)
                return;

            StopActors();

            if (generator == null || !generator.IsAlive)
            {
                Finish(EncounterState.Defeated);
                return;
            }

            if (waveDirector != null && waveDirector.IsFinished)
            {
                State = EncounterState.Finished;

                if (builder != null)
                    builder.enabled = false;
            }
            else
            {
                State = EncounterState.Ready;

                if (builder != null)
                    builder.enabled = true;
            }

            Changed?.Invoke();
        }
        
        private void HandleEnemyRemoved(EnemyController enemy)
        {
            if (!roster.Remove(enemy))
                return;

            if (enemy != null)
                enemy.Removed -= HandleEnemyRemoved;

            if (State != EncounterState.Active)
                return;

            if (waveDirector != null)
            {
                waveDirector.NotifyEnemyRemoved();

                if (generator == null || !generator.IsAlive)
                    Finish(EncounterState.Defeated);

                Changed?.Invoke();
                return;
            }

            if (generator == null || !generator.IsAlive)
                Finish(EncounterState.Defeated);
            else if (roster.Count == 0)
                Finish(EncounterState.Cleared);
            else
                Changed?.Invoke();
        }


        private void HandleGeneratorDeath() => Finish(EncounterState.Defeated);

        private void Finish(EncounterState result)
        {
            if (State == EncounterState.Defeated)
                return;

            State = result;

            if (waveDirector != null)
                waveDirector.StopWaves();

            StopActors();
            builder.enabled = true;
            Changed?.Invoke();
        }

        private void StopActors()
        {
            roster.StopAllCombat();

            foreach (EnemyController enemy in enemies)
                if (enemy != null)
                    enemy.StopCombat();

            foreach (TurretController turret in registeredTurrets)
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
