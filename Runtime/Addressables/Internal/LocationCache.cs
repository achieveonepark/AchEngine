#if ACHENGINE_ADDRESSABLES
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace AchEngine.Assets.Internal
{
    using Addressables = UnityEngine.AddressableAssets.Addressables;

    /// <summary>
    /// 주소 또는 라벨로 조회한 Addressables 위치 메타데이터를 보관합니다.
    /// </summary>
    internal sealed class LocationCache
    {
        private readonly Dictionary<string, AsyncOperationHandle<IList<IResourceLocation>>> _locationHandles = new();

        public async Task<IList<IResourceLocation>> GetLocationsAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("주소 또는 라벨은 비어 있을 수 없습니다.", nameof(key));

            if (!_locationHandles.TryGetValue(key, out var handle) || !handle.IsValid())
            {
                handle = Addressables.LoadResourceLocationsAsync(key, typeof(UnityEngine.Object));
                _locationHandles[key] = handle;
            }

            var locations = await handle.Task;
            if (handle.Status == AsyncOperationStatus.Succeeded && locations != null)
                return locations;

            _locationHandles.Remove(key);
            if (handle.IsValid())
                Addressables.Release(handle);

            throw new InvalidOperationException($"Addressables 위치 조회에 실패했습니다. Key: {key}", handle.OperationException);
        }

        public void Clear()
        {
            foreach (var handle in _locationHandles.Values)
            {
                if (handle.IsValid())
                    Addressables.Release(handle);
            }

            _locationHandles.Clear();
        }
    }
}
#endif
