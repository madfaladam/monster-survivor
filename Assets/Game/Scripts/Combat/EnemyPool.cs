using System.Collections.Generic;
using UnityEngine;

namespace MonsterSurvivor.Combat
{
    public sealed class EnemyPool : MonoBehaviour
    {
        [SerializeField] private EnemyController prefab;
        [SerializeField, Min(1)] private int initialSize = 10;

        private readonly Queue<EnemyController> available = new();
        private readonly HashSet<EnemyController> availableSet = new();
        private readonly HashSet<EnemyController> pooled = new();

        private void Awake()
        {
            if (prefab == null)
            {
                Debug.LogError("Enemy prefab is not assigned.", this);
                enabled = false;
                return;
            }

            for (int i = 0; i < initialSize; i++)
                CreateEnemy();
        }

        private EnemyController CreateEnemy()
        {
            EnemyController enemy = Instantiate(prefab, transform);
            enemy.gameObject.SetActive(false);
            enemy.Removed += HandleEnemyRemoved;

            pooled.Add(enemy);
            available.Enqueue(enemy);
            availableSet.Add(enemy);

            return enemy;
        }

        public EnemyController Spawn(
            Vector3 position,
            Health target,
            Collider2D targetCollider,
            Transform[] waypoints)
        {
            if (!enabled || target == null || targetCollider == null)
                return null;

            EnemyController enemy = null;

            while (available.Count > 0 && enemy == null)
            {
                enemy = available.Dequeue();
                availableSet.Remove(enemy);

                if (enemy == null)
                    continue;

                if (!enemy.gameObject)
                    enemy = null;
            }

            if (enemy == null)
                enemy = CreateEnemy();

            availableSet.Remove(enemy);
            enemy.transform.SetPositionAndRotation(
                position, Quaternion.identity);

            enemy.gameObject.SetActive(true);
            enemy.PrepareForSpawn(target, targetCollider, waypoints);

            return enemy;
        }

        private void HandleEnemyRemoved(EnemyController enemy)
        {
            if (enemy == null || !pooled.Contains(enemy))
                return;

            if (availableSet.Add(enemy))
                available.Enqueue(enemy);

            enemy.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            foreach (EnemyController enemy in pooled)
            {
                if (enemy != null)
                    enemy.Removed -= HandleEnemyRemoved;
            }
        }
    }
}