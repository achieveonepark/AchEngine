using System;
using UnityEngine;

namespace AchEngine.UI
{
    /// <summary>UIView 전환 애니메이션의 종류·시간·스케일·시간축을 설정하는 직렬화 가능한 구조체입니다.</summary>
    [Serializable]
    public struct UITransitionSettings
    {
        [SerializeField] private UITransitionMode mode;
        [Min(0f)]
        [SerializeField] private float duration;
        [Range(0.1f, 1f)]
        [SerializeField] private float hiddenScale;
        [SerializeField] private bool useUnscaledTime;

        /// <summary>전환 애니메이션 방식.</summary>
        public UITransitionMode Mode => mode;
        /// <summary>전환에 소요되는 시간(초).</summary>
        public float Duration => duration;
        /// <summary>숨김 상태의 스케일 값. 0 이하이면 0.96으로 대체됩니다.</summary>
        public float HiddenScale => hiddenScale <= 0f ? 0.96f : hiddenScale;
        /// <summary>true면 Time.timeScale에 영향을 받지 않는 비스케일드 시간을 사용합니다.</summary>
        public bool UseUnscaledTime => useUnscaledTime;
        /// <summary>전환이 실제로 재생될 조건인지 나타냅니다.</summary>
        public bool HasAnimation => mode != UITransitionMode.None && duration > 0.0001f;

        public UITransitionSettings(
            UITransitionMode mode,
            float duration,
            float hiddenScale,
            bool useUnscaledTime)
        {
            this.mode = mode;
            this.duration = Mathf.Max(0f, duration);
            this.hiddenScale = Mathf.Clamp(hiddenScale, 0.1f, 1f);
            this.useUnscaledTime = useUnscaledTime;
        }

        public static UITransitionSettings Default =>
            new UITransitionSettings(UITransitionMode.FadeScale, 0.18f, 0.96f, true);
    }
}
