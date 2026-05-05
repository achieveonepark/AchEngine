using UnityEngine;
using UnityEngine.UI;

namespace AchEngine.UI
{
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
