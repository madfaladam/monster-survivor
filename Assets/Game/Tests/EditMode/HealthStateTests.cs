using System;
using MonsterSurvivor.Combat;
using NUnit.Framework;

namespace MonsterSurvivor.Tests
{
    public sealed class HealthStateTests
    {
        [Test]
        public void LethalDamageClampsAtZeroAndDeathOccursOnce()
        {
            var health = new HealthState(40);
            int deaths = 0;
            int changes = 0;
            health.Died += () => deaths++;
            health.Changed += () => changes++;
            health.TakeDamage(15);
            Assert.That(health.Current, Is.EqualTo(25));
            health.TakeDamage(int.MaxValue);
            health.TakeDamage(15);
            Assert.That(health.Current, Is.Zero);
            Assert.That(health.IsAlive, Is.False);
            Assert.That(deaths, Is.EqualTo(1));
            Assert.That(changes, Is.EqualTo(2));
        }

        [Test]
        public void ReentrantDamageFromAnObserverDoesNotReportDeathTwice()
        {
            var health = new HealthState(10);
            int deaths = 0;
            health.Died += () => deaths++;
            health.Changed += () => health.TakeDamage(10);
            health.TakeDamage(1);
            Assert.That(deaths, Is.EqualTo(1));
        }

        [Test]
        public void InvalidDamageCannotHealAndZeroDamageDoesNotNotify()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new HealthState(0));
            var health = new HealthState(10);
            int changes = 0;
            health.Changed += () => changes++;
            Assert.Throws<ArgumentOutOfRangeException>(() => health.TakeDamage(-1));
            health.TakeDamage(0);
            Assert.That(health.Current, Is.EqualTo(10));
            Assert.That(changes, Is.Zero);
        }
    }
}
