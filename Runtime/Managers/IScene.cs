using System.Threading.Tasks;

namespace AchEngine.Managers
{
    /// <summary>
    /// AchSceneManager가 씬 전환 시 호출하는 씬 라이프사이클 인터페이스.
    /// </summary>
    public interface IScene
    {
        /// <summary>
        /// 씬이 로드되고 준비가 완료되었을 때 호출된다.
        /// </summary>
        Task OnSceneStart();

        /// <summary>
        /// 씬이 언로드되기 직전에 호출된다.
        /// </summary>
        Task OnSceneEnd();
    }
}
