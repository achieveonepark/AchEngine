#if ACHENGINE_ADDRESSABLES
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace AchEngine.Assets.Internal
{
    using Addressables = UnityEngine.AddressableAssets.Addressables;
    using Object = UnityEngine.Object;

    /// <summary>
    /// 위치 메타데이터와 에셋 핸들을 분리해 관리하는 Addressables 로드 구현입니다.
    /// </summary>
    internal sealed class AddressableResourceProvider
    {
        private readonly AssetHandleCache _assetCache = new();
        private readonly LocationCache _locationCache = new();

        public async Task<T> LoadAsync<T>(string address)
            where T : Object
        {
            var asset = await LoadObjectAsync(address);
            if (asset is T typed)
                return typed;

            throw new InvalidCastException($"주소 '{address}'의 에셋을 {typeof(T).Name}(으)로 변환할 수 없습니다.");
        }

        public async Task<T[]> LoadAllAsync<T>(string label)
            where T : Object
        {
            if (string.IsNullOrWhiteSpace(label))
                throw new ArgumentException("라벨은 비어 있을 수 없습니다.", nameof(label));

            if (_assetCache.TryGetMultipleAssetHandle(label, out var cachedHandle))
                return ToTypedArray<T>(await AwaitHandleAsync(cachedHandle, $"다건 에셋 로드 ({label})"));

            var locations = await _locationCache.GetLocationsAsync(label);
            if (locations.Count == 0)
                return Array.Empty<T>();

            if (_assetCache.TryGetMultipleAssetHandle(label, out cachedHandle))
                return ToTypedArray<T>(await AwaitHandleAsync(cachedHandle, $"다건 에셋 로드 ({label})"));

            var handle = Addressables.LoadAssetsAsync<Object>(locations, null);
            _assetCache.AddAssets(label, handle);

            try
            {
                return ToTypedArray<T>(await AwaitHandleAsync(handle, $"다건 에셋 로드 ({label})"));
            }
            catch
            {
                _assetCache.RemoveAssets(label);
                throw;
            }
        }

        public void Release(string key)
        {
            if (!string.IsNullOrWhiteSpace(key))
                _assetCache.Remove(key);
        }

        public void ReleaseAll()
        {
            _assetCache.Clear();
            _locationCache.Clear();
        }

        public void ClearLocations()
        {
            _locationCache.Clear();
        }

        public bool IsLoaded(string address)
        {
            return _assetCache.IsLoaded(address);
        }

        public IReadOnlyDictionary<string, HandleEntry> GetCacheSnapshot()
        {
            return _assetCache.GetAllEntries();
        }

        private async Task<Object> LoadObjectAsync(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentException("에셋 주소는 비어 있을 수 없습니다.", nameof(address));

            if (_assetCache.TryGetAssetHandle(address, out var cachedHandle))
                return await AwaitHandleAsync(cachedHandle, $"에셋 로드 ({address})");

            var location = SelectLocation(await _locationCache.GetLocationsAsync(address), address);
            if (location == null)
                throw new InvalidOperationException($"Addressable 위치를 찾을 수 없습니다. Address: {address}");

            if (_assetCache.TryGetAssetHandle(address, out cachedHandle))
                return await AwaitHandleAsync(cachedHandle, $"에셋 로드 ({address})");

            var handle = Addressables.LoadAssetAsync<Object>(location);
            _assetCache.AddAsset(address, handle);

            try
            {
                return await AwaitHandleAsync(handle, $"에셋 로드 ({address})");
            }
            catch
            {
                _assetCache.RemoveAsset(address);
                throw;
            }
        }

        private static IResourceLocation SelectLocation(IList<IResourceLocation> locations, string address)
        {
            foreach (var location in locations)
            {
                if (location != null && string.Equals(location.PrimaryKey, address, StringComparison.Ordinal))
                    return location;
            }

            return locations.FirstOrDefault(location => location != null);
        }

        private static T[] ToTypedArray<T>(IList<Object> assets)
            where T : Object
        {
            return assets.OfType<T>().ToArray();
        }

        private static async Task<T> AwaitHandleAsync<T>(AsyncOperationHandle<T> handle, string operation)
        {
            var result = await handle.Task;
            if (handle.Status == AsyncOperationStatus.Succeeded)
                return result;

            throw new InvalidOperationException($"{operation}에 실패했습니다.", handle.OperationException);
        }
    }
}
#endif
