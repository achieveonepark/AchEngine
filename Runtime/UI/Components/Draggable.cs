using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AchEngine.UI
{
    public class Draggable : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        public event Action<Collider2D[]> OnTouchUp;
        public event Action<Vector3> OnTouching;
        public event Action OnTouchDown;

        protected Vector3 originalPos;

        private bool _isDragging;
        private Camera _mainCamera;

        protected virtual void Start()
        {
            _mainCamera = Camera.main;
            if (!_mainCamera.TryGetComponent<Physics2DRaycaster>(out _))
                _mainCamera.AddComponent<Physics2DRaycaster>();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging) return;

            var newPos = _mainCamera.ScreenToWorldPoint(new Vector3(eventData.position.x, eventData.position.y, _mainCamera.nearClipPlane));
            newPos.z = 0;
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
            _isDragging = false;
            transform.position = originalPos;
            OnTouchUp?.Invoke(Physics2D.OverlapCircleAll(transform.position, 0.5f));
        }
    }
}
