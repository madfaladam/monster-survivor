using System.Collections;
using MonsterSurvivor.Combat;
using MonsterSurvivor.Player;
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
        [SerializeField] private Button turretButton;
        [SerializeField] private Text turretButtonLabel;
        [SerializeField] private Text notificationLabel;
        [SerializeField, Min(0.1f)] private float notificationDuration = 2f;
        [SerializeField] private GameObject turretPrefab;

        [Header("Placement")]
        [SerializeField] private Collider2D placementArea;
        [SerializeField] private LayerMask blockedLayers;
        [SerializeField, Min(0.1f)] private float placementRadius = 0.5f;

        private ConstructionSite prefabSite;
        private BuildingDefinition definition;
        private GameObject preview;
        private SpriteRenderer[] previewRenderers;
        private bool placing;
        private Coroutine notificationRoutine;

        private void Awake()
        {
            if (encounter == null || inventory == null || worldCamera == null || turretPrefab == null)
            {
                Debug.LogError("Assign encounter, inventory, camera, and turret prefab.", this);
                enabled = false;
                return;
            }

            prefabSite = turretPrefab.GetComponentInChildren<ConstructionSite>();
            if (prefabSite == null || turretPrefab.GetComponentInChildren<TurretController>() == null)
            {
                Debug.LogError("Turret prefab needs ConstructionSite and TurretController components.", this);
                enabled = false;
                return;
            }

            definition = prefabSite.Definition;
        }

        private void OnEnable()
        {
            if (encounter != null)
                encounter.Changed += RefreshUI;
            if (inventory != null && inventory.Wallet != null)
                inventory.Wallet.Changed += RefreshUI;
            if (turretButton != null)
                turretButton.onClick.AddListener(SelectTurret);

            RefreshUI();
        }

        private void Update()
        {
            if (!placing)
                return;

            if (encounter.State != EncounterState.Ready || !CanAfford())
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

            if (Mouse.current.leftButton.wasPressedThisFrame &&
                !IsPointerOverUI() && valid)
            {
                PlaceTurret(position);
            }
        }

        private void SelectTurret()
        {
            if (encounter == null || encounter.State != EncounterState.Ready)
                return;

            if (!CanAfford())
            {
                ShowNotification(
                    $"Resource tidak cukup. Butuh {definition.WoodCost} Wood + {definition.StoneCost} Stone.");
                return;
            }

            CancelPlacement();
            preview = Instantiate(turretPrefab);
            preview.name = $"{turretPrefab.name} (Placement Preview)";
            preview.SetActive(false);

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

        private void PlaceTurret(Vector2 position)
        {
            GameObject building = Instantiate(turretPrefab, position, Quaternion.identity);
            ConstructionSite site = building.GetComponentInChildren<ConstructionSite>();
            TurretController turret = building.GetComponentInChildren<TurretController>();

            if (site == null || turret == null || !site.BeginAutoConstruction(inventory.Wallet))
            {
                Destroy(building);
                ShowNotification("Gagal membangun turret. Periksa resource dan prefab.");
                CancelPlacement();
                RefreshUI();
                return;
            }

            encounter.RegisterTurret(turret);
            CancelPlacement();
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
            Vector3 screen = new Vector3(screenPosition.x, screenPosition.y,
                -worldCamera.transform.position.z);
            Vector3 world = worldCamera.ScreenToWorldPoint(screen);
            return new Vector2(world.x, world.y);
        }

        private static bool IsPointerOverUI() =>
            EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        private bool CanAfford()
        {
            if (definition == null || inventory == null || inventory.Wallet == null)
                return false;

            return inventory.Wallet.GetAmount(MonsterSurvivor.Resources.ResourceType.Wood) >= definition.WoodCost &&
                   inventory.Wallet.GetAmount(MonsterSurvivor.Resources.ResourceType.Stone) >= definition.StoneCost;
        }

        private void RefreshUI()
        {
            bool ready = encounter != null && encounter.State == EncounterState.Ready;
            if (buildPanel != null && buildPanel.activeSelf != ready)
                buildPanel.SetActive(ready);

            if (turretButtonLabel != null && definition != null)
                turretButtonLabel.text = $"{definition.DisplayName}\n{definition.WoodCost} Wood + {definition.StoneCost} Stone";
            if (!ready || (placing && !CanAfford()))
                CancelPlacement();

            // Keep the button clickable when resources are insufficient so it can show a message.
            if (turretButton != null)
                turretButton.interactable = ready && !placing;
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
            if (turretButton != null)
                turretButton.onClick.RemoveListener(SelectTurret);
            CancelPlacement();

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
