using UnityEngine;

namespace AchEngine.UI
{
    /// <summary>
    /// Screen.safeArea를 기준으로 RectTransform의 앵커를 자동으로 조정하는 컴포넌트입니다.
    /// 노치·펀치홀·홈 인디케이터 영역을 피하는 레이아웃에 사용합니다.
    /// </summary>
    [DisallowMultipleComponent]
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public sealed class UISafeAreaFitter : MonoBehaviour
    {
        [SerializeField] private RectTransform target;
        [SerializeField] private bool applyLeft = true;
        [SerializeField] private bool applyRight = true;
        [SerializeField] private bool applyTop = true;
        [SerializeField] private bool applyBottom = true;

        private Rect lastSafeArea;
        private Vector2Int lastScreenSize;
        private ScreenOrientation lastOrientation;

        private RectTransform Target => target != null ? target : target = GetComponent<RectTransform>();

        private void Reset()
        {
            target = GetComponent<RectTransform>();
        }

        private void OnEnable()
        {
            Refresh();
        }

        private void Update()
        {
            if (HasSafeAreaChanged())
            {
                Refresh();
            }
        }

        /// <summary>현재 Screen.safeArea 값을 기준으로 앵커를 즉시 재계산합니다.</summary>
        public void Refresh()
        {
            if (Screen.width <= 0 || Screen.height <= 0 || Target == null)
            {
                return;
            }

            var safeArea = Screen.safeArea;
            var min = safeArea.position;
            var max = safeArea.position + safeArea.size;

            min.x /= Screen.width;
            min.y /= Screen.height;
            max.x /= Screen.width;
            max.y /= Screen.height;

            var anchorMin = Target.anchorMin;
            var anchorMax = Target.anchorMax;

            if (applyLeft)
            {
                anchorMin.x = min.x;
            }

            if (applyBottom)
            {
                anchorMin.y = min.y;
            }

            if (applyRight)
            {
                anchorMax.x = max.x;
            }

            if (applyTop)
            {
                anchorMax.y = max.y;
            }

            Target.anchorMin = anchorMin;
            Target.anchorMax = anchorMax;
            Target.offsetMin = Vector2.zero;
            Target.offsetMax = Vector2.zero;

            lastSafeArea = safeArea;
            lastScreenSize = new Vector2Int(Screen.width, Screen.height);
            lastOrientation = Screen.orientation;
        }

        private bool HasSafeAreaChanged()
        {
            return lastSafeArea != Screen.safeArea
                || lastScreenSize.x != Screen.width
                || lastScreenSize.y != Screen.height
                || lastOrientation != Screen.orientation;
        }
    }
}
