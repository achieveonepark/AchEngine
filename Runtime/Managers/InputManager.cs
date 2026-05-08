using System.Threading.Tasks;
using UnityEngine;

namespace AchEngine.Managers
{
    public class InputManager : IManager
    {
        public bool IsEnabled { get; private set; } = true;

        public Task Initialize()
        {
            IsEnabled = true;
            return Task.CompletedTask;
        }

        public void Enable()  { IsEnabled = true; }
        public void Disable() { IsEnabled = false; }

        public bool GetKey(KeyCode key)     => IsEnabled && Input.GetKey(key);
        public bool GetKeyDown(KeyCode key) => IsEnabled && Input.GetKeyDown(key);
        public bool GetKeyUp(KeyCode key)   => IsEnabled && Input.GetKeyUp(key);
    }
}
