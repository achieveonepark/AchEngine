using UnityEngine;
using UnityEngine.UI;

namespace AchEngine.UI
{
    /// <summary>버튼 클릭 시 지정한 ID의 UIView를 여는 컴포넌트입니다.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    public sealed class UIOpenButton : MonoBehaviour
    {
        [SerializeField] private string viewId;
        [SerializeField] private bool closeCurrentViewFirst;
        [SerializeField] private Button button;

        private UIView ownerView;

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

            ownerView = GetComponentInParent<UIView>();
        }

        private void OnEnable()
        {
            button.onClick.AddListener(HandleClick);
        }

        private void OnDisable()
        {
            button.onClick.RemoveListener(HandleClick);
        }

        /// <summary>스크립트에서 직접 열기 동작을 트리거합니다.</summary>
        public void Open()
        {
            HandleClick();
        }

        private void HandleClick()
        {
            if (string.IsNullOrWhiteSpace(viewId))
            {
                Debug.LogWarning($"[{nameof(UIOpenButton)}] Missing view id.", this);
                return;
            }

            if (closeCurrentViewFirst && ownerView != null)
            {
                UI.Close(ownerView);
            }

            UI.Show(viewId);
        }
    }
}
