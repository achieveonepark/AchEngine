#if ACHENGINE_ADDRESSABLES
using UnityEngine.ResourceManagement.AsyncOperations;

namespace AchEngine.Assets.Internal
{
    /// <summary>
    /// 런타임 대시보드에 표시할 캐시 핸들 정보입니다.
    /// </summary>
    internal sealed class HandleEntry
    {
        public AsyncOperationHandle Handle { get; }
        public string Address { get; }
        public System.Type AssetType { get; }
        public float LoadTime { get; }
        public bool IsValid => Handle.IsValid();
        public bool IsComplete => IsValid && Handle.IsDone;
        public bool IsSucceeded => IsComplete && Handle.Status == AsyncOperationStatus.Succeeded;

        public HandleEntry(AsyncOperationHandle handle, string address, System.Type assetType)
        {
            Handle = handle;
            Address = address;
            AssetType = assetType;
            LoadTime = UnityEngine.Time.realtimeSinceStartup;
        }
    }
}
#endif
