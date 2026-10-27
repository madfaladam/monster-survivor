using System;
using System.Collections.Generic;
using UnityEngine;

namespace MonsterSurvivor.VFX
{
    /// <summary>
    /// Defines all available Visual Effects.
    /// </summary>
    public enum VFXType
    {
        None = 0,
        EnemyHit,
        EnemyDeath,
        EnemyAttack,
        ResourceCollected,
        BuildingDone,
        BuildingDestroyed
    }

    [Serializable]
    public struct VFXData
    {
        public VFXType Type;
        public ParticleSystem Prefab;
        [Min(1)] public int InitialPoolSize;
    }

    [DisallowMultipleComponent]
    public sealed class VFXManager : MonoBehaviour
    {
        [Header("VFX Library")]
        [SerializeField] private VFXData[] vfxLibrary;

        // O(1) Lookup dictionary for VFX Pools
        private readonly Dictionary<VFXType, Queue<ParticleSystem>> poolDictionary = new Dictionary<VFXType, Queue<ParticleSystem>>();
        private readonly Dictionary<VFXType, VFXData> dataDictionary = new Dictionary<VFXType, VFXData>();

        public static VFXManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            InitializePools();
        }

        /// <summary>
        /// Generates the parent-child hierarchy and instantiates the initial particle pools.
        /// </summary>
        private void InitializePools()
        {
            if (vfxLibrary == null) return;

            foreach (VFXData data in vfxLibrary)
            {
                if (data.Type == VFXType.None || data.Prefab == null || poolDictionary.ContainsKey(data.Type))
                    continue;

                dataDictionary[data.Type] = data;
                Queue<ParticleSystem> queue = new Queue<ParticleSystem>();

                // Create a neat parent container for each VFX type in the Hierarchy
                GameObject parentObj = new GameObject($"VFXPool_{data.Type}");
                parentObj.transform.SetParent(transform);

                for (int i = 0; i < data.InitialPoolSize; i++)
                {
                    ParticleSystem ps = Instantiate(data.Prefab, parentObj.transform);
                    ps.gameObject.SetActive(false);
                    queue.Enqueue(ps);
                }

                poolDictionary.Add(data.Type, queue);
            }
        }

        /// <summary>
        /// Plays a VFX at the specified world position.
        /// </summary>
        public void PlayVFX(VFXType type, Vector3 position)
        {
            if (type == VFXType.None || !poolDictionary.TryGetValue(type, out Queue<ParticleSystem> queue))
                return;

            ParticleSystem ps = null;
            int count = queue.Count;

            // Search for an inactive particle in the queue
            for (int i = 0; i < count; i++)
            {
                ParticleSystem candidate = queue.Dequeue();
                if (candidate != null && !candidate.gameObject.activeInHierarchy)
                {
                    ps = candidate;
                    break;
                }
                else if (candidate != null)
                {
                    // If it is currently playing, return it to the back of the queue
                    queue.Enqueue(candidate);
                }
            }

            // Auto-Expand Pool: If all particles in the pool are currently active, instantiate a new one
            if (ps == null)
            {
                VFXData data = dataDictionary[type];
                Transform parentObj = transform.Find($"VFXPool_{type}");
                ps = Instantiate(data.Prefab, parentObj);
            }

            // Position and Play
            ps.transform.position = position;
            ps.gameObject.SetActive(true);
            ps.Play();
            
            // Re-enqueue for future use
            queue.Enqueue(ps);
        }
    }
}