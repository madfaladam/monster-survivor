
using MonsterSurvivor.Construction;
using MonsterSurvivor.Production;
using UnityEngine;
using UnityEngine.UI;

namespace MonsterSurvivor.UI
{
    [DisallowMultipleComponent]
    public sealed class BuildingStatusView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ConstructionSite site;
        [SerializeField] private ProductionBuilding production;
        [SerializeField] private Text statusLabel;
        [SerializeField] private Image progressFill;
        [SerializeField] private SpriteRenderer[] buildingSprites;

        private Color[] originalColors;
        private string costLabel;
        private string builtLabel;

        private bool initialized;
        private bool builtVisual;
        private bool visualsInitialized;
        private bool subscribed;

        private HealthLabel healthLabel;
        private int lastCount = -1;

        private void Awake()
        {
            if (site == null)
                site = GetComponent<ConstructionSite>();

            if (production == null)
                production = GetComponent<ProductionBuilding>();

            if (site == null)
            {
                Debug.LogError(
                    "BuildingStatusView requires a ConstructionSite reference.",
                    this);
                enabled = false;
                return;
            }

            if (statusLabel == null)
            {
                Debug.LogError(
                    "BuildingStatusView requires a Status Label reference.",
                    this);
                enabled = false;
                return;
            }

            if (progressFill == null)
            {
                Debug.LogError(
                    "BuildingStatusView requires a Progress Fill reference.",
                    this);
                enabled = false;
                return;
            }

            if (buildingSprites == null)
                buildingSprites = new SpriteRenderer[0];

            originalColors = new Color[buildingSprites.Length];

            for (int i = 0; i < buildingSprites.Length; i++)
            {
                if (buildingSprites[i] != null)
                    originalColors[i] = buildingSprites[i].color;
            }

            healthLabel = GetComponent<HealthLabel>();

            var definition = site.Definition;

            if (definition == null)
            {
                Debug.LogError(
                    "ConstructionSite has no BuildingDefinition.",
                    this);
                enabled = false;
                return;
            }

            costLabel =
                $"{definition.DisplayName}\n" +
                $"{definition.WoodCost} Wood + " +
                $"{definition.StoneCost} Stone";

            builtLabel = $"{definition.DisplayName}\nReady";

            statusLabel.font =
                UnityEngine.Resources.GetBuiltinResource<Font>(
                    "LegacyRuntime.ttf");

            initialized = true;
        }

        private void OnEnable()
        {
            if (!initialized)
                return;

            Subscribe();
            Refresh();
        }

        private void Start()
        {
            if (initialized)
                Refresh();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            if (subscribed || site == null)
                return;

            site.Changed += Refresh;

            if (production != null)
                production.Changed += Refresh;

            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed)
                return;

            if (site != null)
                site.Changed -= Refresh;

            if (production != null)
                production.Changed -= Refresh;

            subscribed = false;
        }

        private void Refresh()
        {
            if (!initialized || site == null)
                return;

            bool built = site.IsBuilt;

            // Update building appearance only when build state changes.
            if (!visualsInitialized || built != builtVisual)
            {
                for (int i = 0; i < buildingSprites.Length; i++)
                {
                    if (buildingSprites[i] == null)
                        continue;

                    Color color = originalColors[i];

                    if (!built)
                        color.a *= 0.35f;

                    buildingSprites[i].color = color;
                }

                builtVisual = built;
                visualsInitialized = true;
            }

            // Hide construction progress when building is complete.
            Transform progressContainer = progressFill.transform.parent;

            if (progressContainer != null &&
                progressContainer.gameObject.activeSelf != !built)
            {
                progressContainer.gameObject.SetActive(!built);
            }

            progressFill.rectTransform.localScale = new Vector3(site.Progress, 1f, 1f);

            // Show health only after construction is complete.
            if (healthLabel != null &&
                healthLabel.HealthStatusObject != null)
            {
                GameObject healthObject =
                    healthLabel.HealthStatusObject;

                if (healthObject.activeSelf != built)
                    healthObject.SetActive(built);
            }

            // Hide the construction label after completion.
            // Production buildings continue displaying their resource count.
            bool showStatus = !built || production != null;

            if (statusLabel.gameObject.activeSelf != showStatus)
                statusLabel.gameObject.SetActive(showStatus);

            if (!built)
            {
                statusLabel.color = site.NeedsResources
                    ? new Color(1f, 0.65f, 0.55f)
                    : Color.white;

                statusLabel.text = site.IsBuilding
                    ? "Building..."
                    : costLabel;

                return;
            }

            if (production == null)
            {
                lastCount = -1;
                return;
            }

            if (lastCount != production.Count)
            {
                lastCount = production.Count;

                statusLabel.color = Color.white;
                statusLabel.text =
                    $"{production.ResourceType}: " +
                    $"{production.Count}/{production.Capacity}\n" +
                    "Collect in front";
            }
        }
    }
}
