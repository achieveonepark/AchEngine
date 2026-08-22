using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.Advertisements;

namespace AchEngine.Managers
{
    /// <summary>Unity Ads Legacy SDK를 <see cref="IAdsProvider"/>로 연결하는 기본 공급자입니다.</summary>
    public sealed class UnityAdsProvider : IAdsProvider, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
    {
        private readonly object syncRoot = new();
        private readonly Dictionary<string, LoadRequest> loadRequests = new(StringComparer.Ordinal);

        private TaskCompletionSource<bool> initializationCompletionSource;
        private ShowRequest showRequest;

        /// <summary>Unity Ads SDK 초기화를 시작하고 완료 콜백을 기다립니다.</summary>
        public Task InitializeAsync(string applicationId, bool testMode)
        {
            if (string.IsNullOrWhiteSpace(applicationId))
                return Task.FromException(new ArgumentException("Unity Ads 게임 ID는 비어 있을 수 없습니다.", nameof(applicationId)));

            if (Advertisement.isInitialized)
                return Task.CompletedTask;

            lock (syncRoot)
            {
                if (initializationCompletionSource != null)
                    return initializationCompletionSource.Task;

                initializationCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                try
                {
                    Advertisement.Initialize(applicationId, testMode, this);
                }
                catch (Exception exception)
                {
                    initializationCompletionSource.TrySetException(exception);
                }

                return initializationCompletionSource.Task;
            }
        }

        /// <summary>Unity Ads 광고 단위를 로드합니다.</summary>
        public Task<AdsLoadResult> LoadAsync(string adUnitId, AdsPlacement placement)
        {
            if (string.IsNullOrWhiteSpace(adUnitId))
                return Task.FromResult(AdsLoadResult.Failed(placement, "광고 단위 ID는 비어 있을 수 없습니다."));

            if (placement == AdsPlacement.Banner)
                return LoadBannerAsync(adUnitId);

            lock (syncRoot)
            {
                if (loadRequests.TryGetValue(adUnitId, out var existingRequest))
                    return existingRequest.CompletionSource.Task;

                var request = new LoadRequest(placement);
                loadRequests.Add(adUnitId, request);
                try
                {
                    Advertisement.Load(adUnitId, this);
                }
                catch (Exception exception)
                {
                    CompleteLoad(adUnitId, AdsLoadResult.Failed(placement, exception.Message));
                }

                return request.CompletionSource.Task;
            }
        }

        /// <summary>로드된 Unity Ads 광고를 표시합니다.</summary>
        public Task<AdsShowResult> ShowAsync(string adUnitId, AdsPlacement placement)
        {
            if (string.IsNullOrWhiteSpace(adUnitId))
                return Task.FromResult(AdsShowResult.Failed(placement, "광고 단위 ID는 비어 있을 수 없습니다."));

            if (placement == AdsPlacement.Banner)
            {
                try
                {
                    Advertisement.Banner.Show(adUnitId);
                    return Task.FromResult(AdsShowResult.Completed(placement));
                }
                catch (Exception exception)
                {
                    return Task.FromResult(AdsShowResult.Failed(placement, exception.Message));
                }
            }

            lock (syncRoot)
            {
                if (showRequest != null)
                    return Task.FromResult(AdsShowResult.Failed(placement, "다른 전면 광고가 이미 표시 중입니다."));

                var request = new ShowRequest(adUnitId, placement);
                showRequest = request;
                try
                {
                    Advertisement.Show(adUnitId, this);
                }
                catch (Exception exception)
                {
                    CompleteShow(AdsShowResult.Failed(placement, exception.Message));
                }

                return request.CompletionSource.Task;
            }
        }

        /// <summary>현재 표시 중인 Unity Ads 배너를 숨깁니다.</summary>
        public void HideBanner()
        {
            Advertisement.Banner.Hide();
        }

        /// <summary>Unity Ads SDK 초기화 완료 콜백입니다.</summary>
        public void OnInitializationComplete()
        {
            initializationCompletionSource?.TrySetResult(true);
        }

        /// <summary>Unity Ads SDK 초기화 실패 콜백입니다.</summary>
        public void OnInitializationFailed(UnityAdsInitializationError error, string message)
        {
            initializationCompletionSource?.TrySetException(new InvalidOperationException($"Unity Ads 초기화 실패 ({error}): {message}"));
        }

        /// <summary>광고 단위 로드 완료 콜백입니다.</summary>
        public void OnUnityAdsAdLoaded(string placementId)
        {
            LoadRequest request;
            lock (syncRoot)
                loadRequests.TryGetValue(placementId, out request);

            if (request != null)
                CompleteLoad(placementId, AdsLoadResult.Succeeded(request.Placement));
        }

        /// <summary>광고 단위 로드 실패 콜백입니다.</summary>
        public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
        {
            LoadRequest request;
            lock (syncRoot)
                loadRequests.TryGetValue(placementId, out request);

            if (request != null)
                CompleteLoad(placementId, AdsLoadResult.Failed(request.Placement, $"Unity Ads 로드 실패 ({error}): {message}"));
        }

        /// <summary>광고 표시 실패 콜백입니다.</summary>
        public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
        {
            CompleteShow(AdsShowResult.Failed(GetShowingPlacement(placementId), $"Unity Ads 표시 실패 ({error}): {message}"));
        }

        /// <summary>광고 표시 시작 콜백입니다.</summary>
        public void OnUnityAdsShowStart(string placementId)
        {
        }

        /// <summary>광고 클릭 콜백입니다.</summary>
        public void OnUnityAdsShowClick(string placementId)
        {
        }

        /// <summary>광고 표시 완료 콜백입니다.</summary>
        public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
        {
            var placement = GetShowingPlacement(placementId);
            var result = showCompletionState switch
            {
                UnityAdsShowCompletionState.COMPLETED => AdsShowResult.Completed(placement),
                UnityAdsShowCompletionState.SKIPPED => AdsShowResult.Skipped(placement),
                _ => AdsShowResult.Failed(placement, "Unity Ads가 알 수 없는 완료 상태를 반환했습니다."),
            };
            CompleteShow(result);
        }

        private Task<AdsLoadResult> LoadBannerAsync(string adUnitId)
        {
            lock (syncRoot)
            {
                if (loadRequests.TryGetValue(adUnitId, out var existingRequest))
                    return existingRequest.CompletionSource.Task;

                var request = new LoadRequest(AdsPlacement.Banner);
                loadRequests.Add(adUnitId, request);
                try
                {
                    Advertisement.Banner.SetPosition(BannerPosition.BOTTOM_CENTER);
                    Advertisement.Banner.Load(adUnitId, new BannerLoadOptions
                    {
                        loadCallback = () => CompleteLoad(adUnitId, AdsLoadResult.Succeeded(AdsPlacement.Banner)),
                        errorCallback = message => CompleteLoad(adUnitId, AdsLoadResult.Failed(AdsPlacement.Banner, $"Unity Ads 배너 로드 실패: {message}")),
                    });
                }
                catch (Exception exception)
                {
                    CompleteLoad(adUnitId, AdsLoadResult.Failed(AdsPlacement.Banner, exception.Message));
                }

                return request.CompletionSource.Task;
            }
        }

        private void CompleteLoad(string adUnitId, AdsLoadResult result)
        {
            LoadRequest request;
            lock (syncRoot)
            {
                if (!loadRequests.TryGetValue(adUnitId, out request))
                    return;

                loadRequests.Remove(adUnitId);
            }

            request.CompletionSource.TrySetResult(result);
        }

        private AdsPlacement GetShowingPlacement(string placementId)
        {
            lock (syncRoot)
            {
                return showRequest != null && showRequest.AdUnitId == placementId
                    ? showRequest.Placement
                    : AdsPlacement.Interstitial;
            }
        }

        private void CompleteShow(AdsShowResult result)
        {
            ShowRequest request;
            lock (syncRoot)
            {
                request = showRequest;
                showRequest = null;
            }

            request?.CompletionSource.TrySetResult(result);
        }

        private sealed class LoadRequest
        {
            public LoadRequest(AdsPlacement placement)
            {
                Placement = placement;
                CompletionSource = new TaskCompletionSource<AdsLoadResult>(TaskCreationOptions.RunContinuationsAsynchronously);
            }

            public AdsPlacement Placement { get; }
            public TaskCompletionSource<AdsLoadResult> CompletionSource { get; }
        }

        private sealed class ShowRequest
        {
            public ShowRequest(string adUnitId, AdsPlacement placement)
            {
                AdUnitId = adUnitId;
                Placement = placement;
                CompletionSource = new TaskCompletionSource<AdsShowResult>(TaskCreationOptions.RunContinuationsAsynchronously);
            }

            public string AdUnitId { get; }
            public AdsPlacement Placement { get; }
            public TaskCompletionSource<AdsShowResult> CompletionSource { get; }
        }
    }
}
