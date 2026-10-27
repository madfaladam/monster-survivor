using System.Collections;
using UnityEngine;

namespace MonsterSurvivor.Combat
{
    public sealed class WaveDirector : MonoBehaviour
    {
        [SerializeField] private CombatEncounter encounter;
        [SerializeField] private EnemyGate[] gates;
        [SerializeField, Min(1)] private int[] enemiesPerWave = { 3, 5, 8 };
        [SerializeField, Min(0f)] private float spawnInterval = 0.5f;
        //[SerializeField, Min(0f)] private float waveDelay = 2f;

        private Coroutine routine;
        private int currentWave;
        private int pendingSpawns;
        private int aliveEnemies;
        private bool running;

        public int CurrentWave => Mathf.Min(currentWave + 1, enemiesPerWave.Length);
        public int TotalWaves => enemiesPerWave.Length;
        public bool IsRunning => running;
        public int CompletedWaves => currentWave;
        public bool IsFinished => currentWave >= TotalWaves;

        public event System.Action Changed;

        public void BeginWaves()
        {
            if (running || encounter == null || gates == null || gates.Length == 0 ||
                enemiesPerWave == null || enemiesPerWave.Length == 0 ||
                currentWave >= enemiesPerWave.Length)
                return;

            running = true;
            aliveEnemies = 0;
            pendingSpawns = 0;
            routine = StartCoroutine(RunWaves());
            Changed?.Invoke();
        }

        private IEnumerator RunWaves()
        {
            while (currentWave < enemiesPerWave.Length && running)
            {
                pendingSpawns = enemiesPerWave[currentWave];

                while (pendingSpawns > 0 && running)
                {
                    EnemyGate randomGate = gates[Random.Range(0, gates.Length)];
                    EnemyController enemy = encounter.SpawnEnemyFromGate(randomGate);
                    
                    if (enemy != null)
                        aliveEnemies++;

                    pendingSpawns--;

                    if (spawnInterval > 0f)
                        yield return new WaitForSeconds(spawnInterval);
                    else
                        yield return null;
                }

                while (aliveEnemies > 0 && running)
                    yield return null;

                if (!running)
                    yield break;

                // wave done
                currentWave++;
                Changed?.Invoke();
                running = false;
                routine = null;

                encounter.CompleteWaves();
                yield break;
            }

            running = false;
            routine = null;
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
            Changed?.Invoke();
        }
    }
}