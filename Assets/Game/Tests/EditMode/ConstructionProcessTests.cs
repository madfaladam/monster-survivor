using System;
using MonsterSurvivor.Construction;
using MonsterSurvivor.Resources;
using NUnit.Framework;

namespace MonsterSurvivor.Tests
{
    public sealed class ConstructionProcessTests
    {
        [Test]
        public void InsufficientMixedCostCannotStartOrConsumeResources()
        {
            var wallet = new ResourceWallet(10, 2);
            var process = new ConstructionProcess(5, 3, 3f);
            Assert.That(process.TryBegin(wallet), Is.False);
            Assert.That(process.IsBuilding, Is.False);
            Assert.That(wallet.GetAmount(ResourceType.Wood), Is.EqualTo(10));
            Assert.That(wallet.GetAmount(ResourceType.Stone), Is.EqualTo(2));
        }

        [Test]
        public void CancelRefundsOnceAndClearsProgress()
        {
            var wallet = new ResourceWallet(10, 10);
            var process = new ConstructionProcess(5, 3, 3f);
            Assert.That(process.TryBegin(wallet), Is.True);
            process.Advance(1.5f);
            Assert.That(process.Progress, Is.EqualTo(0.5f));
            Assert.That(wallet.GetAmount(ResourceType.Wood), Is.EqualTo(5));
            process.Cancel();
            process.Cancel();
            Assert.That(process.Progress, Is.Zero);
            Assert.That(wallet.GetAmount(ResourceType.Wood), Is.EqualTo(10));
            Assert.That(wallet.GetAmount(ResourceType.Stone), Is.EqualTo(10));
            Assert.That(process.TryBegin(wallet), Is.True);
        }

        [Test]
        public void CompletedConstructionCannotRefundOrChargeAgain()
        {
            var wallet = new ResourceWallet(10, 10);
            var process = new ConstructionProcess(5, 3, 3f);
            process.TryBegin(wallet);
            Assert.That(process.TryBegin(wallet), Is.False);
            process.Advance(20f);
            process.Cancel();
            Assert.That(process.IsCompleted, Is.True);
            Assert.That(process.IsBuilding, Is.False);
            Assert.That(process.Progress, Is.EqualTo(1f));
            Assert.That(process.TryBegin(wallet), Is.False);
            Assert.That(wallet.GetAmount(ResourceType.Wood), Is.EqualTo(5));
            Assert.That(wallet.GetAmount(ResourceType.Stone), Is.EqualTo(7));
        }

        [Test]
        public void OnlyActiveConstructionAccumulatesTime()
        {
            var process = new ConstructionProcess(0, 0, 3f);
            process.Advance(100f);
            Assert.That(process.Progress, Is.Zero);
            process.TryBegin(new ResourceWallet(0, 0));
            process.Advance(0f);
            Assert.That(process.Progress, Is.Zero);
            process.Advance(2f);
            Assert.That(process.IsCompleted, Is.False);
            process.Advance(1f);
            Assert.That(process.IsCompleted, Is.True);
        }

        [Test]
        public void InvalidConfigurationAndTimeAreRejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ConstructionProcess(-1, 0, 3f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ConstructionProcess(1, 1, 0f));
            var process = new ConstructionProcess(1, 1, 3f);
            Assert.Throws<ArgumentOutOfRangeException>(() => process.Advance(float.NaN));
            Assert.Throws<ArgumentOutOfRangeException>(() => process.Advance(-1f));
        }
    }
}
