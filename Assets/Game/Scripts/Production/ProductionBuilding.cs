using System;
using MonsterSurvivor.Construction;
using MonsterSurvivor.Resources;
using UnityEngine;

namespace MonsterSurvivor.Production
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ConstructionSite))]
    public sealed class ProductionBuilding : MonoBehaviour
    {
        [SerializeField] private ProductionDefinition definition;
        private ConstructionSite site;
        private ProductionBuffer buffer;

        public int Count => buffer != null ? buffer.Count : 0;
        public int Capacity => definition != null ? definition.Capacity : 0;
        public ResourceType ResourceType => definition.ResourceType;
        public event Action Changed;

        private void Awake()
        {
            site = GetComponent<ConstructionSite>();
            if (definition == null)
            {
                Debug.LogError("Assign a production definition to the building.", this);
                enabled = false;
                return;
            }

            buffer = new ProductionBuffer(definition.Interval, definition.Capacity);
            buffer.Changed += NotifyChanged;
        }

        private void Update()
        {
            if (site.isActiveAndEnabled && site.IsBuilt)
                buffer.Advance(Time.deltaTime);
        }

        public void Collect(IResourceWallet wallet)
        {
            if (isActiveAndEnabled && buffer != null && site.isActiveAndEnabled && site.IsBuilt)
                buffer.Collect(wallet, definition.ResourceType);
        }

        private void NotifyChanged() => Changed?.Invoke();

        private void OnDestroy()
        {
            if (buffer != null)
                buffer.Changed -= NotifyChanged;
        }
    }
}
