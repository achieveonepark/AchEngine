using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
#if ACHENGINE_UNITASK
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif

namespace AchEngine.Managers
{
    public class PoolManager : IManager
    {
        private readonly Dictionary<string, IObjectPool<GameObject>> _pools = new();

#if ACHENGINE_UNITASK
        public UniTask Initialize() => UniTask.CompletedTask;
#else
        public Task Initialize() => Task.CompletedTask;
#endif

        public void RegisterPool(string key, GameObject prefab, int defaultCapacity = 10, int maxSize = 100)
        {
            if (_pools.ContainsKey(key))
            {
                Debug.LogWarning($"[PoolManager] Pool '{key}' already registered.");
                return;
            }

            _pools[key] = new ObjectPool<GameObject>(
                createFunc:    () => Object.Instantiate(prefab),
                actionOnGet:   go => go.SetActive(true),
                actionOnRelease: go => go.SetActive(false),
                actionOnDestroy: Object.Destroy,
                collectionCheck: false,
                defaultCapacity: defaultCapacity,
                maxSize: maxSize
            );
        }

        public T Get<T>(string key) where T : Component
        {
            if (!_pools.TryGetValue(key, out var pool))
            {
                Debug.LogError($"[PoolManager] No pool registered for key '{key}'.");
                return null;
            }

            var go = pool.Get();
            return go.TryGetComponent<T>(out var component) ? component : null;
        }

        public GameObject Get(string key)
        {
            if (!_pools.TryGetValue(key, out var pool))
            {
                Debug.LogError($"[PoolManager] No pool registered for key '{key}'.");
                return null;
            }
            return pool.Get();
        }

        public void Release(string key, GameObject go)
        {
            if (_pools.TryGetValue(key, out var pool))
                pool.Release(go);
            else
                Object.Destroy(go);
        }

        public void ClearPool(string key)
        {
            if (_pools.TryGetValue(key, out var pool))
            {
                pool.Clear();
                _pools.Remove(key);
            }
        }

        public void ClearAll()
        {
            foreach (var pool in _pools.Values)
                pool.Clear();
            _pools.Clear();
        }
    }
}
