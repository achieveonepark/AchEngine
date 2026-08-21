using UnityEngine;
using AchEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

namespace AchEngine.UI
{
    public class ObjectTouchManager : MonoSingleton<ObjectTouchManager>
    {
        private Camera _mainCamera;

        public override void InitializeSingleton()
        {
            base.InitializeSingleton();
            _mainCamera = Camera.main;
        }

        private void Update()
        {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            var mouse = Mouse.current;
            if (mouse == null || !mouse.leftButton.wasPressedThisFrame) return;
            var screenPosition = mouse.position.ReadValue();
#else
            if (!Input.GetMouseButtonDown(0)) return;
            var screenPosition = (Vector2)Input.mousePosition;
#endif

            if (_mainCamera == null)
                _mainCamera = Camera.main;
            if (_mainCamera == null)
            {
                Debug.LogWarning("[ObjectTouchManager] Main Camera를 찾을 수 없습니다.", this);
                return;
            }

            Vector2 pos = _mainCamera.ScreenToWorldPoint(screenPosition);
            var hit = Physics2D.OverlapPoint(pos);

            if (hit == null) return;
            if (hit.gameObject.TryGetComponent<TouchableObject>(out var touchable))
                touchable.OnTouched();
        }
    }
}
