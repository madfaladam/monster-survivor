using System.Collections.Generic;
using MonsterSurvivor.Construction;
using UnityEngine;

namespace MonsterSurvivor.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerInventory))]
    public sealed class PlayerBuildInteractor : MonoBehaviour
    {
        private readonly HashSet<ConstructionZone> candidates = new HashSet<ConstructionZone>();
        private PlayerInventory inventory;
        private ConstructionZone activeZone;

        private void Awake() => inventory = GetComponent<PlayerInventory>();

        public void AddCandidate(ConstructionZone zone)
        {
            if (zone != null)
                candidates.Add(zone);
        }

        public void RemoveCandidate(ConstructionZone zone)
        {
            candidates.Remove(zone);
            if (activeZone == zone)
                CancelActive();
        }

        private void Update()
        {
            if (Time.deltaTime <= 0f)
                return;

            if (activeZone != null)
            {
                ConstructionSite site = activeZone.Site;
                if (!IsAvailable(activeZone) || !site.IsBuilding)
                    CancelActive();
                else
                {
                    // Keep the current site until completion or exit: no switching mid-build.
                    site.Advance(Time.deltaTime);
                    if (site.IsBuilt)
                        activeZone = null;
                    return;
                }
            }

            ConstructionZone nearest = null;
            float nearestDistance = float.PositiveInfinity;
            foreach (ConstructionZone zone in candidates)
            {
                if (!IsAvailable(zone) || zone.Site.IsBuilding)
                    continue;

                float distance = ((Vector2)(zone.transform.position - transform.position)).sqrMagnitude;
                if (distance < nearestDistance)
                {
                    nearest = zone;
                    nearestDistance = distance;
                }
            }

            if (nearest != null && nearest.Site.TryBegin(inventory.Wallet))
                activeZone = nearest;
        }

        private static bool IsAvailable(ConstructionZone zone) =>
            zone != null && zone.isActiveAndEnabled && zone.Site != null &&
            zone.Site.isActiveAndEnabled && !zone.Site.IsBuilt;

        private void CancelActive()
        {
            ConstructionZone zone = activeZone;
            activeZone = null;
            if (zone != null && zone.Site != null)
                zone.Site.Cancel();
        }

        private void OnDisable()
        {
            CancelActive();
            candidates.Clear();
        }
    }
}
