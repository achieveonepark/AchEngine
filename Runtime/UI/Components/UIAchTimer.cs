using UnityEngine;
using UnityEngine.UI;

namespace AchEngine.UI
{
    /// <summary>
    /// AchTimerHandle의 진행 상황을 Text와 Slider에 실시간으로 표시하는 컴포넌트.
    ///
    /// 사용 방법:
    ///   var timer = AchTimer.Start(5f);
    ///   GetComponent&lt;UIAchTimer&gt;().Bind(timer);
    ///   await timer;
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("AchEngine/UI/Ach Timer Display")]
    public sealed class UIAchTimer : MonoBehaviour
    {
        // 시간을 텍스트로 표시할 Text 컴포넌트 (선택)
        [SerializeField] private Text _timeText;

        // 진행률을 표시할 Slider 컴포넌트 (선택)
        [SerializeField] private Slider _progressSlider;

        // true면 남은 시간, false면 경과 시간을 표시한다
        [SerializeField] private bool _showRemaining = true;

        // 시간 표시 포맷 문자열. {0}이 시간(초) 값으로 대체된다.
        [SerializeField] private string _format = "{0:F1}";

        // 현재 바인딩된 타이머 핸들
        private AchTimerHandle _handle;

        private void Awake()
        {
            // 초기 상태에서는 비활성화. Bind() 호출 시 활성화된다.
            enabled = false;
        }

        /// <summary>
        /// 타이머 핸들을 연결해 실시간 업데이트를 시작한다.
        /// </summary>
        public void Bind(AchTimerHandle handle)
        {
            _handle = handle;
            enabled = true;
            Refresh();
        }

        /// <summary>
        /// 연결을 해제하고 업데이트를 중단한다.
        /// </summary>
        public void Unbind()
        {
            _handle = null;
            enabled = false;
        }

        private void Update()
        {
            if (_handle == null) return;

            Refresh();

            // 타이머가 완료되면 자동으로 연결 해제
            if (_handle.IsDone)
                Unbind();
        }

        /// <summary>
        /// 현재 핸들 값으로 Text와 Slider를 갱신한다.
        /// </summary>
        private void Refresh()
        {
            if (_handle == null) return;

            float value = _showRemaining ? _handle.Remaining : _handle.Elapsed;

            if (_timeText != null)
                _timeText.text = string.Format(_format, value);

            if (_progressSlider != null)
                _progressSlider.value = _handle.Progress;
        }
    }
}
