using System;
using MonsterSurvivor.Combat;
using NUnit.Framework;

namespace MonsterSurvivor.Tests
{
    public sealed class AttackCooldownTests
    {
        [Test]
        public void AttackIsReadyImmediatelyButCannotRepeatBeforeInterval()
        {
            var cooldown = new AttackCooldown(1f);
            Assert.That(cooldown.TryAttack(), Is.True);
            Assert.That(cooldown.TryAttack(), Is.False);
            cooldown.Advance(0.5f);
            Assert.That(cooldown.TryAttack(), Is.False);
            cooldown.Advance(0.5f);
            Assert.That(cooldown.TryAttack(), Is.True);
        }

        [Test]
        public void LongIdleOrFrameStallDoesNotAccumulateAnAttackBurst()
        {
            var cooldown = new AttackCooldown(1f);
            cooldown.TryAttack();
            cooldown.Advance(100f);
            Assert.That(cooldown.TryAttack(), Is.True);
            Assert.That(cooldown.TryAttack(), Is.False);
            cooldown.Advance(0f);
            Assert.That(cooldown.TryAttack(), Is.False);
        }

        [Test]
        public void InvalidIntervalAndTimeAreRejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new AttackCooldown(0f));
            var cooldown = new AttackCooldown(1f);
            Assert.Throws<ArgumentOutOfRangeException>(() => cooldown.Advance(-1f));
            Assert.Throws<ArgumentOutOfRangeException>(() => cooldown.Advance(float.NaN));
        }
    }
}
