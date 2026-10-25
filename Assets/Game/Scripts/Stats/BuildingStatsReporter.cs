using MonsterSurvivor.Combat;
using MonsterSurvivor.Construction;
using UnityEngine;

namespace MonsterSurvivor.Stats
{
    [DisallowMultipleComponent]
    public sealed class BuildingStatsReporter : MonoBehaviour
    {
        public enum BuildingKind { Other, Turret, Barricade }

        private ConstructionSite site;
        private Health health;
        private GameSessionStats stats;
        private BuildingKind kind;
        private bool countedBuilt;
        private bool countedDestroyed;

        public void Initialize(ConstructionSite constructionSite, GameSessionStats sessionStats)
        {
            site = constructionSite;
            stats = sessionStats;

            if (site == null)
            {
                Debug.LogWarning("BuildingStatsReporter requires a ConstructionSite.", this);
                return;
            }

            if (GetComponentInChildren<TurretController>(true) != null)
                kind = BuildingKind.Turret;
            else if (site.Definition != null &&
                     site.Definition.DisplayName.ToLowerInvariant().Contains("barricade"))
                kind = BuildingKind.Barricade;
            else
                kind = BuildingKind.Other;

            health = GetComponentInChildren<Health>(true);
            site.Changed += HandleConstructionChanged;
            if (health != null)
                health.Died += HandleHealthDeath;

            if (site.IsBuilt)
                HandleConstructionChanged();
        }

        private void HandleConstructionChanged()
        {
            if (countedBuilt || site == null || !site.IsBuilt)
                return;

            countedBuilt = true;
            if (stats != null)
                stats.RecordBuildingBuilt(kind);
        }

        private void HandleHealthDeath()
        {
            if (!countedBuilt || countedDestroyed)
                return;

            countedDestroyed = true;
            if (stats != null)
                stats.RecordBuildingDestroyed(kind);
        }

        private void OnDestroy()
        {
            if (site != null)
                site.Changed -= HandleConstructionChanged;
            if (health != null)
                health.Died -= HandleHealthDeath;

            // Destroyed buildings count only after construction completed.
            if (countedBuilt && !countedDestroyed && Application.isPlaying)
            {
                countedDestroyed = true;
                if (stats != null)
                    stats.RecordBuildingDestroyed(kind);
            }
        }
    }
}
