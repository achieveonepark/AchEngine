using System;
using System.Collections;
using UnityEngine;

namespace AchEngine.UI
{
    [DisallowMultipleComponent]
    public abstract class UIView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private bool blocksRaycastsWhenVisible = true;
        [SerializeField] private bool interactableWhenVisible = true;
        [SerializeField] private UITransitionSettings transition =
            new UITransitionSettings(UITransitionMode.FadeScale, 0.18f, 0.96f, true);

        private UIService service;
        private UIViewCatalogEntry catalogEntry;
        private RectTransform rectTransform;
        private Coroutine transitionCoroutine;
        private bool initialized;
        private bool isVisible;

        public string ViewId => catalogEntry != null ? catalogEntry.Id : string.Empty;
        public UILayerId Layer => catalogEntry != null ? catalogEntry.Layer : UILayerId.Screen;
        public UIService Service => service;
        public bool IsVisible => isVisible;
        public bool IsClosing { get; private set; }
        public RectTransform RectTransform => rectTransform != null ? rectTransform : rectTransform = transform as RectTransform;
        public object LastPayload { get; private set; }

        internal void Initialize(UIService owningService, UIViewCatalogEntry entry)
        {
            service = owningService;
            catalogEntry = entry;
            rectTransform = transform as RectTransform;

            EnsureCanvasGroup();
            if (!initialized)
            {
                initialized = true;
                OnInitialize();
            }

            ApplyHiddenState();
            gameObject.SetActive(false);
        }

        public void CloseSelf()
        {
            service?.Close(this);
        }

        internal void Open(object payload)
        {
            StopTransition();

            LastPayload = payload;
            IsClosing = false;
            isVisible = false;
            gameObject.SetActive(true);
            transform.SetAsLastSibling();

            OnBeforeOpen(payload);

            if (!transition.HasAnimation)
            {
                ApplyVisibleState();
                CompleteOpen(payload);
                return;
            }

            transitionCoroutine = StartCoroutine(PlayTransition(true, () => CompleteOpen(payload)));
        }

        internal void Close(Action onComplete)
        {
            if (!gameObject.activeSelf)
            {
                onComplete?.Invoke();
                return;
            }

            StopTransition();

            IsClosing = true;
            OnBeforeClose();
            SetCanvasState(canvasGroup.alpha, false);

            if (!transition.HasAnimation)
            {
                FinishClose(onComplete);
                return;
            }

            transitionCoroutine = StartCoroutine(PlayTransition(false, () => FinishClose(onComplete)));
        }

        protected virtual void OnInitialize()
        {
        }

        protected virtual void OnBeforeOpen(object payload)
        {
        }

        protected virtual void OnOpened(object payload)
        {
        }

        protected virtual void OnBeforeClose()
        {
        }

        protected virtual void OnClosed()
        {
        }

        private void CompleteOpen(object payload)
        {
            IsClosing = false;
            isVisible = true;
            ApplyVisibleState();
            OnOpened(payload);
        }

        private void FinishClose(Action onComplete)
        {
            IsClosing = false;
            isVisible = false;
            ApplyHiddenState();
            gameObject.SetActive(false);
            OnClosed();
            onComplete?.Invoke();
        }

        private IEnumerator PlayTransition(bool visible, Action onComplete)
        {
            var duration = transition.Duration;
            var elapsed = 0f;
            var fromAlpha = canvasGroup.alpha;
            var toAlpha = visible ? 1f : 0f;
            var fromScale = RectTransform != null ? RectTransform.localScale.x : 1f;
            var toScale = visible ? 1f : GetHiddenScale();

            while (elapsed < duration)
            {
                elapsed += transition.UseUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                var eased = 1f - Mathf.Pow(1f - t, 3f);

                switch (transition.Mode)
                {
                    case UITransitionMode.Fade:
                        canvasGroup.alpha = Mathf.LerpUnclamped(fromAlpha, toAlpha, eased);
                        break;
                    case UITransitionMode.FadeScale:
                        canvasGroup.alpha = Mathf.LerpUnclamped(fromAlpha, toAlpha, eased);
                        if (RectTransform != null)
                        {
                            var scale = Mathf.LerpUnclamped(fromScale, toScale, eased);
                            RectTransform.localScale = Vector3.one * scale;
                        }
                        break;
                }

                yield return null;
            }

            switch (transition.Mode)
            {
                case UITransitionMode.Fade:
                    canvasGroup.alpha = toAlpha;
                    break;
                case UITransitionMode.FadeScale:
                    canvasGroup.alpha = toAlpha;
                    if (RectTransform != null)
                    {
                        RectTransform.localScale = Vector3.one * toScale;
                    }
                    break;
            }

            transitionCoroutine = null;
            onComplete?.Invoke();
        }

        private void ApplyVisibleState()
        {
            if (RectTransform != null)
            {
                RectTransform.localScale = Vector3.one;
            }

            SetCanvasState(1f, true);
        }

        private void ApplyHiddenState()
        {
            if (RectTransform != null)
            {
                RectTransform.localScale = Vector3.one * GetHiddenScale();
            }

            SetCanvasState(0f, false);
        }

        private float GetHiddenScale()
        {
            return transition.Mode == UITransitionMode.FadeScale ? transition.HiddenScale : 1f;
        }

        private void SetCanvasState(float alpha, bool visible)
        {
            canvasGroup.alpha = alpha;
            canvasGroup.blocksRaycasts = visible && blocksRaycastsWhenVisible;
            canvasGroup.interactable = visible && interactableWhenVisible;
        }

        private void EnsureCanvasGroup()
        {
            if (canvasGroup == null && !TryGetComponent(out canvasGroup))
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        private void StopTransition()
        {
            if (transitionCoroutine == null)
            {
                return;
            }

            StopCoroutine(transitionCoroutine);
            transitionCoroutine = null;
        }
    }
}
