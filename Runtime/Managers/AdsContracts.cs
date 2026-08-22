using System;
using System.Threading.Tasks;
using UnityEngine;

namespace AchEngine.Managers
{
    /// <summary>광고 단위의 표시 형식입니다.</summary>
    public enum AdsPlacement
    {
        Interstitial,
        Rewarded,
        Banner,
    }

    /// <summary>전면 광고 표시의 최종 상태입니다.</summary>
    public enum AdsShowStatus
    {
        Completed,
        Skipped,
        Failed,
    }

    /// <summary>광고 로드 요청의 결과입니다.</summary>
    public readonly struct AdsLoadResult
    {
        private AdsLoadResult(AdsPlacement placement, bool isSuccess, string message)
        {
            Placement = placement;
            IsSuccess = isSuccess;
            Message = message ?? string.Empty;
        }

        /// <summary>요청한 광고 형식입니다.</summary>
        public AdsPlacement Placement { get; }

        /// <summary>광고를 표시할 수 있도록 로드했는지 여부입니다.</summary>
        public bool IsSuccess { get; }

        /// <summary>실패 원인입니다. 성공하면 빈 문자열입니다.</summary>
        public string Message { get; }

        /// <summary>성공한 로드 결과를 만듭니다.</summary>
        public static AdsLoadResult Succeeded(AdsPlacement placement) => new(placement, true, string.Empty);

        /// <summary>실패한 로드 결과를 만듭니다.</summary>
        public static AdsLoadResult Failed(AdsPlacement placement, string message) => new(placement, false, message);
    }

    /// <summary>광고 표시 요청의 결과입니다.</summary>
    public readonly struct AdsShowResult
    {
        private AdsShowResult(AdsPlacement placement, AdsShowStatus status, string message)
        {
            Placement = placement;
            Status = status;
            Message = message ?? string.Empty;
        }

        /// <summary>표시를 요청한 광고 형식입니다.</summary>
        public AdsPlacement Placement { get; }

        /// <summary>광고 표시의 최종 상태입니다.</summary>
        public AdsShowStatus Status { get; }

        /// <summary>실패 원인입니다. 실패하지 않았으면 빈 문자열입니다.</summary>
        public string Message { get; }

        /// <summary>광고 표시 요청이 실패하지 않았는지 여부입니다.</summary>
        public bool IsSuccess => Status != AdsShowStatus.Failed;

        /// <summary>보상형 광고를 끝까지 시청해 보상을 지급해도 되는지 여부입니다.</summary>
        public bool ShouldGrantReward => Placement == AdsPlacement.Rewarded && Status == AdsShowStatus.Completed;

        /// <summary>완료된 광고 표시 결과를 만듭니다.</summary>
        public static AdsShowResult Completed(AdsPlacement placement) => new(placement, AdsShowStatus.Completed, string.Empty);

        /// <summary>사용자가 건너뛴 광고 표시 결과를 만듭니다.</summary>
        public static AdsShowResult Skipped(AdsPlacement placement) => new(placement, AdsShowStatus.Skipped, string.Empty);

        /// <summary>실패한 광고 표시 결과를 만듭니다.</summary>
        public static AdsShowResult Failed(AdsPlacement placement, string message) => new(placement, AdsShowStatus.Failed, message);
    }

    /// <summary>Unity Ads에 사용할 게임 ID와 광고 단위 ID 묶음입니다.</summary>
    public sealed class AdsConfiguration
    {
        /// <summary>플랫폼별 게임 ID와 선택적인 광고 단위 ID로 설정을 만듭니다.</summary>
        public AdsConfiguration(
            string androidGameId,
            string iosGameId,
            string interstitialAdUnitId = null,
            string rewardedAdUnitId = null,
            string bannerAdUnitId = null,
            bool testMode = false)
        {
            AndroidGameId = androidGameId;
            IosGameId = iosGameId;
            InterstitialAdUnitId = interstitialAdUnitId;
            RewardedAdUnitId = rewardedAdUnitId;
            BannerAdUnitId = bannerAdUnitId;
            TestMode = testMode;
        }

        /// <summary>Android용 Unity Ads 게임 ID입니다.</summary>
        public string AndroidGameId { get; }

        /// <summary>iOS용 Unity Ads 게임 ID입니다.</summary>
        public string IosGameId { get; }

        /// <summary>전면 광고 단위 ID입니다.</summary>
        public string InterstitialAdUnitId { get; }

        /// <summary>보상형 광고 단위 ID입니다.</summary>
        public string RewardedAdUnitId { get; }

        /// <summary>배너 광고 단위 ID입니다.</summary>
        public string BannerAdUnitId { get; }

        /// <summary>Unity Ads 테스트 모드를 사용할지 여부입니다.</summary>
        public bool TestMode { get; }

        internal bool TryGetGameId(out string gameId)
        {
            if (Application.isEditor || Application.platform == RuntimePlatform.Android)
                gameId = AndroidGameId;
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
                gameId = IosGameId;
            else
                gameId = null;

            return !string.IsNullOrWhiteSpace(gameId);
        }

        internal bool TryGetAdUnitId(AdsPlacement placement, out string adUnitId)
        {
            adUnitId = placement switch
            {
                AdsPlacement.Interstitial => InterstitialAdUnitId,
                AdsPlacement.Rewarded => RewardedAdUnitId,
                AdsPlacement.Banner => BannerAdUnitId,
                _ => null,
            };

            return !string.IsNullOrWhiteSpace(adUnitId);
        }
    }

    /// <summary>
    /// 광고 SDK 또는 미디에이션 SDK를 연결하는 경계입니다.
    /// LevelPlay, MAX, AdMob 등 다른 공급자를 사용하려면 이 인터페이스를 구현해 <see cref="AdsManager.Create"/>에 전달하세요.
    /// </summary>
    public interface IAdsProvider
    {
        /// <summary>광고 SDK를 초기화합니다.</summary>
        Task InitializeAsync(string applicationId, bool testMode);

        /// <summary>광고 단위를 로드합니다.</summary>
        Task<AdsLoadResult> LoadAsync(string adUnitId, AdsPlacement placement);

        /// <summary>로드된 광고를 표시합니다.</summary>
        Task<AdsShowResult> ShowAsync(string adUnitId, AdsPlacement placement);

        /// <summary>표시 중인 배너를 숨깁니다.</summary>
        void HideBanner();
    }
}
