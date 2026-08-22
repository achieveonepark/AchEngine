using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AchEngine.Managers
{
    /// <summary>
    /// 광고 SDK 초기화와 전면, 보상형, 배너 광고의 로드 및 표시를 관리합니다.
    /// 기본 공급자는 Unity Ads이며, 다른 미디에이션은 <see cref="IAdsProvider"/> 구현으로 교체할 수 있습니다.
    /// </summary>
    public sealed class AdsManager : IManager
    {
        private readonly object syncRoot = new();
        private readonly IAdsProvider provider;
        private readonly HashSet<AdsPlacement> loadedPlacements = new();
        private readonly HashSet<AdsPlacement> loadingPlacements = new();

        private AdsConfiguration configuration;
        private Task initializationTask;
        private bool isShowingFullscreenAd;

        /// <summary>기본 Unity Ads 공급자로 매니저를 만듭니다.</summary>
        public AdsManager() : this(new UnityAdsProvider())
        {
        }

        /// <summary>지정한 광고 공급자로 매니저를 만듭니다.</summary>
        internal AdsManager(IAdsProvider provider)
        {
            this.provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        /// <summary>지정한 광고 공급자를 사용하는 독립 AdsManager를 만듭니다.</summary>
        public static AdsManager Create(IAdsProvider provider) => new(provider);

        /// <summary>광고 SDK 초기화가 완료되었는지 여부입니다.</summary>
        public bool IsInitialized { get; private set; }

        /// <summary>광고 SDK 초기화가 완료되었을 때 호출됩니다.</summary>
        public event Action Initialized;

        /// <summary>광고 SDK 초기화에 실패했을 때 호출됩니다.</summary>
        public event Action<string> InitializationFailed;

        /// <summary>광고 단위 로드가 완료됐을 때 호출됩니다.</summary>
        public event Action<AdsPlacement> AdLoaded;

        /// <summary>광고 단위 로드에 실패했을 때 호출됩니다.</summary>
        public event Action<AdsPlacement, string> AdLoadFailed;

        /// <summary>광고 표시가 끝났거나 실패했을 때 호출됩니다.</summary>
        public event Action<AdsShowResult> AdShowCompleted;

        /// <summary>
        /// 광고 SDK 설정을 지정합니다. 초기화를 시작하기 전에 한 번만 호출할 수 있습니다.
        /// </summary>
        public void Configure(AdsConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            lock (syncRoot)
            {
                if (initializationTask != null || IsInitialized)
                    throw new InvalidOperationException("광고 초기화가 시작된 뒤에는 설정을 변경할 수 없습니다.");

                this.configuration = configuration;
            }
        }

        /// <summary>지정한 광고가 표시 가능한 상태로 로드되었는지 반환합니다.</summary>
        public bool IsLoaded(AdsPlacement placement)
        {
            lock (syncRoot)
                return loadedPlacements.Contains(placement);
        }

        /// <summary>현재 플랫폼의 Unity Ads 게임 ID로 SDK를 초기화합니다.</summary>
        public Task Initialize()
        {
            lock (syncRoot)
            {
                if (IsInitialized)
                    return Task.CompletedTask;

                if (initializationTask != null)
                    return initializationTask;

                if (configuration == null)
                    return Task.FromException(new InvalidOperationException("광고 초기화 전에 AdsConfiguration을 설정하세요."));

                if (!configuration.TryGetGameId(out var gameId))
                    return Task.FromException(new InvalidOperationException("현재 플랫폼에 사용할 Unity Ads 게임 ID가 없습니다. Android 또는 iOS 빌드에서만 지원됩니다."));

                initializationTask = InitializeInternalAsync(gameId, configuration.TestMode);
                return initializationTask;
            }
        }

        /// <summary>전면 광고를 로드합니다.</summary>
        public Task<AdsLoadResult> LoadInterstitialAsync() => LoadAsync(AdsPlacement.Interstitial);

        /// <summary>보상형 광고를 로드합니다.</summary>
        public Task<AdsLoadResult> LoadRewardedAsync() => LoadAsync(AdsPlacement.Rewarded);

        /// <summary>배너 광고를 로드합니다.</summary>
        public Task<AdsLoadResult> LoadBannerAsync() => LoadAsync(AdsPlacement.Banner);

        /// <summary>로드된 전면 광고를 표시합니다.</summary>
        public Task<AdsShowResult> ShowInterstitialAsync() => ShowAsync(AdsPlacement.Interstitial);

        /// <summary>로드된 보상형 광고를 표시합니다.</summary>
        public Task<AdsShowResult> ShowRewardedAsync() => ShowAsync(AdsPlacement.Rewarded);

        /// <summary>로드된 배너 광고를 표시합니다.</summary>
        public Task<AdsShowResult> ShowBannerAsync() => ShowAsync(AdsPlacement.Banner);

        /// <summary>현재 표시 중인 배너 광고를 숨깁니다.</summary>
        public void HideBanner()
        {
            provider.HideBanner();
        }

        private async Task InitializeInternalAsync(string gameId, bool testMode)
        {
            try
            {
                await provider.InitializeAsync(gameId, testMode);

                lock (syncRoot)
                    IsInitialized = true;

                Initialized?.Invoke();
            }
            catch (Exception exception)
            {
                InitializationFailed?.Invoke(exception.Message);
                throw;
            }
        }

        private async Task<AdsLoadResult> LoadAsync(AdsPlacement placement)
        {
            string adUnitId;
            lock (syncRoot)
            {
                if (!IsInitialized)
                    return AdsLoadResult.Failed(placement, "광고 초기화가 완료되지 않았습니다.");

                if (!configuration.TryGetAdUnitId(placement, out adUnitId))
                    return AdsLoadResult.Failed(placement, $"{placement} 광고 단위 ID가 설정되지 않았습니다.");

                if (loadedPlacements.Contains(placement))
                    return AdsLoadResult.Succeeded(placement);

                if (!loadingPlacements.Add(placement))
                    return AdsLoadResult.Failed(placement, "같은 광고 단위를 이미 로드하고 있습니다.");
            }

            try
            {
                var result = await provider.LoadAsync(adUnitId, placement);
                if (result.IsSuccess)
                {
                    lock (syncRoot)
                        loadedPlacements.Add(placement);

                    AdLoaded?.Invoke(placement);
                }
                else
                {
                    AdLoadFailed?.Invoke(placement, result.Message);
                }

                return result;
            }
            catch (Exception exception)
            {
                AdLoadFailed?.Invoke(placement, exception.Message);
                return AdsLoadResult.Failed(placement, exception.Message);
            }
            finally
            {
                lock (syncRoot)
                    loadingPlacements.Remove(placement);
            }
        }

        private async Task<AdsShowResult> ShowAsync(AdsPlacement placement)
        {
            string adUnitId;
            lock (syncRoot)
            {
                if (!IsInitialized)
                    return AdsShowResult.Failed(placement, "광고 초기화가 완료되지 않았습니다.");

                if (!configuration.TryGetAdUnitId(placement, out adUnitId))
                    return AdsShowResult.Failed(placement, $"{placement} 광고 단위 ID가 설정되지 않았습니다.");

                if (!loadedPlacements.Contains(placement))
                    return AdsShowResult.Failed(placement, "광고를 표시하기 전에 로드하세요.");

                if (placement != AdsPlacement.Banner && isShowingFullscreenAd)
                    return AdsShowResult.Failed(placement, "다른 전면 광고가 이미 표시 중입니다.");

                loadedPlacements.Remove(placement);
                if (placement != AdsPlacement.Banner)
                    isShowingFullscreenAd = true;
            }

            try
            {
                var result = await provider.ShowAsync(adUnitId, placement);
                AdShowCompleted?.Invoke(result);
                return result;
            }
            catch (Exception exception)
            {
                var result = AdsShowResult.Failed(placement, exception.Message);
                AdShowCompleted?.Invoke(result);
                return result;
            }
            finally
            {
                if (placement != AdsPlacement.Banner)
                {
                    lock (syncRoot)
                        isShowingFullscreenAd = false;
                }
            }
        }
    }
}
