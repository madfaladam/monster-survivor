using System;
using MonsterSurvivor.Audio;
using UnityEngine;
using UnityEngine.UI;

namespace MonsterSurvivor.Construction
{
    [DisallowMultipleComponent]
    public sealed class BuildingButtonUI : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Text nameLabel;
        [SerializeField] private Text priceLabel;
        [SerializeField] private GameObject buildingPrefab;

        public event Action<BuildingButtonUI> Clicked;
        public GameObject Prefab => buildingPrefab;
        public BuildingDefinition Definition { get; private set; }

        private void Awake()
        {
            if (button == null)
                button = GetComponent<Button>();

            RefreshDefinition();
        }

        private void OnEnable()
        {
            if (button != null)
                button.onClick.AddListener(HandleClick);
        }

        private void OnDisable()
        {
            if (button != null)
                button.onClick.RemoveListener(HandleClick);
        }

        public bool RefreshDefinition()
        {
            Definition = null;

            if (buildingPrefab != null)
            {
                ConstructionSite site = buildingPrefab.GetComponentInChildren<ConstructionSite>();
                if (site != null)
                    Definition = site.Definition;
            }

            if (nameLabel != null)
                nameLabel.text = Definition != null ? Definition.DisplayName : "Missing Definition";

            if (priceLabel != null)
                priceLabel.text = Definition != null
                    ? $"{Definition.WoodCost} Wood\n{Definition.StoneCost} Stone"
                    : "-";

            return Definition != null;
        }

        public void SetInteractable(bool interactable)
        {
            if (button != null)
                button.interactable = interactable;
        }

        private void HandleClick()
        {
            AudioManager.Instance.PlaySFX(SFXType.ButtonClick);
            Clicked?.Invoke(this);
        }
    }
}
