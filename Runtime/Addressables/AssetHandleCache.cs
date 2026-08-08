#if ACHENGINE_ADDRESSABLES
using System.Collections.Generic;
using AchEngine.Assets.Internal;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace AchEngine.Assets
{
    using Addressables = UnityEngine.AddressableAssets.Addressables;

    /// <summary>
    /// 단건 및 다건 에셋 로드 핸들을 주소별로 보관합니다.
    /// </summary>
    internal sealed class AssetHandleCache
    {
        private readonly Dictionary<string, HandleEntry> _assetHandles = new();
        private readonly Dictionary<string, HandleEntry> _multipleAssetHandles = new();

        public bool TryGetAssetHandle(string key, out AsyncOperationHandle<Object> handle)
        {
            if (_assetHandles.TryGetValue(key, out var entry) && entry.IsValid)
            {
                handle = entry.Handle.Convert<Object>();
                return true;
            }

            handle = default;
            return false;
        }

        public bool TryGetMultipleAssetHandle(string key, out AsyncOperationHandle<IList<Object>> handle)
        {
            if (_multipleAssetHandles.TryGetValue(key, out var entry) && entry.IsValid)
            {
                handle = entry.Handle.Convert<IList<Object>>();
                return true;
            }

            handle = default;
            return false;
        }

        public void AddAsset(string key, AsyncOperationHandle<Object> handle)
        {
            RemoveAsset(key);
            _assetHandles[key] = new HandleEntry(handle, key, typeof(Object));
        }

        public void AddAssets(string key, AsyncOperationHandle<IList<Object>> handle)
        {
            RemoveAssets(key);
            _multipleAssetHandles[key] = new HandleEntry(handle, key, typeof(Object));
        }

        public void Remove(string key)
        {
            if (_assetHandles.ContainsKey(key))
            {
                RemoveAsset(key);
                return;
            }

            RemoveAssets(key);
        }

        public void RemoveAsset(string key)
        {
            if (_assetHandles.TryGetValue(key, out var entry) && entry.IsValid)
                Addressables.Release(entry.Handle);

            _assetHandles.Remove(key);
        }

        public void RemoveAssets(string key)
        {
            if (_multipleAssetHandles.TryGetValue(key, out var entry) && entry.IsValid)
                Addressables.Release(entry.Handle);

            _multipleAssetHandles.Remove(key);
        }

        public bool IsLoaded(string key)
        {
            return _assetHandles.TryGetValue(key, out var entry)
                   && entry.IsValid
                   && entry.Handle.IsDone
                   && entry.Handle.Status == AsyncOperationStatus.Succeeded;
        }

        public IReadOnlyDictionary<string, HandleEntry> GetAllEntries()
        {
            var entries = new Dictionary<string, HandleEntry>(_assetHandles);
            foreach (var pair in _multipleAssetHandles)
                entries[pair.Key] = pair.Value;

            return entries;
        }

        public void Clear()
        {
            foreach (var entry in _assetHandles.Values)
            {
                if (entry.IsValid)
                    Addressables.Release(entry.Handle);
            }

            foreach (var entry in _multipleAssetHandles.Values)
            {
                if (entry.IsValid)
                    Addressables.Release(entry.Handle);
            }

            _assetHandles.Clear();
            _multipleAssetHandles.Clear();
        }
    }
}
#endif
