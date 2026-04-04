using System;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

namespace AchEngine.Assets.Internal
{
    internal class HandleEntry
    {
        public AsyncOperationHandle Handle { get; }
        public string Address { get; }
        public Type AssetType { get; }
        public int ReferenceCount { get; set; }
        public Scene? OwnerScene { get; set; }
        public float LoadTime { get; }
        public bool IsValid => Handle.IsValid();

        public HandleEntry(AsyncOperationHandle handle, string address, Type assetType, Scene? ownerScene)
        {
            Handle = handle;
            Address = address;
            AssetType = assetType;
            ReferenceCount = 1;
            OwnerScene = ownerScene;
            LoadTime = UnityEngine.Time.realtimeSinceStartup;
        }
    }
}
