using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace AchEngine.UI
{
    /// <summary>
    /// UI 버튼을 길게 누르면 이벤트를 반복 발동시키는 컴포넌트.
    /// 초기 딜레이 이후 일정 간격으로 onHoldFire 이벤트를 반복 호출한다.
    /// </summary>
    [AddComponentMenu("AchEngine/UI/Ach Button Hold")]
    [DisallowMultipleComponent]
    public sealed class AchButtonHold : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        // 첫 번째 반복 이벤트가 발동되기까지의 초기 대기 시간(초)
        [SerializeField] private float _initialDelay = 0.5f;

        // 초기 딜레이 이후 각 반복 이벤트 사이의 간격(초)
        [SerializeField] private float _repeatInterval = 0.1f;

        // 반복 발동 시 실행되는 이벤트
        [SerializeField] private UnityEvent _onHoldFire;

        // 현재 홀드 반복 코루틴 참조
        private Coroutine _holdCoroutine;

        // 현재 홀드 중인지 여부
        private bool _isHolding;

        /// <summary>현재 버튼을 누르고 있는 상태인지 여부.</summary>
        public bool IsHolding => _isHolding;

        /// <summary>
        /// 포인터가 버튼 위에서 눌렸을 때 호출된다.
        /// 홀드 반복 코루틴을 시작한다.
        /// </summary>
        public void OnPointerDown(PointerEventData eventData)
        {
            if (_isHolding)
                return;

            _isHolding = true;
            _holdCoroutine = StartCoroutine(HoldRoutine());
        }

        /// <summary>
        /// 포인터가 버튼에서 떼어졌을 때 호출된다.
        /// 홀드 반복 코루틴을 정지한다.
        /// </summary>
        public void OnPointerUp(PointerEventData eventData)
        {
            StopHold();
        }

        private void OnDisable()
        {
            // 오브젝트가 비활성화될 때 진행 중인 홀드를 강제 종료한다
            StopHold();
        }

        /// <summary>
        /// 초기 딜레이 이후 반복 간격마다 onHoldFire를 발동하는 코루틴.
        /// </summary>
        private IEnumerator HoldRoutine()
        {
            // 초기 딜레이 대기
            yield return new WaitForSeconds(_initialDelay);

            // 포인터가 떼어질 때까지 반복 발동
            while (_isHolding)
            {
                _onHoldFire?.Invoke();
                yield return new WaitForSeconds(_repeatInterval);
            }
        }

        /// <summary>
        /// 홀드 상태를 종료하고 코루틴을 정지한다.
        /// </summary>
        private void StopHold()
        {
            _isHolding = false;

            if (_holdCoroutine != null)
            {
                StopCoroutine(_holdCoroutine);
                _holdCoroutine = null;
            }
        }
    }
}
