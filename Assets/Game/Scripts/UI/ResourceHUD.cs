using MonsterSurvivor.Player;
using MonsterSurvivor.Resources;
using UnityEngine;
using UnityEngine.UI;

namespace MonsterSurvivor.UI
{
    [DisallowMultipleComponent]
    public sealed class ResourceHUD : MonoBehaviour
    {
        [SerializeField] private PlayerInventory inventory;
        [SerializeField] private Text resourceLabel;

        private IResourceWallet wallet;

        private void Awake()
        {
            resourceLabel.font = UnityEngine.Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        // Start runs after the player's Awake, including when this HUD is enabled later.
        private void Start() => Bind();

        private void OnEnable()
        {
            if (inventory != null && inventory.Wallet != null)
                Bind();
        }

        private void Bind()
        {
            if (wallet != null)
                return;

            wallet = inventory.Wallet;
            wallet.Changed += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            if (wallet == null)
                return;

            wallet.Changed -= Refresh;
            wallet = null;
        }

        private void Refresh()
        {
            resourceLabel.text = $"Wood: {wallet.GetAmount(ResourceType.Wood)}    Stone: {wallet.GetAmount(ResourceType.Stone)}";
        }
    }
}
