using UnityEngine;

namespace MonsterSurvivor.Construction
{
    [CreateAssetMenu(menuName = "Monster Survivor/Building")]
    public sealed class BuildingDefinition : ScriptableObject
    {
        [SerializeField] private string displayName;
        [SerializeField, Min(0)] private int woodCost = 5;
        [SerializeField, Min(0)] private int stoneCost = 3;
        [SerializeField, Min(0.1f)] private float buildDuration = 3f;

        public string DisplayName => displayName;
        public int WoodCost => woodCost;
        public int StoneCost => stoneCost;
        public float BuildDuration => buildDuration;
    }
}
