using MonsterSurvivor.Resources;
using UnityEngine;

namespace MonsterSurvivor.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerInventory))]
    public sealed class PlayerCollector : MonoBehaviour
    {
        private PlayerInventory inventory;

        private void Awake() => inventory = GetComponent<PlayerInventory>();

        private void OnTriggerEnter2D(Collider2D other) => Collect(other);
        private void OnTriggerStay2D(Collider2D other) => Collect(other);

        private void Collect(Collider2D other)
        {
            if (isActiveAndEnabled && other.TryGetComponent(out ResourcePickup pickup))
                pickup.TryCollect(inventory.Wallet);
        }
    }
}
