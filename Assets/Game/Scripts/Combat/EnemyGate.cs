using UnityEngine;

namespace MonsterSurvivor.Combat
{
    public sealed class EnemyGate : MonoBehaviour
    {
        [SerializeField] private EnemyPool pool;
        [SerializeField] private Transform[] waypoints;

        public EnemyController SpawnEnemy(
            Health target,
            Collider2D targetCollider)
        {
            if (pool == null)
            {
                Debug.LogError("Enemy pool is not assigned.", this);
                return null;
            }

            return pool.Spawn(
                transform.position,
                target,
                targetCollider,
                waypoints);
        }
    }
}