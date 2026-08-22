using System.Threading.Tasks;
using NUnit.Framework;

namespace AchEngine.Managers.Tests
{
    public class AdsManagerTests
    {
        [Test]
        public void 설정없이_초기화하면_예외가_발생한다()
        {
            var manager = new AdsManager(new FakeAdsProvider());

            Assert.ThrowsAsync<System.InvalidOperationException>(async () => await manager.Initialize());
        }

        [Test]
        public async Task 보상형광고는_로드후_완료상태에서만_보상을_지급한다()
        {
            var provider = new FakeAdsProvider();
            var manager = new AdsManager(provider);
            manager.Configure(CreateConfiguration());

            await manager.Initialize();
            var load = await manager.LoadRewardedAsync();
            var show = await manager.ShowRewardedAsync();

            Assert.That(manager.IsInitialized, Is.True);
            Assert.That(load.IsSuccess, Is.True);
            Assert.That(show.ShouldGrantReward, Is.True);
            Assert.That(provider.LastLoadedPlacement, Is.EqualTo(AdsPlacement.Rewarded));
            Assert.That(provider.LastShownPlacement, Is.EqualTo(AdsPlacement.Rewarded));
        }

        [Test]
        public async Task 로드하지않은_광고는_표시하지않는다()
        {
            var provider = new FakeAdsProvider();
            var manager = new AdsManager(provider);
            manager.Configure(CreateConfiguration());
            await manager.Initialize();

            var result = await manager.ShowInterstitialAsync();

            Assert.That(result.Status, Is.EqualTo(AdsShowStatus.Failed));
            Assert.That(provider.LastShownPlacement, Is.Null);
        }

        private static AdsConfiguration CreateConfiguration()
        {
            return new AdsConfiguration(
                "android-game-id",
                "ios-game-id",
                "interstitial-id",
                "rewarded-id",
                "banner-id",
                true);
        }

        private sealed class FakeAdsProvider : IAdsProvider
        {
            public AdsPlacement? LastLoadedPlacement { get; private set; }
            public AdsPlacement? LastShownPlacement { get; private set; }

            public Task InitializeAsync(string applicationId, bool testMode) => Task.CompletedTask;

            public Task<AdsLoadResult> LoadAsync(string adUnitId, AdsPlacement placement)
            {
                LastLoadedPlacement = placement;
                return Task.FromResult(AdsLoadResult.Succeeded(placement));
            }

            public Task<AdsShowResult> ShowAsync(string adUnitId, AdsPlacement placement)
            {
                LastShownPlacement = placement;
                return Task.FromResult(AdsShowResult.Completed(placement));
            }

            public void HideBanner()
            {
            }
        }
    }
}
