using AchEngine.DI;
#if ACHENGINE_UNITASK
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif

namespace AchEngine.Managers
{
    /// <summary>
    /// 모든 기본 매니저를 DI 컨테이너에 싱글톤으로 등록하는 인스톨러.
    ///
    /// 사용법: AchEngineScope의 Installers 배열에 이 컴포넌트를 추가하세요.
    /// 매니저 접근: ServiceLocator.Get&lt;ConfigManager&gt;() 등
    ///
    /// 원하는 매니저만 선택 등록하려면 이 클래스를 상속하여 Install()을 오버라이드하세요.
    /// </summary>
    public class AchManagerInstaller : AchEngineInstaller
    {
        public override void Install(IServiceBuilder builder)
        {
            builder
                .Register<ConfigManager>()
                .Register<SoundManager>()
                .Register<AchSceneManager>()
                .Register<InputManager>()
                .Register<TimeManager>()
                .Register<PoolManager>()
                .Register<PlayerManager>();
        }
    }
}
