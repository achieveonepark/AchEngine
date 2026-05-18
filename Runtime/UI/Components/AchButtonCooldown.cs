using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace AchEngine.UI
{
    /// <summary>
    /// UI 버튼에 쿨다운(재사용 대기시간)을 부여하는 컴포넌트.
    /// 클릭 직후 버튼을 비활성화하고, 지정된 시간이 지나면 다시 활성화한다.
    /// </summary>
    [AddComponentMenu("AchEngine/UI/Ach Button Cooldown")]
    [DisallowMultipleComponent]
    public sealed class AchButtonCooldown : MonoBehaviour
    {
        // 클릭 후 다음 클릭까지 허용하는 최소 대기 시간(초)
        [SerializeField] private float _cooldown = 1f;

        // 남은 쿨다운 시간을 레이블에 표시할지 여부
        [SerializeField] private bool _showCountdown = false;

        // 카운트다운을 표시할 Text 레이블 (선택 사항)
        [SerializeField] private Text _countdownLabel;

        // 쿨다운이 시작될 때 발동되는 이벤트
        [SerializeField] private UnityEvent _onCooldownStart;

        // 쿨다운이 끝날 때 발동되는 이벤트
        [SerializeField] private UnityEvent _onCooldownEnd;

        // 이 GameObject에 붙어 있는 Button 컴포넌트 참조
        private Button _button;

        // 현재 남은 쿨다운 시간 (0이면 쿨다운 없음)
        private float _remainingTime;

        // 현재 쿨다운이 진행 중인지 여부
        private bool _isCoolingDown;

        /// <summary>현재 쿨다운이 진행 중인지 여부.</summary>
        public bool IsCoolingDown => _isCoolingDown;

        private void Awake()
        {
            // 동일 GameObject에서 Button 컴포넌트를 가져와 클릭 이벤트를 구독한다
            _button = GetComponent<Button>();

            if (_button == null)
            {
                Debug.LogWarning($"[{nameof(AchButtonCooldown)}] Button 컴포넌트를 찾을 수 없습니다.", this);
                return;
            }

            _button.onClick.AddListener(HandleClick);
        }

        private void OnDestroy()
        {
            // 구독 해제로 메모리 누수 방지
            if (_button != null)
            {
                _button.onClick.RemoveListener(HandleClick);
            }
        }

        private void Update()
        {
            if (!_isCoolingDown)
                return;

            // 남은 시간을 프레임마다 감소
            _remainingTime -= Time.deltaTime;

            // 카운트다운 레이블 업데이트
            if (_showCountdown && _countdownLabel != null)
            {
                _countdownLabel.text = Mathf.CeilToInt(_remainingTime).ToString();
            }

            // 쿨다운 종료 확인
            if (_remainingTime <= 0f)
            {
                EndCooldown();
            }
        }

        /// <summary>
        /// 버튼 클릭 핸들러. 쿨다운이 없을 때만 쿨다운을 시작한다.
        /// </summary>
        private void HandleClick()
        {
            if (_isCoolingDown)
                return;

            StartCooldown();
        }

        /// <summary>
        /// 쿨다운을 수동으로 시작한다.
        /// 이미 쿨다운 중이면 타이머를 재시작한다.
        /// </summary>
        public void StartCooldown()
        {
            _isCoolingDown = true;
            _remainingTime = _cooldown;

            // 버튼 비활성화
            if (_button != null)
                _button.interactable = false;

            // 카운트다운 레이블 초기화
            if (_showCountdown && _countdownLabel != null)
                _countdownLabel.text = Mathf.CeilToInt(_remainingTime).ToString();

            _onCooldownStart?.Invoke();
        }

        /// <summary>
        /// 쿨다운을 즉시 초기화하고 버튼을 다시 활성화한다.
        /// </summary>
        public void ResetCooldown()
        {
            _isCoolingDown = false;
            _remainingTime = 0f;

            // 버튼 재활성화
            if (_button != null)
                _button.interactable = true;

            // 카운트다운 레이블 초기화
            if (_showCountdown && _countdownLabel != null)
                _countdownLabel.text = string.Empty;
        }

        /// <summary>
        /// 쿨다운이 자연스럽게 만료될 때 호출되는 내부 메서드.
        /// </summary>
        private void EndCooldown()
        {
            _isCoolingDown = false;
            _remainingTime = 0f;

            // 버튼 재활성화
            if (_button != null)
                _button.interactable = true;

            // 카운트다운 레이블 초기화
            if (_showCountdown && _countdownLabel != null)
                _countdownLabel.text = string.Empty;

            _onCooldownEnd?.Invoke();
        }
    }
}
