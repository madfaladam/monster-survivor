using System;
using MonsterSurvivor.Resources;

namespace MonsterSurvivor.Production
{
    public sealed class ProductionBuffer
    {
        private readonly double interval;
        private double elapsed;

        public int Count { get; private set; }
        public int Capacity { get; }
        public event Action Changed;

        public ProductionBuffer(float interval, int capacity)
        {
            if (interval <= 0f || float.IsNaN(interval) || float.IsInfinity(interval))
                throw new ArgumentOutOfRangeException(nameof(interval));
            if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));

            this.interval = interval;
            Capacity = capacity;
        }

        public void Advance(float deltaTime)
        {
            if (deltaTime < 0f || float.IsNaN(deltaTime) || float.IsInfinity(deltaTime))
                throw new ArgumentOutOfRangeException(nameof(deltaTime));
            if (Count == Capacity)
                return;

            elapsed += deltaTime;
            int produced = (int)Math.Min(Capacity - Count, Math.Floor(elapsed / interval));
            if (produced == 0)
                return;

            Count += produced;
            // A full store discards excess time; collecting starts a fresh interval.
            elapsed = Count == Capacity ? 0d : elapsed - produced * interval;
            Changed?.Invoke();
        }

        public int Collect(IResourceWallet wallet, ResourceType type)
        {
            if (wallet == null) throw new ArgumentNullException(nameof(wallet));
            int amount = Math.Min(Count, int.MaxValue - wallet.GetAmount(type));
            if (amount == 0)
                return 0;

            Count -= amount;
            wallet.Add(type, amount);
            Changed?.Invoke();
            return amount;
        }
    }
}
