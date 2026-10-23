using MonsterSurvivor.Resources;
using UnityEngine;

namespace MonsterSurvivor.Production
{
    [CreateAssetMenu(menuName = "Monster Survivor/Production")]
    public sealed class ProductionDefinition : ScriptableObject
    {
        [SerializeField] private ResourceType resourceType;
        [SerializeField, Min(0.1f)] private float interval = 3f;
        [SerializeField, Min(1)] private int capacity = 6;

        public ResourceType ResourceType => resourceType;
        public float Interval => interval;
        public int Capacity => capacity;
    }
}
