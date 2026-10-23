using MonsterSurvivor.Construction;
using MonsterSurvivor.Production;
using UnityEngine;
using UnityEngine.UI;

namespace MonsterSurvivor.UI
{
    [DisallowMultipleComponent]
    public sealed class BuildingStatusView : MonoBehaviour
    {
        [SerializeField] private ConstructionSite site;
        [SerializeField] private ProductionBuilding production;
        [SerializeField] private Text statusLabel;
        [SerializeField] private Image progressFill;
        [SerializeField] private SpriteRenderer[] buildingSprites;

        private Color[] originalColors;
        private string costLabel;
        private bool initialized;
        private bool builtVisual;
        private bool visualsInitialized;
        private int lastCount = -1;

        private void Start()
        {
            originalColors = new Color[buildingSprites.Length];
            for (int i = 0; i < buildingSprites.Length; i++)
                originalColors[i] = buildingSprites[i].color;

            BuildingDefinition definition = site.Definition;
            costLabel = $"{definition.DisplayName}\n{definition.WoodCost} Wood + {definition.StoneCost} Stone";
            statusLabel.font = UnityEngine.Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            initialized = true;
            Subscribe();
            Refresh();
        }

        private void OnEnable()
        {
            if (!initialized)
                return;
            Subscribe();
            Refresh();
        }

        private void Subscribe()
        {
            site.Changed += Refresh;
            production.Changed += Refresh;
        }

        private void OnDisable()
        {
            if (!initialized)
                return;
            site.Changed -= Refresh;
            production.Changed -= Refresh;
        }

        private void Refresh()
        {
            bool built = site.IsBuilt;
            if (!visualsInitialized || built != builtVisual)
            {
                for (int i = 0; i < buildingSprites.Length; i++)
                {
                    Color color = originalColors[i];
                    if (!built) color.a *= 0.35f;
                    buildingSprites[i].color = color;
                }
                builtVisual = built;
                visualsInitialized = true;
            }

            progressFill.rectTransform.localScale = new Vector3(site.Progress, 1f, 1f);
            progressFill.transform.parent.gameObject.SetActive(!built);
            statusLabel.color = site.NeedsResources ? new Color(1f, 0.65f, 0.55f) : Color.white;
            if (!built)
                statusLabel.text = site.IsBuilding ? "Building..." : costLabel;
            else if (lastCount != production.Count)
            {
                lastCount = production.Count;
                statusLabel.text = $"{production.ResourceType}: {production.Count}/{production.Capacity}\nCollect in front";
            }
        }
    }
}
