using System.Collections.Generic;
using UnityEngine;

namespace MonsterSurvivor.Combat
{
    public sealed class EnemyRoster : IEnemyTargetProvider
    {
        private readonly List<EnemyController> enemies = new List<EnemyController>();
        public int Count => enemies.Count;

        public void Add(EnemyController enemy)
        {
            if (!enemies.Contains(enemy))
                enemies.Add(enemy);
        }

        public bool Remove(EnemyController enemy) => enemies.Remove(enemy);

        public EnemyController FindNearest(Vector2 origin, float range)
        {
            EnemyController nearest = null;
            float nearestDistance = range * range;
            for (int i = 0; i < enemies.Count; i++)
            {
                EnemyController enemy = enemies[i];
                if (enemy == null || !enemy.IsAlive || !enemy.IsCombatActive)
                    continue;
                float distance = ((Vector2)enemy.transform.position - origin).sqrMagnitude;
                if (distance <= nearestDistance)
                {
                    nearest = enemy;
                    nearestDistance = distance;
                }
            }
            return nearest;
        }
    }
}
