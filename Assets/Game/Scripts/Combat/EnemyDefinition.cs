using UnityEngine;

namespace MonsterSurvivor.Combat
{
    [CreateAssetMenu(menuName = "Monster Survivor/Enemy")]
    public sealed class EnemyDefinition : ScriptableObject
    {
        [SerializeField, Min(0.1f)] private float moveSpeed = 1.5f;
        [SerializeField, Min(1)] private int damage = 10;
        [SerializeField, Min(0.1f)] private float attackInterval = 1f;
        [SerializeField, Min(0.1f)] private float attackRange = 0.45f;

        public float MoveSpeed => moveSpeed;
        public int Damage => damage;
        public float AttackInterval => attackInterval;
        public float AttackRange => attackRange;
    }
}
