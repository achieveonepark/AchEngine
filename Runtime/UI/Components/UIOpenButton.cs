using UnityEngine;
using UnityEngine.UI;

namespace AchEngine.UI
{
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
