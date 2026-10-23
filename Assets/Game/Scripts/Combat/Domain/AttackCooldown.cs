using System;

namespace MonsterSurvivor.Combat
{
    public sealed class AttackCooldown
    {
        private readonly float interval;
        private float remaining;

        public AttackCooldown(float interval)
        {
            if (interval <= 0f || float.IsNaN(interval) || float.IsInfinity(interval))
                throw new ArgumentOutOfRangeException(nameof(interval));
            this.interval = interval;
        }

        public void Advance(float deltaTime)
        {
            if (deltaTime < 0f || float.IsNaN(deltaTime) || float.IsInfinity(deltaTime))
                throw new ArgumentOutOfRangeException(nameof(deltaTime));
            remaining = Math.Max(0f, remaining - deltaTime);
        }

        public bool TryAttack()
        {
            if (remaining > 0f)
                return false;
            remaining = interval;
            return true;
        }
    }
}
