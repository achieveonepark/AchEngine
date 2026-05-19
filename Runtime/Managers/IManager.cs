using System.Threading.Tasks;

namespace AchEngine.Managers
{
    /// <summary>
    /// AchEngine의 모든 매니저가 구현해야 하는 기본 인터페이스.
    /// </summary>
    public interface IManager
    {
        /// <summary>
        /// 매니저를 비동기로 초기화한다.
        /// </summary>
        Task Initialize();
    }
}
