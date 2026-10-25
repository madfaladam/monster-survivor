using MonsterSurvivor.Combat;
using UnityEngine;
using UnityEngine.UI;

namespace MonsterSurvivor.UI
{
    [DisallowMultipleComponent]
    public sealed class CombatHUD : MonoBehaviour
    {
        [SerializeField] private CombatEncounter encounter;
        [SerializeField] private WaveDirector waveDirector;
        [SerializeField] private Health generator;
        [SerializeField] private Text label;

        private void Awake() =>
            label.font = UnityEngine.Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        private void OnEnable()
        {
            encounter.Changed += Refresh;
            generator.Changed += Refresh;

            if (waveDirector != null)
                waveDirector.Changed += Refresh;
        }

        private void Start() => Refresh();

        // FITUR BARU: Update HUD setiap frame HANYA JIKA sedang dalam fase Prepare (Ready)
        private void Update()
        {
            if (encounter != null && encounter.State == EncounterState.Ready)
            {
                Refresh();
            }
        }

        private void OnDisable()
        {
            encounter.Changed -= Refresh;
            generator.Changed -= Refresh;

            if (waveDirector != null)
                waveDirector.Changed -= Refresh;
        }

        private void Refresh()
        {
            string status;

            if (encounter.State == EncounterState.Finished)
            {
                status = "All Waves Cleared | Encounter Finished";
            }
            else if (encounter.State == EncounterState.Active)
            {
                status = $"Wave {waveDirector.CurrentWave}/{waveDirector.TotalWaves} | " +
                        $"Enemies: {encounter.RemainingEnemies}";
            }
            else if (encounter.State == EncounterState.Ready)
            {
                // Format detik menjadi bilangan bulat tanpa desimal (misal: 29s, 14s)
                int timeRemaining = Mathf.CeilToInt(encounter.PrepareTimeRemaining);
                
                if (waveDirector != null && waveDirector.CompletedWaves > 0)
                {
                    status = $"Wave {waveDirector.CompletedWaves} Cleared | " +
                             $"Next Wave in {timeRemaining}s (Space to Skip) | Next: {waveDirector.CurrentWave}";
                }
                else
                {
                    status = $"Prepare Build | Starts in {timeRemaining}s (Space to Skip) | Next Wave: 1";
                }
            }
            else if (encounter.State == EncounterState.Cleared)
            {
                status = "Encounter cleared";
            }
            else
            {
                status = "Generator destroyed";
            }
            
            label.text =
                $"Generator: {generator.Current}/{generator.Maximum} | {status} | R: restart";
        }
    }
}