using UnityEngine;
#if ACHENGINE_UNITASK
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif

namespace AchEngine
{
    internal static class AchTaskUtils
    {
#if ACHENGINE_UNITASK
        internal static async AchTask ToAchTask(this AsyncOperation op) => await op.ToUniTask();
#else
        internal static async AchTask ToAchTask(this AsyncOperation op)
        {
            while (!op.isDone) await Task.Yield();
        }
#endif
    }
}
