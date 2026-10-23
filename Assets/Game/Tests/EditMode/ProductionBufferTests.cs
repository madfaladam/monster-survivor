using System;
using MonsterSurvivor.Production;
using MonsterSurvivor.Resources;
using NUnit.Framework;

namespace MonsterSurvivor.Tests
{
    public sealed class ProductionBufferTests
    {
        [Test]
        public void ProductionRequiresACompleteInterval()
        {
            var buffer = new ProductionBuffer(3f, 6);
            buffer.Advance(2f);
            Assert.That(buffer.Count, Is.Zero);
            buffer.Advance(1f);
            Assert.That(buffer.Count, Is.EqualTo(1));
        }

        [Test]
        public void LargeFrameIsCappedAndNotifiedOnce()
        {
            var buffer = new ProductionBuffer(3f, 6);
            int notifications = 0;
            buffer.Changed += () => notifications++;
            buffer.Advance(float.MaxValue);
            Assert.That(buffer.Count, Is.EqualTo(6));
            Assert.That(notifications, Is.EqualTo(1));
            buffer.Advance(100f);
            Assert.That(notifications, Is.EqualTo(1));
        }

        [Test]
        public void CollectingFullStoreRestartsTimerWithoutBacklog()
        {
            var buffer = new ProductionBuffer(3f, 6);
            var wallet = new ResourceWallet(0, 0);
            buffer.Advance(100f);
            buffer.Advance(100f);
            Assert.That(buffer.Collect(wallet, ResourceType.Wood), Is.EqualTo(6));
            Assert.That(buffer.Collect(wallet, ResourceType.Wood), Is.Zero);
            Assert.That(wallet.GetAmount(ResourceType.Wood), Is.EqualTo(6));
            buffer.Advance(2f);
            Assert.That(buffer.Count, Is.Zero);
            buffer.Advance(1f);
            Assert.That(buffer.Count, Is.EqualTo(1));
        }

        [Test]
        public void CollectingBeforeFullPreservesPartiallyCompletedProduction()
        {
            var buffer = new ProductionBuffer(3f, 6);
            var wallet = new ResourceWallet(0, 0);
            buffer.Advance(5f);
            Assert.That(buffer.Collect(wallet, ResourceType.Stone), Is.EqualTo(1));
            buffer.Advance(1f);
            Assert.That(buffer.Count, Is.EqualTo(1));
            Assert.That(wallet.GetAmount(ResourceType.Wood), Is.Zero);
            Assert.That(wallet.GetAmount(ResourceType.Stone), Is.EqualTo(1));
        }

        [Test]
        public void WalletHeadroomLimitsCollectionWithoutLosingStock()
        {
            var buffer = new ProductionBuffer(3f, 6);
            var wallet = new ResourceWallet(int.MaxValue - 2, 0);
            buffer.Advance(18f);
            Assert.That(buffer.Collect(wallet, ResourceType.Wood), Is.EqualTo(2));
            Assert.That(buffer.Count, Is.EqualTo(4));
            Assert.That(buffer.Collect(wallet, ResourceType.Wood), Is.Zero);
            Assert.That(buffer.Count, Is.EqualTo(4));
        }

        [Test]
        public void InvalidConfigurationAndTimeAreRejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ProductionBuffer(0f, 6));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ProductionBuffer(3f, 0));
            var buffer = new ProductionBuffer(3f, 6);
            Assert.Throws<ArgumentOutOfRangeException>(() => buffer.Advance(-1f));
            Assert.Throws<ArgumentOutOfRangeException>(() => buffer.Advance(float.PositiveInfinity));
        }
    }
}
