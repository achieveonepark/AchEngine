using UnityEngine;
#if ACHENGINE_UNITASK
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif

namespace AchEngine.Managers
{
    public class InputManager : IManager
    {
        public bool IsEnabled { get; private set; } = true;

#if ACHENGINE_UNITASK
        public UniTask Initialize()
        {
            IsEnabled = true;
            return UniTask.CompletedTask;
        }
#else
        public Task Initialize()
        {
            IsEnabled = true;
            return Task.CompletedTask;
        }
#endif

        public void Enable()  { IsEnabled = true; }
        public void Disable() { IsEnabled = false; }

        public bool GetKey(KeyCode key)     => IsEnabled && Input.GetKey(key);
        public bool GetKeyDown(KeyCode key) => IsEnabled && Input.GetKeyDown(key);
        public bool GetKeyUp(KeyCode key)   => IsEnabled && Input.GetKeyUp(key);
    }
}
