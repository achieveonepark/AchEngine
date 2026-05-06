#if ACHENGINE_UNITASK
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif

namespace AchEngine.Managers
{
    public interface IScene
    {
#if ACHENGINE_UNITASK
        UniTask OnSceneStart();
        UniTask OnSceneEnd();
#else
        Task OnSceneStart();
        Task OnSceneEnd();
#endif
    }
}
