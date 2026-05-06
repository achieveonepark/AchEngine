#if ACHENGINE_ADDRESSABLES
using System;
using UnityEngine;

namespace AchEngine.Assets.Internal
{
    internal class ManagedInstanceTracker : MonoBehaviour
    {
        private Action<GameObject> _onDestroyed;
        private bool _suppressDestroyCallback;

        public void Initialize(Action<GameObject> onDestroyed)
        {
            _onDestroyed = onDestroyed;
            _suppressDestroyCallback = false;
        }

        public void SuppressDestroyCallback()
        {
            _suppressDestroyCallback = true;
        }

        private void OnDestroy()
        {
            if (_suppressDestroyCallback)
                return;

            _onDestroyed?.Invoke(gameObject);
        }
    }
}
#endif
