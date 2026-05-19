using UnityEngine;
using UnityEngine.UI;

namespace AchEngine.UI
{
    /// <summary>버튼 클릭 시 지정한 ID의 UIView 또는 가장 가까운 부모 UIView를 닫는 컴포넌트입니다.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    public sealed class UICloseButton : MonoBehaviour
    {
        [SerializeField] private string viewId;
        [SerializeField] private bool closeNearestViewIfIdEmpty = true;
        [SerializeField] private Button button;

        private UIView parentView;

        private void Reset()
        {
            button = GetComponent<Button>();
        }

        private void Awake()
        {
            if (button == null)
            {
                button = GetComponent<Button>();
            }

            parentView = GetComponentInParent<UIView>();
        }

        private void OnEnable()
        {
            button.onClick.AddListener(HandleClick);
        }

        private void OnDisable()
        {
            button.onClick.RemoveListener(HandleClick);
        }

        /// <summary>스크립트에서 직접 닫기 동작을 트리거합니다.</summary>
        public void Close()
        {
            HandleClick();
        }

        private void HandleClick()
        {
            if (!string.IsNullOrWhiteSpace(viewId))
            {
                UI.Close(viewId);
                return;
            }

            if (closeNearestViewIfIdEmpty && parentView != null)
            {
                UI.Close(parentView);
                return;
            }

            Debug.LogWarning($"[{nameof(UICloseButton)}] No target view could be resolved.", this);
        }
    }
}
