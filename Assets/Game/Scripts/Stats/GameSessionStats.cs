using MonsterSurvivor.Combat;
using MonsterSurvivor.Player;
using MonsterSurvivor.Resources;
using UnityEngine;

namespace MonsterSurvivor.Stats
{
    [DisallowMultipleComponent]
    public sealed class GameSessionStats : MonoBehaviour
    {
        [SerializeField] private CombatEncounter encounter;
        [SerializeField] private PlayerInventory inventory;

        public int TurretsBuilt { get; private set; }
        public int TurretsDestroyed { get; private set; }
        public int BarricadesBuilt { get; private set; }
        public int BarricadesDestroyed { get; private set; }
        public int EnemiesKilled { get; private set; }
        public int TotalWood { get; private set; }
        public int TotalStone { get; private set; }

        private IResourceWallet wallet;
        private int lastWood;
        private int lastStone;
        private bool started;
        private bool walletSubscribed;
        private bool encounterSubscribed;
        private bool resourceTotalsInitialized;

        private void Start()
        {
            started = true;
            Subscribe();
        }

        private void OnEnable()
        {
            if (started)
                Subscribe();
        }

        private void Subscribe()
        {
            IResourceWallet currentWallet = inventory != null ? inventory.Wallet : null;
            if (currentWallet != wallet)
            {
                if (walletSubscribed && wallet != null)
                    wallet.Changed -= HandleWalletChanged;

                wallet = currentWallet;
                walletSubscribed = false;
                resourceTotalsInitialized = false;
            }

            if (wallet != null && !walletSubscribed)
            {
                if (!resourceTotalsInitialized)
                {
                    lastWood = wallet.GetAmount(ResourceType.Wood);
                    lastStone = wallet.GetAmount(ResourceType.Stone);
                    TotalWood = lastWood;
                    TotalStone = lastStone;
                    resourceTotalsInitialized = true;
                }

                wallet.Changed += HandleWalletChanged;
                walletSubscribed = true;
            }

            if (encounter != null && !encounterSubscribed)
            {
                encounter.EnemyKilled += HandleEnemyKilled;
                encounterSubscribed = true;
            }
        }

        private void HandleWalletChanged()
        {
            if (wallet == null)
                return;

            int wood = wallet.GetAmount(ResourceType.Wood);
            int stone = wallet.GetAmount(ResourceType.Stone);

            // Totals track resources acquired, not resources remaining after construction costs.
            if (wood > lastWood)
                TotalWood += wood - lastWood;
            if (stone > lastStone)
                TotalStone += stone - lastStone;

            lastWood = wood;
            lastStone = stone;
        }

        private void HandleEnemyKilled(EnemyController enemy) => EnemiesKilled++;

        public void RecordBuildingBuilt(BuildingStatsReporter.BuildingKind kind)
        {
            if (kind == BuildingStatsReporter.BuildingKind.Turret)
                TurretsBuilt++;
            else if (kind == BuildingStatsReporter.BuildingKind.Barricade)
                BarricadesBuilt++;
        }

        public void RecordBuildingDestroyed(BuildingStatsReporter.BuildingKind kind)
        {
            if (kind == BuildingStatsReporter.BuildingKind.Turret)
                TurretsDestroyed++;
            else if (kind == BuildingStatsReporter.BuildingKind.Barricade)
                BarricadesDestroyed++;
        }

        private void OnDisable()
        {
            if (walletSubscribed && wallet != null)
                wallet.Changed -= HandleWalletChanged;
            walletSubscribed = false;

            if (encounterSubscribed && encounter != null)
                encounter.EnemyKilled -= HandleEnemyKilled;
            encounterSubscribed = false;
        }
    }
}
