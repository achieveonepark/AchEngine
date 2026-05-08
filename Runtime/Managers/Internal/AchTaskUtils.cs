using System.Threading.Tasks;
using UnityEngine;

namespace AchEngine
{
    internal static class AchTaskUtils
    {
        internal static async Task ToAchTask(this AsyncOperation op)
        {
            while (!op.isDone) await Task.Yield();
        }
    }
}
