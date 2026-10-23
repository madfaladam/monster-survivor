using System;
using UnityEngine;

namespace MonsterSurvivor.Combat
{
    [DisallowMultipleComponent]
    public sealed class Health : MonoBehaviour
    {
        [SerializeField, Min(1)] private int maximum = 100;
        private HealthState state;

        public int Current => state != null ? state.Current : maximum;
        public int Maximum => maximum;
        public bool IsAlive => isActiveAndEnabled && state != null && state.IsAlive;
        public event Action Changed;
        public event Action Died;

        private void Awake()
        {
            state = new HealthState(maximum);
            state.Changed += NotifyChanged;
            state.Died += NotifyDeath;
        }

        public void TakeDamage(int amount)
        {
            if (isActiveAndEnabled)
                state.TakeDamage(amount);
        }

        private void NotifyChanged() => Changed?.Invoke();
        private void NotifyDeath() => Died?.Invoke();
    }
}
