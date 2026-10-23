using System;
using MonsterSurvivor.Resources;
using NUnit.Framework;

namespace MonsterSurvivor.Tests
{
    public sealed class ResourceWalletTests
    {
        [Test]
        public void StartingResourcesAndPickupAreTrackedSeparately()
        {
            var wallet = new ResourceWallet(10, 5);
            wallet.Add(ResourceType.Wood, 1);
            Assert.That(wallet.GetAmount(ResourceType.Wood), Is.EqualTo(11));
            Assert.That(wallet.GetAmount(ResourceType.Stone), Is.EqualTo(5));
        }

        [TestCase(11, 1)]
        [TestCase(1, 6)]
        public void InsufficientResourcesDoNotPartiallySpend(int wood, int stone)
        {
            var wallet = new ResourceWallet(10, 5);
            int notifications = 0;
            wallet.Changed += () => notifications++;
            Assert.That(wallet.TrySpend(wood, stone), Is.False);
            Assert.That(wallet.GetAmount(ResourceType.Wood), Is.EqualTo(10));
            Assert.That(wallet.GetAmount(ResourceType.Stone), Is.EqualTo(5));
            Assert.That(notifications, Is.Zero);
        }

        [Test]
        public void SpendingExactBalanceNotifiesOnceWithBothBalancesUpdated()
        {
            var wallet = new ResourceWallet(10, 5);
            int notifications = 0;
            wallet.Changed += () =>
            {
                notifications++;
                Assert.That(wallet.GetAmount(ResourceType.Wood), Is.Zero);
                Assert.That(wallet.GetAmount(ResourceType.Stone), Is.Zero);
            };
            Assert.That(wallet.TrySpend(10, 5), Is.True);
            Assert.That(notifications, Is.EqualTo(1));
        }

        [Test]
        public void InvalidAmountsCannotCreateOrRemoveResources()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ResourceWallet(-1, 0));
            var wallet = new ResourceWallet(10, 5);
            Assert.Throws<ArgumentOutOfRangeException>(() => wallet.Add(ResourceType.Wood, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => wallet.TrySpend(1, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => wallet.Add((ResourceType)99, 1));
            Assert.That(wallet.GetAmount(ResourceType.Wood), Is.EqualTo(10));
            Assert.That(wallet.GetAmount(ResourceType.Stone), Is.EqualTo(5));
        }

        [Test]
        public void OverflowLeavesBalanceUnchanged()
        {
            var wallet = new ResourceWallet(int.MaxValue, 0);
            Assert.Throws<OverflowException>(() => wallet.Add(ResourceType.Wood, 1));
            Assert.That(wallet.GetAmount(ResourceType.Wood), Is.EqualTo(int.MaxValue));
        }

        [Test]
        public void ZeroTransactionsDoNotRefreshUI()
        {
            var wallet = new ResourceWallet(0, 0);
            int notifications = 0;
            wallet.Changed += () => notifications++;
            wallet.Add(ResourceType.Wood, 0);
            Assert.That(wallet.TrySpend(0, 0), Is.True);
            Assert.That(notifications, Is.Zero);
        }
    }
}
