using UnityEngine;
using AchEngine;

namespace AchEngine.UI
{
    public class ObjectTouchManager : MonoSingleton<ObjectTouchManager>
    {
        private Camera _mainCamera;

        public override void InitializeSingleton()
        {
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            if (!Input.GetMouseButtonDown(0)) return;

            Vector2 pos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
            var hit = Physics2D.Raycast(pos, Vector2.zero);

            if (!hit.collider) return;
            if (hit.collider.gameObject.TryGetComponent<TouchableObject>(out var touchable))
                touchable.OnTouched();
        }
    }
}
