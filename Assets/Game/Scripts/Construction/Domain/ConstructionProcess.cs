using System;
using MonsterSurvivor.Resources;

namespace MonsterSurvivor.Construction
{
    public sealed class ConstructionProcess
    {
        private readonly int woodCost;
        private readonly int stoneCost;
        private readonly float duration;
        private IResourceWallet reservedWallet;
        private float elapsed;

        public bool IsBuilding => reservedWallet != null;
        public bool IsCompleted { get; private set; }
        public float Progress => Math.Min(elapsed / duration, 1f);

        public ConstructionProcess(int woodCost, int stoneCost, float duration)
        {
            if (woodCost < 0) throw new ArgumentOutOfRangeException(nameof(woodCost));
            if (stoneCost < 0) throw new ArgumentOutOfRangeException(nameof(stoneCost));
            if (duration <= 0f || float.IsNaN(duration) || float.IsInfinity(duration))
                throw new ArgumentOutOfRangeException(nameof(duration));

            this.woodCost = woodCost;
            this.stoneCost = stoneCost;
            this.duration = duration;
        }

        public bool TryBegin(IResourceWallet wallet)
        {
            if (wallet == null) throw new ArgumentNullException(nameof(wallet));
            if (IsBuilding || IsCompleted || !wallet.TrySpend(woodCost, stoneCost))
                return false;

            reservedWallet = wallet;
            return true;
        }

        public void Advance(float deltaTime)
        {
            if (deltaTime < 0f || float.IsNaN(deltaTime) || float.IsInfinity(deltaTime))
                throw new ArgumentOutOfRangeException(nameof(deltaTime));
            if (!IsBuilding)
                return;

            elapsed = Math.Min(elapsed + deltaTime, duration);
            if (elapsed >= duration)
            {
                IsCompleted = true;
                reservedWallet = null;
            }
        }

        public void Cancel()
        {
            if (!IsBuilding)
                return;

            IResourceWallet wallet = reservedWallet;
            reservedWallet = null;
            elapsed = 0f;
            wallet.Add(ResourceType.Wood, woodCost);
            wallet.Add(ResourceType.Stone, stoneCost);
        }
    }
}
