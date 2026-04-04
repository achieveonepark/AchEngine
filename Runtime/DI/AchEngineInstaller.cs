using UnityEngine;

namespace AchEngine.DI
{
    /// <summary>
    /// 서비스를 AchEngine DI 컨테이너에 등록하는 기본 클래스.
    /// VContainer를 직접 사용하지 않아도 됩니다.
    ///
    /// 사용법:
    ///   1. 이 클래스를 상속한 컴포넌트를 만듭니다.
    ///   2. Install() 메서드에서 builder를 통해 서비스를 등록합니다.
    ///   3. AchEngineScope의 Installers 배열에 추가합니다.
    ///
    /// 예시:
    /// <code>
    /// public class GameInstaller : AchEngineInstaller
    /// {
    ///     public override void Install(IServiceBuilder builder)
    ///     {
    ///         builder.Register&lt;IGameService, GameService&gt;()
    ///                .Register&lt;IPlayerService, PlayerService&gt;(ServiceLifetime.Transient);
    ///     }
    /// }
    /// </code>
    /// </summary>
    public abstract class AchEngineInstaller : MonoBehaviour
    {
        public abstract void Install(IServiceBuilder builder);
    }
}
