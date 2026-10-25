using MonsterSurvivor.Combat;
using UnityEngine;
using UnityEngine.UI;

namespace MonsterSurvivor.UI
{
    [DisallowMultipleComponent]
    public sealed class HealthLabel : MonoBehaviour
    {
        [SerializeField] private Health health;
        [SerializeField] private Text label;

        public GameObject HealthStatusObject => label.transform.parent.gameObject;
        private void OnEnable() => health.Changed += Refresh;
        private void Start() => Refresh();
        private void OnDisable() => health.Changed -= Refresh;
        private void Refresh() => label.text = $"HP: {health.Current}/{health.Maximum}";
    }
}
