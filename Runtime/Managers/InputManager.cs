using System.Threading.Tasks;
using UnityEngine;

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
        public bool GetKey(KeyCode key)     => IsEnabled && Input.GetKey(key);

        /// <summary>
        /// 지정한 키가 이번 프레임에 눌렸는지 반환한다. 입력이 비활성화 상태면 항상 false.
        /// </summary>
        /// <param name="key">확인할 키 코드.</param>
        public bool GetKeyDown(KeyCode key) => IsEnabled && Input.GetKeyDown(key);

        /// <summary>
        /// 지정한 키가 이번 프레임에 떼어졌는지 반환한다. 입력이 비활성화 상태면 항상 false.
        /// </summary>
        /// <param name="key">확인할 키 코드.</param>
        public bool GetKeyUp(KeyCode key)   => IsEnabled && Input.GetKeyUp(key);
    }
}
