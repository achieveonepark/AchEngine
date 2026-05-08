using UnityEngine;
using UnityEngine.UI;

namespace AchEngine.UI
{
    [DisallowMultipleComponent]
    public sealed class RedDotBadge : MonoBehaviour
    {
        [SerializeField] private string _key;
        [SerializeField] private GameObject _dot;
        [SerializeField] private Text _countLabel;

        private void OnEnable()
        {
            if (string.IsNullOrEmpty(_key)) return;
            RedDot.Subscribe(_key, OnChanged);
            Refresh(RedDot.Get(_key));
        }

        private void OnDisable()
        {
            if (string.IsNullOrEmpty(_key)) return;
            RedDot.Unsubscribe(_key, OnChanged);
        }

        private void OnChanged(int count) => Refresh(count);

        private void Refresh(int count)
        {
            if (_dot != null)
                _dot.SetActive(count > 0);

            if (_countLabel != null)
                _countLabel.text = count > 1 ? count.ToString() : string.Empty;
        }
    }
}
