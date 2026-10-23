using System;
using MonsterSurvivor.Resources;
using UnityEngine;

namespace MonsterSurvivor.Construction
{
    [DisallowMultipleComponent]
    public sealed class ConstructionSite : MonoBehaviour
    {
        [SerializeField] private BuildingDefinition definition;
        private ConstructionProcess process;

        public BuildingDefinition Definition => definition;
        public bool IsBuilt => process != null && process.IsCompleted;
        public bool IsBuilding => process != null && process.IsBuilding;
        public float Progress => process != null ? process.Progress : 0f;
        public bool NeedsResources { get; private set; }
        public event Action Changed;

        private void Awake()
        {
            if (definition == null)
            {
                Debug.LogError("Assign a building definition to the construction site.", this);
                enabled = false;
                return;
            }

            process = new ConstructionProcess(definition.WoodCost, definition.StoneCost,
                definition.BuildDuration);
        }

        public bool TryBegin(IResourceWallet wallet)
        {
            if (!isActiveAndEnabled || process == null || IsBuilt || IsBuilding)
                return false;

            bool started = process.TryBegin(wallet);
            bool needsResources = !started;
            if (started || NeedsResources != needsResources)
            {
                NeedsResources = needsResources;
                Changed?.Invoke();
            }
            return started;
        }

        public void Advance(float deltaTime)
        {
            if (!isActiveAndEnabled || !IsBuilding || deltaTime <= 0f)
                return;

            process.Advance(deltaTime);
            Changed?.Invoke();
        }

        public void Cancel()
        {
            if (!IsBuilding)
                return;

            process.Cancel();
            NeedsResources = false;
            Changed?.Invoke();
        }

        private void OnDisable() => Cancel();
    }
}
