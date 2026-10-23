using MonsterSurvivor.Combat;
using UnityEngine;
using UnityEngine.UI;

namespace MonsterSurvivor.UI
{
    [DisallowMultipleComponent]
    public sealed class CombatHUD : MonoBehaviour
    {
        [SerializeField] private CombatEncounter encounter;
        [SerializeField] private Health generator;
        [SerializeField] private Text label;

        private void Awake() =>
            label.font = UnityEngine.Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        private void OnEnable()
        {
            encounter.Changed += Refresh;
            generator.Changed += Refresh;
        }

        private void Start() => Refresh();

        private void OnDisable()
        {
            encounter.Changed -= Refresh;
            generator.Changed -= Refresh;
        }

        private void Refresh()
        {
            string status;
            switch (encounter.State)
            {
                case EncounterState.Ready: status = "Prepare defenses | Space: start combat"; break;
                case EncounterState.Active: status = $"Defending | Enemies: {encounter.RemainingEnemies}"; break;
                case EncounterState.Cleared: status = "Encounter cleared"; break;
                default: status = "Generator destroyed"; break;
            }
            label.text = $"Generator: {generator.Current}/{generator.Maximum} | {status} | R: restart";
        }
    }
}
