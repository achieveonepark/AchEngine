using System;
using System.Collections.Generic;
using AchEngine.Assets.Internal;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

namespace AchEngine.Assets
{
    using Addressables = UnityEngine.AddressableAssets.Addressables;

    internal class AssetHandleCache
    {
        private readonly Dictionary<string, HandleEntry> _cache = new();
        private readonly object _lock = new();

        public bool TryGet<T>(string key, out AsyncOperationHandle<T> handle)
        {
            lock (_lock)
            {
                if (_cache.TryGetValue(key, out var entry) && entry.IsValid)
                {
                    entry.ReferenceCount++;
                    handle = entry.Handle.Convert<T>();
                    return true;
                }
            }

            handle = default;
            return false;
        }

        public void Add<T>(string key, AsyncOperationHandle<T> handle, Scene? ownerScene)
        {
            lock (_lock)
            {
                var entry = new HandleEntry(handle, key, typeof(T), ownerScene);
                _cache[key] = entry;
            }
        }

        public bool Release(string key)
        {
            lock (_lock)
            {
                if (!_cache.TryGetValue(key, out var entry))
                    return false;

                entry.ReferenceCount--;
                if (entry.ReferenceCount <= 0)
                {
                    if (entry.IsValid)
                    {
                        Addressables.Release(entry.Handle);
                    }
                    _cache.Remove(key);
                    return true;
                }

                return false;
            }
        }

        public void ForceRelease(string key)
        {
            lock (_lock)
            {
                if (_cache.TryGetValue(key, out var entry))
                {
                    if (entry.IsValid)
                    {
                        Addressables.Release(entry.Handle);
                    }
                    _cache.Remove(key);
                }
            }
        }

        public void ReleaseAll()
        {
            lock (_lock)
            {
                foreach (var entry in _cache.Values)
                {
                    if (entry.IsValid)
                    {
                        Addressables.Release(entry.Handle);
                    }
                }
                _cache.Clear();
            }
        }

        public bool Contains(string key)
        {
            lock (_lock)
            {
                return _cache.ContainsKey(key);
            }
        }

        public int GetReferenceCount(string key)
        {
            lock (_lock)
            {
                return _cache.TryGetValue(key, out var entry) ? entry.ReferenceCount : 0;
            }
        }

        public IReadOnlyDictionary<string, HandleEntry> GetAllEntries()
        {
            lock (_lock)
            {
                return new Dictionary<string, HandleEntry>(_cache);
            }
        }
    }
}
