using MonsterSurvivor.Player;
using UnityEngine;

namespace MonsterSurvivor.Production
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class ProductionCollectZone : MonoBehaviour
    {
        [SerializeField] private ProductionBuilding building;

        private void OnTriggerEnter2D(Collider2D other) => Collect(other);
        private void OnTriggerStay2D(Collider2D other) => Collect(other);

        private void Collect(Collider2D other)
        {
            if (isActiveAndEnabled && building != null &&
                other.TryGetComponent(out PlayerInventory inventory))
                building.Collect(inventory.Wallet);
        }
    }
}
