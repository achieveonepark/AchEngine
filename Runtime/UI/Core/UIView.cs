using System;
using System.Collections;
using UnityEngine;

namespace AchEngine.UI
{
    /// <summary>
    /// 모든 UI 뷰의 기반 추상 클래스.
    /// 열기/닫기 트랜지션, 레이어 관리, 페이로드 전달을 담당한다.
    /// </summary>
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

        /// <summary>카탈로그에 등록된 이 뷰의 고유 ID.</summary>
        public string ViewId => catalogEntry != null ? catalogEntry.Id : string.Empty;

        /// <summary>이 뷰가 배치되는 UI 레이어.</summary>
        public UILayerId Layer => catalogEntry != null ? catalogEntry.Layer : UILayerId.Screen;

        /// <summary>이 뷰를 소유하는 <see cref="UIService"/> 인스턴스.</summary>
        public UIService Service => service;

        /// <summary>뷰가 현재 화면에 표시 중인지 여부.</summary>
        public bool IsVisible => isVisible;

        /// <summary>닫기 트랜지션이 진행 중인지 여부.</summary>
        public bool IsClosing { get; private set; }

        /// <summary>이 뷰의 <see cref="RectTransform"/>. 없으면 자동으로 캐싱한다.</summary>
        public RectTransform RectTransform => rectTransform != null ? rectTransform : rectTransform = transform as RectTransform;

        /// <summary>마지막으로 <see cref="Open"/> 호출 시 전달된 페이로드 객체.</summary>
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

        /// <summary>
        /// 이 뷰를 스스로 닫는다.
        /// 소유 서비스의 <see cref="UIService.Close(UIView)"/>를 호출한다.
        /// </summary>
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

        /// <summary>
        /// 최초 초기화 시 한 번만 호출된다.
        /// 하위 클래스에서 오버라이드하여 초기화 로직을 작성한다.
        /// </summary>
        protected virtual void OnInitialize()
        {
        }

        /// <summary>
        /// 열기 트랜지션이 시작되기 직전에 호출된다.
        /// </summary>
        /// <param name="payload">Open 호출 시 전달된 페이로드 객체.</param>
        protected virtual void OnBeforeOpen(object payload)
        {
        }

        /// <summary>
        /// 열기 트랜지션이 완전히 끝난 후 호출된다.
        /// </summary>
        /// <param name="payload">Open 호출 시 전달된 페이로드 객체.</param>
        protected virtual void OnOpened(object payload)
        {
        }

        /// <summary>
        /// 닫기 트랜지션이 시작되기 직전에 호출된다.
        /// </summary>
        protected virtual void OnBeforeClose()
        {
        }

        /// <summary>
        /// 닫기 트랜지션이 완전히 끝난 후 호출된다.
        /// </summary>
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
