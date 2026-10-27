using System;
using System.Collections.Generic;
using UnityEngine;

namespace MonsterSurvivor.Audio
{
    /// <summary>
    /// Defines all available Background Music tracks.
    /// </summary>
    public enum BGMType
    {
        None = 0,
        PreparePhase,
        BattlePhase,
        GameOver,
        Victory
    }

    /// <summary>
    /// Defines all available Sound Effects.
    /// </summary>
    public enum SFXType
    {
        None = 0,
        EnemyAttack,
        EnemyDeath,
        TurretShoot,
        BuildingPlaced,
        BuildingDestroyed,
        ResourceCollected,
        ButtonClick,
        BuildingDone
    }

    [Serializable]
    public struct BGMData
    {
        public BGMType Type;
        public AudioClip Clip;
        [Range(0f, 1f)] public float Volume;
    }

    [Serializable]
    public struct SFXData
    {
        public SFXType Type;
        public AudioClip Clip;
        [Range(0f, 1f)] public float Volume;
    }

    [DisallowMultipleComponent]
    public sealed class AudioManager : MonoBehaviour
    {
        [Header("Audio Libraries")]
        [SerializeField] private BGMData[] bgmLibrary;
        [SerializeField] private SFXData[] sfxLibrary;

        // O(1) Lookup dictionaries mapping Enums directly to their dedicated AudioSources.
        private readonly Dictionary<BGMType, AudioSource> bgmSources = new Dictionary<BGMType, AudioSource>();
        private readonly Dictionary<SFXType, AudioSource> sfxSources = new Dictionary<SFXType, AudioSource>();

        // SFX Throttling (Anti-Spam) to prevent audio peaking and voice stealing.
        private readonly Dictionary<SFXType, float> lastPlayedTimes = new Dictionary<SFXType, float>();
        private const float MinTimeBetweenSameSFX = 0.05f; // 50ms cooldown for the exact same SFX type

        private BGMType currentPlayingBGM = BGMType.None;

        public static AudioManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            GenerateAudioHierarchy();
        }

        /// <summary>
        /// Automatically generates the parent-child hierarchy and AudioSource components
        /// based on the provided libraries in the Inspector.
        /// </summary>
        private void GenerateAudioHierarchy()
        {
            // --- Generate BGM Hierarchy ---
            GameObject bgmParent = new GameObject("BGM");
            bgmParent.transform.SetParent(transform);

            if (bgmLibrary != null)
            {
                foreach (BGMData data in bgmLibrary)
                {
                    if (data.Type == BGMType.None || bgmSources.ContainsKey(data.Type))
                        continue;

                    GameObject childObj = new GameObject(data.Type.ToString());
                    childObj.transform.SetParent(bgmParent.transform);
                    
                    AudioSource source = childObj.AddComponent<AudioSource>();
                    source.clip = data.Clip;
                    source.volume = data.Volume;
                    source.loop = true;
                    source.playOnAwake = false;
                    
                    bgmSources.Add(data.Type, source);
                }
            }

            // --- Generate SFX Hierarchy ---
            GameObject sfxParent = new GameObject("SFX");
            sfxParent.transform.SetParent(transform);

            if (sfxLibrary != null)
            {
                foreach (SFXData data in sfxLibrary)
                {
                    if (data.Type == SFXType.None || sfxSources.ContainsKey(data.Type))
                        continue;

                    GameObject childObj = new GameObject(data.Type.ToString());
                    childObj.transform.SetParent(sfxParent.transform);

                    AudioSource source = childObj.AddComponent<AudioSource>();
                    source.clip = data.Clip;
                    source.volume = data.Volume;
                    source.loop = false;
                    source.playOnAwake = false;

                    sfxSources.Add(data.Type, source);
                    
                    // Initialize throttle tracking
                    lastPlayedTimes.Add(data.Type, -MinTimeBetweenSameSFX);
                }
            }
        }

        /// <summary>
        /// Cross-switches to the specified BGM. Stops the current one if playing.
        /// </summary>
        public void PlayBGM(BGMType type)
        {
            if (type == BGMType.None) 
                return;

            // Prevent restarting if it's already the active track
            if (currentPlayingBGM == type && bgmSources[type].isPlaying)
                return;

            // Stop current BGM
            StopBGM();

            // Play new BGM
            if (bgmSources.TryGetValue(type, out AudioSource source))
            {
                source.Play();
                currentPlayingBGM = type;
            }
            else
            {
                Debug.LogWarning($"AudioManager: BGMType '{type}' not found.");
            }
        }

        /// <summary>
        /// Stops whatever BGM is currently playing.
        /// </summary>
        public void StopBGM()
        {
            if (currentPlayingBGM != BGMType.None && bgmSources.TryGetValue(currentPlayingBGM, out AudioSource source))
            {
                if (source.isPlaying)
                    source.Stop();
            }
            
            currentPlayingBGM = BGMType.None;
        }

        /// <summary>
        /// Plays an SFX using its dedicated AudioSource with throttling (anti-spam) protection.
        /// </summary>
        public void PlaySFX(SFXType type)
        {
            if (type == SFXType.None) 
                return;

            // Throttling Check: Prevent this specific sound from stacking in the same frame/brief window
            if (lastPlayedTimes.TryGetValue(type, out float lastTime))
            {
                if (Time.time - lastTime < MinTimeBetweenSameSFX)
                    return; // Cancel playback to prevent audio clipping/peaking
            }

            if (sfxSources.TryGetValue(type, out AudioSource source))
            {
                lastPlayedTimes[type] = Time.time;
                
                // Using PlayOneShot on the dedicated source allows the tail of the sound 
                // to finish smoothly even if triggered again shortly after the throttle window.
                source.PlayOneShot(source.clip, source.volume);
            }
            else
            {
                Debug.LogWarning($"AudioManager: SFXType '{type}' not found.");
            }
        }
    }
}