using System;

namespace MonsterSurvivor.Resources
{
    public interface IResourceWallet
    {
        event Action Changed;
        int GetAmount(ResourceType type);
        void Add(ResourceType type, int amount);
        bool TrySpend(int woodCost, int stoneCost);
    }
}
