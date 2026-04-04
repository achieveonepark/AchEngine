using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace AchEngine.Assets
{
    internal class SceneHandleTracker
    {
        private readonly Dictionary<Scene, HashSet<string>> _sceneAssets = new();

        public void Track(Scene scene, string address)
        {
            if (!_sceneAssets.TryGetValue(scene, out var addresses))
            {
                addresses = new HashSet<string>();
                _sceneAssets[scene] = addresses;
            }
            addresses.Add(address);
        }

        public void ReleaseAllForScene(Scene scene, AssetHandleCache cache)
        {
            if (!_sceneAssets.TryGetValue(scene, out var addresses))
                return;

            foreach (var address in addresses)
            {
                cache.ForceRelease(address);
            }

            _sceneAssets.Remove(scene);
        }

        public void Untrack(string address, Scene scene)
        {
            if (_sceneAssets.TryGetValue(scene, out var addresses))
            {
                addresses.Remove(address);
                if (addresses.Count == 0)
                    _sceneAssets.Remove(scene);
            }
        }

        public void Clear()
        {
            _sceneAssets.Clear();
        }
    }
}
