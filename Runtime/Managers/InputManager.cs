using System.Threading.Tasks;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif

namespace AchEngine.Managers
{
    /// <summary>
    /// 입력 활성화 상태를 관리하고 Unity Input을 래핑하는 입력 매니저.
    /// 비활성화 상태에서는 모든 입력 쿼리가 false를 반환한다.
    /// </summary>
    public class InputManager : IManager
    {
        /// <summary>
        /// 입력이 활성화되어 있는지 여부.
        /// </summary>
        public bool IsEnabled { get; private set; } = true;

        /// <summary>
        /// 초기화. 입력을 활성화 상태로 설정한다.
        /// </summary>
        public Task Initialize()
        {
            IsEnabled = true;
            return Task.CompletedTask;
        }

        /// <summary>
        /// 입력을 활성화한다.
        /// </summary>
        public void Enable()  { IsEnabled = true; }

        /// <summary>
        /// 입력을 비활성화한다. 비활성화 중에는 모든 입력이 무시된다.
        /// </summary>
        public void Disable() { IsEnabled = false; }

        /// <summary>
        /// 지정한 키가 현재 눌려 있는지 반환한다. 입력이 비활성화 상태면 항상 false.
        /// </summary>
        /// <param name="key">확인할 키 코드.</param>
        public bool GetKey(KeyCode key)
        {
            if (!IsEnabled) return false;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            return TryGetKeyControl(key, out var control) && control.isPressed;
#else
            return Input.GetKey(key);
#endif
        }

        /// <summary>
        /// 지정한 키가 이번 프레임에 눌렸는지 반환한다. 입력이 비활성화 상태면 항상 false.
        /// </summary>
        /// <param name="key">확인할 키 코드.</param>
        public bool GetKeyDown(KeyCode key)
        {
            if (!IsEnabled) return false;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            return TryGetKeyControl(key, out var control) && control.wasPressedThisFrame;
#else
            return Input.GetKeyDown(key);
#endif
        }

        /// <summary>
        /// 지정한 키가 이번 프레임에 떼어졌는지 반환한다. 입력이 비활성화 상태면 항상 false.
        /// </summary>
        /// <param name="key">확인할 키 코드.</param>
        public bool GetKeyUp(KeyCode key)
        {
            if (!IsEnabled) return false;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            return TryGetKeyControl(key, out var control) && control.wasReleasedThisFrame;
#else
            return Input.GetKeyUp(key);
#endif
        }

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        private static bool TryGetKeyControl(KeyCode keyCode, out KeyControl control)
        {
            control = null;
            var keyboard = Keyboard.current;
            if (keyboard == null) return false;

            var keyName = keyCode switch
            {
                KeyCode.Alpha0 => nameof(Key.Digit0),
                KeyCode.Alpha1 => nameof(Key.Digit1),
                KeyCode.Alpha2 => nameof(Key.Digit2),
                KeyCode.Alpha3 => nameof(Key.Digit3),
                KeyCode.Alpha4 => nameof(Key.Digit4),
                KeyCode.Alpha5 => nameof(Key.Digit5),
                KeyCode.Alpha6 => nameof(Key.Digit6),
                KeyCode.Alpha7 => nameof(Key.Digit7),
                KeyCode.Alpha8 => nameof(Key.Digit8),
                KeyCode.Alpha9 => nameof(Key.Digit9),
                KeyCode.Keypad0 => nameof(Key.Numpad0),
                KeyCode.Keypad1 => nameof(Key.Numpad1),
                KeyCode.Keypad2 => nameof(Key.Numpad2),
                KeyCode.Keypad3 => nameof(Key.Numpad3),
                KeyCode.Keypad4 => nameof(Key.Numpad4),
                KeyCode.Keypad5 => nameof(Key.Numpad5),
                KeyCode.Keypad6 => nameof(Key.Numpad6),
                KeyCode.Keypad7 => nameof(Key.Numpad7),
                KeyCode.Keypad8 => nameof(Key.Numpad8),
                KeyCode.Keypad9 => nameof(Key.Numpad9),
                KeyCode.KeypadEnter => nameof(Key.NumpadEnter),
                KeyCode.Return => nameof(Key.Enter),
                KeyCode.LeftControl => nameof(Key.LeftCtrl),
                KeyCode.RightControl => nameof(Key.RightCtrl),
                _ => keyCode.ToString()
            };

            if (!System.Enum.TryParse(keyName, out Key key) || key == Key.None)
                return false;

            control = keyboard[key];
            return control != null;
        }
#endif
    }
}
