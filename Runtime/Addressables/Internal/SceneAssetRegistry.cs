#if ACHENGINE_ADDRESSABLES
using System.Collections.Generic;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace AchEngine.Assets.Internal
{
    internal class SceneAssetRegistry
    {
        private readonly Dictionary<string, AsyncOperationHandle<SceneInstance>> _sceneHandles = new();

        public void Register(string address, AsyncOperationHandle<SceneInstance> handle)
        {
            _sceneHandles[address] = handle;
        }

        public bool TryGet(string address, out AsyncOperationHandle<SceneInstance> handle)
        {
            return _sceneHandles.TryGetValue(address, out handle);
        }

        public void Remove(string address)
        {
            _sceneHandles.Remove(address);
        }

        public void Clear()
        {
            _sceneHandles.Clear();
        }

        public IReadOnlyDictionary<string, AsyncOperationHandle<SceneInstance>> GetAll()
        {
            return _sceneHandles;
        }
    }
}
#endif
