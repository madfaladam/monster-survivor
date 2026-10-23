using System;

namespace MonsterSurvivor.Combat
{
    public sealed class HealthState
    {
        public int Current { get; private set; }
        public int Maximum { get; }
        public bool IsAlive => Current > 0;
        public event Action Changed;
        public event Action Died;

        public HealthState(int maximum)
        {
            if (maximum <= 0) throw new ArgumentOutOfRangeException(nameof(maximum));
            Maximum = maximum;
            Current = maximum;
        }

        public void TakeDamage(int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            if (!IsAlive || amount == 0)
                return;

            Current -= Math.Min(amount, Current);
            bool died = !IsAlive;
            Changed?.Invoke();
            if (died)
                Died?.Invoke();
        }
    }
}
