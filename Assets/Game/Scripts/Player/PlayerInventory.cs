using MonsterSurvivor.Resources;
using UnityEngine;

namespace MonsterSurvivor.Player
{
    [DisallowMultipleComponent]
    public sealed class PlayerInventory : MonoBehaviour
    {
        [SerializeField, Min(0)] private int initialWood = 10;
        [SerializeField, Min(0)] private int initialStone = 10;

        public IResourceWallet Wallet { get; private set; }

        private void Awake()
        {
            Wallet = new ResourceWallet(initialWood, initialStone);
        }
    }
}
