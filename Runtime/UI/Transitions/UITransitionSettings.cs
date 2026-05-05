using System;
using UnityEngine;

namespace AchEngine.UI
{
    [Serializable]
    public struct UITransitionSettings
    {
        [SerializeField] private UITransitionMode mode;
        [Min(0f)]
        [SerializeField] private float duration;
        [Range(0.1f, 1f)]
        [SerializeField] private float hiddenScale;
        [SerializeField] private bool useUnscaledTime;

        public UITransitionMode Mode => mode;
        public float Duration => duration;
        public float HiddenScale => hiddenScale <= 0f ? 0.96f : hiddenScale;
        public bool UseUnscaledTime => useUnscaledTime;
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
