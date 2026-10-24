using System.Collections;
using UnityEngine;

namespace MonsterSurvivor.Combat
{
    public sealed class WaveDirector : MonoBehaviour
    {
        [SerializeField] private CombatEncounter encounter;
        [SerializeField] private EnemyGate gate;
        [SerializeField, Min(1)] private int[] enemiesPerWave = { 3, 5, 8 };
        [SerializeField, Min(0f)] private float spawnInterval = 0.5f;
        [SerializeField, Min(0f)] private float waveDelay = 2f;

        private Coroutine routine;
        private int currentWave;
        private int pendingSpawns;
        private int aliveEnemies;
        private bool running;

        public int CurrentWave => currentWave + 1;
        public int TotalWaves => enemiesPerWave.Length;

        public void BeginWaves()
        {
            if (running || encounter == null || gate == null ||
                enemiesPerWave == null || enemiesPerWave.Length == 0)
                return;

            running = true;
            currentWave = 0;
            aliveEnemies = 0;
            pendingSpawns = 0;
            routine = StartCoroutine(RunWaves());
        }

        private IEnumerator RunWaves()
        {
            while (currentWave < enemiesPerWave.Length && running)
            {
                pendingSpawns = enemiesPerWave[currentWave];

                while (pendingSpawns > 0 && running)
                {
                    EnemyController enemy = encounter.SpawnEnemyFromGate(gate);

                    // Count enemies only when they were successfully spawned.
                    if (enemy != null)
                        aliveEnemies ++;

                    pendingSpawns--;

                    if (spawnInterval > 0f)
                        yield return new WaitForSeconds(spawnInterval);
                    else
                        yield return null;
                }

                while (aliveEnemies > 0 && running)
                    yield return null;

                currentWave++;

                if (currentWave < enemiesPerWave.Length && running &&
                    waveDelay > 0f)
                    yield return new WaitForSeconds(waveDelay);
            }

            bool completed = running && currentWave >= enemiesPerWave.Length;
            running = false;
            routine = null;

            if (completed && encounter != null)
                encounter.CompleteWaves();
        }

        public void NotifyEnemyRemoved()
        {
            if (aliveEnemies > 0)
                aliveEnemies--;
        }

        public void StopWaves()
        {
            running = false;

            if (routine != null)
                StopCoroutine(routine);

            routine = null;
        }
    }
}