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

        /// <summary>클릭 시 해당 키의 레드닷을 자동으로 지울지 여부.</summary>
        [SerializeField] private bool _clearOnClick = true;

        /// <summary>클릭 이벤트를 받을 버튼. null이면 자동 클리어 기능이 동작하지 않는다.</summary>
        [SerializeField] private Button _button;

        private void OnEnable()
        {
            if (string.IsNullOrEmpty(_key)) return;
            RedDot.Subscribe(_key, OnChanged);
            Refresh(RedDot.Get(_key));

            // 버튼이 연결되어 있고 클릭 클리어가 활성화된 경우 리스너 등록
            if (_clearOnClick && _button != null)
                _button.onClick.AddListener(OnClicked);
        }

        private void OnDisable()
        {
            if (string.IsNullOrEmpty(_key)) return;
            RedDot.Unsubscribe(_key, OnChanged);

            // 버튼 리스너 해제
            if (_clearOnClick && _button != null)
                _button.onClick.RemoveListener(OnClicked);
        }

        private void OnChanged(int count) => Refresh(count);

        private void Refresh(int count)
        {
            if (_dot != null)
                _dot.SetActive(count > 0);

            if (_countLabel != null)
                _countLabel.text = count > 1 ? count.ToString() : string.Empty;
        }

        /// <summary>버튼 클릭 시 해당 키의 레드닷을 초기화한다.</summary>
        private void OnClicked()
        {
            if (_clearOnClick)
                RedDot.Clear(_key);
        }
    }
}
