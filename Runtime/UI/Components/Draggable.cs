using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AchEngine.UI
{
    /// <summary>
    /// 드래그 중 오브젝트를 월드 좌표로 이동시키고, 포인터 업 시 원래 위치로 되돌리는 컴포넌트입니다.
    /// </summary>
    public class Draggable : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        /// <summary>포인터 업 시 발생합니다. 드롭 위치의 Collider2D 배열을 전달합니다.</summary>
        public event Action<Collider2D[]> OnTouchUp;
        /// <summary>드래그 중 매 프레임 발생합니다. 현재 월드 좌표를 전달합니다.</summary>
        public event Action<Vector3> OnTouching;
        /// <summary>포인터 다운 시 발생합니다.</summary>
        public event Action OnTouchDown;

        protected Vector3 originalPos;

        private bool _isDragging;
        private Camera _mainCamera;

        protected virtual void Start()
        {
            _mainCamera = Camera.main;

            if (_mainCamera == null)
            {
                Debug.LogWarning("[Draggable] Main Camera를 찾지 못했습니다. 드래그 시 다시 탐색합니다.", this);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging) return;
            if (!TryGetMainCamera(out var camera)) return;

            var ray = camera.ScreenPointToRay(eventData.position);
            var dragPlane = new Plane(Vector3.forward, new Vector3(0f, 0f, originalPos.z));
            if (!dragPlane.Raycast(ray, out var distance)) return;

            var newPos = ray.GetPoint(distance);
            transform.position = newPos;
            OnTouching?.Invoke(newPos);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            _isDragging = true;
            originalPos = transform.position;
            OnTouchDown?.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left || !_isDragging) return;

            _isDragging = false;
            var dropPosition = transform.position;
            var colliders = Physics2D.OverlapCircleAll(dropPosition, 0.5f);
            transform.position = originalPos;
            OnTouchUp?.Invoke(colliders);
        }

        private bool TryGetMainCamera(out Camera camera)
        {
            if (_mainCamera == null)
                _mainCamera = Camera.main;

            camera = _mainCamera;
            return camera != null;
        }
    }
}
