using UnityEngine;

namespace MonsterSurvivor.Combat
{
    [CreateAssetMenu(menuName = "Monster Survivor/Turret")]
    public sealed class TurretDefinition : ScriptableObject
    {
        [SerializeField, Min(0.1f)] private float range = 4.5f;
        [SerializeField, Min(1)] private int damage = 15;
        [SerializeField, Min(0.1f)] private float attackInterval = 0.75f;

        public float Range => range;
        public int Damage => damage;
        public float AttackInterval => attackInterval;
    }
}
