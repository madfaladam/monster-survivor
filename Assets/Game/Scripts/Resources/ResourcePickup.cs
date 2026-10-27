using MonsterSurvivor.Audio;
using MonsterSurvivor.VFX;
using UnityEngine;

namespace MonsterSurvivor.Resources
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider2D))]
    public sealed class ResourcePickup : MonoBehaviour
    {
        [SerializeField] private ResourceType resourceType;
        [SerializeField, Min(1)] private int amount = 1;

        private bool collected;

        public bool TryCollect(IResourceWallet wallet)
        {
            if (collected || !isActiveAndEnabled || wallet == null || amount <= 0)
                return false;

            if (wallet.GetAmount(resourceType) > int.MaxValue - amount)
                return false;

            // Consume before notifying wallet observers so callbacks cannot collect twice.
            collected = true;
            gameObject.SetActive(false);

            AudioManager.Instance.PlaySFX(SFXType.ResourceCollected);
            VFXManager.Instance.PlayVFX(VFXType.ResourceCollected, transform.position);
            
            wallet.Add(resourceType, amount);
            return true;
        }

        public void Restore()
        {
            if (!collected)
                return;

            collected = false;
            gameObject.SetActive(true);
        }
    }
}
