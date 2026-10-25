using System.Collections;
using MonsterSurvivor.Combat;
using MonsterSurvivor.Player;
using MonsterSurvivor.Resources;
using MonsterSurvivor.Stats;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MonsterSurvivor.Construction
{
    [DisallowMultipleComponent]
    public sealed class TurretPlacementController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CombatEncounter encounter;
        [SerializeField] private PlayerInventory inventory;
        [SerializeField] private Camera worldCamera;
        [SerializeField] private GameObject buildPanel;
        [SerializeField] private BuildingButtonUI[] buildingButtons;
        [SerializeField] private Text notificationLabel;
        [SerializeField, Min(0.1f)] private float notificationDuration = 2f;

        [Header("Placement")]
        [SerializeField] private Collider2D placementArea;
        [SerializeField] private LayerMask blockedLayers;
        [SerializeField, Min(0.1f)] private float placementRadius = 0.5f;

        private BuildingButtonUI selectedOption;
        private GameSessionStats gameStats;
        private GameObject preview;
        private SpriteRenderer[] previewRenderers;
        private bool placing;
        private Coroutine notificationRoutine;

        private void Awake()
        {
            gameStats = FindFirstObjectByType<GameSessionStats>();
            if (gameStats == null)
                Debug.LogWarning("GameSessionStats was not found; building stats will not be recorded.", this);

            if (encounter == null || inventory == null || worldCamera == null)
            {
                Debug.LogError("Assign encounter, inventory, and camera.", this);
                enabled = false;
                return;
            }

            if (buildingButtons == null || buildingButtons.Length == 0)
            {
                Debug.LogError("Assign at least one BuildingButtonUI in Building Buttons.", this);
                enabled = false;
                return;
            }

            foreach (BuildingButtonUI option in buildingButtons)
            {
                if (option == null)
                    continue;

                if (!option.RefreshDefinition())
                    Debug.LogError($"Building button '{option.name}' needs a prefab with ConstructionSite and BuildingDefinition.", option);

                if (option.Prefab != null && option.Prefab.GetComponentInChildren<ConstructionSite>() == null)
                    Debug.LogError($"Building prefab '{option.Prefab.name}' needs a ConstructionSite.", option);
            }
        }

        private void OnEnable()
        {
            if (encounter != null)
                encounter.Changed += RefreshUI;
            if (inventory != null && inventory.Wallet != null)
                inventory.Wallet.Changed += RefreshUI;

            if (buildingButtons != null)
            {
                foreach (BuildingButtonUI option in buildingButtons)
                {
                    if (option == null)
                        continue;
                    option.Clicked += SelectBuilding;
                    option.RefreshDefinition();
                }
            }

            RefreshUI();
        }

        private void Update()
        {
            if (!placing)
                return;

            if (encounter == null || encounter.State != EncounterState.Ready || !CanAfford(selectedOption))
            {
                CancelPlacement();
                RefreshUI();
                return;
            }

            if ((Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) ||
                (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame))
            {
                CancelPlacement();
                RefreshUI();
                return;
            }

            if (Mouse.current == null || preview == null)
                return;

            Vector2 position = ScreenToWorld(Mouse.current.position.ReadValue());
            preview.transform.position = position;
            bool valid = IsValidPlacement(position);
            SetPreviewColor(valid);

            if (Mouse.current.leftButton.wasPressedThisFrame && !IsPointerOverUI() && valid)
                PlaceBuilding(position);
        }

        private void SelectBuilding(BuildingButtonUI option)
        {
            if (option == null || option.Prefab == null || option.Definition == null ||
                encounter == null || encounter.State != EncounterState.Ready)
                return;

            CancelPlacement();
            selectedOption = option;

            if (!CanAfford(selectedOption))
            {
                ShowNotification($"Not enough resources. Need {FormatPrice(option.Definition)}.");
                selectedOption = null;
                RefreshUI();
                return;
            }

            preview = Instantiate(option.Prefab);
            preview.name = $"{option.Prefab.name} (Placement Preview)";
            foreach (MonoBehaviour behaviour in preview.GetComponentsInChildren<MonoBehaviour>(true))
                behaviour.enabled = false;
            foreach (Collider2D collider in preview.GetComponentsInChildren<Collider2D>(true))
                collider.enabled = false;
            foreach (Rigidbody2D body in preview.GetComponentsInChildren<Rigidbody2D>(true))
                body.simulated = false;

            previewRenderers = preview.GetComponentsInChildren<SpriteRenderer>(true);
            preview.SetActive(true);
            placing = true;
            RefreshUI();
        }

        private void PlaceBuilding(Vector2 position)
        {
            if (selectedOption == null)
                return;

            GameObject building = Instantiate(selectedOption.Prefab, position, Quaternion.identity);
            ConstructionSite site = building.GetComponentInChildren<ConstructionSite>();

            if (site == null || !site.BeginAutoConstruction(inventory.Wallet))
            {
                Destroy(building);
                ShowNotification($"Failed to build {selectedOption.Definition.DisplayName}. Check resources and prefab.");
                CancelPlacement();
                selectedOption = null;
                RefreshUI();
                return;
            }

            // Register build statistics only after placement and construction start succeed.
            BuildingStatsReporter statsReporter = building.GetComponent<BuildingStatsReporter>();
            if (statsReporter == null)
                statsReporter = building.AddComponent<BuildingStatsReporter>();
            statsReporter.Initialize(site, gameStats);

            TurretController turret = building.GetComponentInChildren<TurretController>();
            if (turret != null)
                encounter.RegisterTurret(turret);

            CancelPlacement();
            selectedOption = null;
            RefreshUI();
        }

        private bool IsValidPlacement(Vector2 position)
        {
            if (placementArea != null && !placementArea.OverlapPoint(position))
                return false;

            Collider2D[] overlaps = Physics2D.OverlapCircleAll(position, placementRadius, blockedLayers);
            foreach (Collider2D overlap in overlaps)
            {
                if (overlap == null || overlap == placementArea)
                    continue;
                if (inventory != null &&
                    (overlap.transform == inventory.transform || overlap.transform.IsChildOf(inventory.transform)))
                    continue;
                return false;
            }

            return true;
        }

        private Vector2 ScreenToWorld(Vector2 screenPosition)
        {
            Vector3 screen = new Vector3(screenPosition.x, screenPosition.y, -worldCamera.transform.position.z);
            Vector3 world = worldCamera.ScreenToWorldPoint(screen);
            return new Vector2(world.x, world.y);
        }

        private static bool IsPointerOverUI() =>
            EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        private bool CanAfford(BuildingButtonUI option)
        {
            if (option == null || option.Definition == null || inventory == null || inventory.Wallet == null)
                return false;

            return inventory.Wallet.GetAmount(ResourceType.Wood) >= option.Definition.WoodCost &&
                   inventory.Wallet.GetAmount(ResourceType.Stone) >= option.Definition.StoneCost;
        }

        private static string FormatPrice(BuildingDefinition definition) =>
            $"{definition.WoodCost} Wood + {definition.StoneCost} Stone";

        private void RefreshUI()
        {
            bool ready = encounter != null && encounter.State == EncounterState.Ready;
            if (buildPanel != null && buildPanel.activeSelf != ready)
                buildPanel.SetActive(ready);

            if (!ready)
                CancelPlacement();

            if (buildingButtons == null)
                return;

            foreach (BuildingButtonUI option in buildingButtons)
                if (option != null)
                    option.SetInteractable(ready && !placing);
        }

        private void SetPreviewColor(bool valid)
        {
            Color color = valid
                ? new Color(0.45f, 1f, 0.45f, 0.65f)
                : new Color(1f, 0.3f, 0.3f, 0.65f);

            if (previewRenderers == null)
                return;
            foreach (SpriteRenderer renderer in previewRenderers)
                if (renderer != null)
                    renderer.color = color;
        }

        private void CancelPlacement()
        {
            placing = false;
            previewRenderers = null;
            if (preview != null)
                Destroy(preview);
            preview = null;
        }

        private void ShowNotification(string message)
        {
            if (notificationLabel == null)
            {
                Debug.LogWarning(message, this);
                return;
            }

            notificationLabel.text = message;
            notificationLabel.gameObject.SetActive(true);

            if (notificationRoutine != null)
                StopCoroutine(notificationRoutine);
            notificationRoutine = StartCoroutine(ClearNotification());
        }

        private IEnumerator ClearNotification()
        {
            yield return new WaitForSeconds(notificationDuration);
            if (notificationLabel != null)
                notificationLabel.gameObject.SetActive(false);
            notificationRoutine = null;
        }

        private void OnDisable()
        {
            if (encounter != null)
                encounter.Changed -= RefreshUI;
            if (inventory != null && inventory.Wallet != null)
                inventory.Wallet.Changed -= RefreshUI;

            if (buildingButtons != null)
            {
                foreach (BuildingButtonUI option in buildingButtons)
                    if (option != null)
                        option.Clicked -= SelectBuilding;
            }

            CancelPlacement();
            selectedOption = null;

            if (notificationRoutine != null)
            {
                StopCoroutine(notificationRoutine);
                notificationRoutine = null;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (preview == null)
                return;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(preview.transform.position, placementRadius);
        }
    }
}
