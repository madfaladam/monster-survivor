using System;

namespace MonsterSurvivor.Resources
{
    public sealed class ResourceWallet : IResourceWallet
    {
        private int wood;
        private int stone;

        public event Action Changed;

        public ResourceWallet(int initialWood, int initialStone)
        {
            ValidateAmount(initialWood, nameof(initialWood));
            ValidateAmount(initialStone, nameof(initialStone));
            wood = initialWood;
            stone = initialStone;
        }

        public int GetAmount(ResourceType type)
        {
            switch (type)
            {
                case ResourceType.Wood: return wood;
                case ResourceType.Stone: return stone;
                default: throw new ArgumentOutOfRangeException(nameof(type));
            }
        }

        public void Add(ResourceType type, int amount)
        {
            ValidateAmount(amount, nameof(amount));
            switch (type)
            {
                case ResourceType.Wood: wood = checked(wood + amount); break;
                case ResourceType.Stone: stone = checked(stone + amount); break;
                default: throw new ArgumentOutOfRangeException(nameof(type));
            }

            if (amount > 0)
                Changed?.Invoke();
        }

        public bool TrySpend(int woodCost, int stoneCost)
        {
            ValidateAmount(woodCost, nameof(woodCost));
            ValidateAmount(stoneCost, nameof(stoneCost));
            if (wood < woodCost || stone < stoneCost)
                return false;

            wood -= woodCost;
            stone -= stoneCost;
            if (woodCost > 0 || stoneCost > 0)
                Changed?.Invoke();
            return true;
        }

        private static void ValidateAmount(int amount, string name)
        {
            if (amount < 0)
                throw new ArgumentOutOfRangeException(name, "Resource amounts cannot be negative.");
        }
    }
}
