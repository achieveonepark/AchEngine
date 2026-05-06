#if ACHENGINE_UNITASK
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif

namespace AchEngine.Managers
{
    public interface IManager
    {
#if ACHENGINE_UNITASK
        UniTask Initialize();
#else
        Task Initialize();
#endif
    }
}
