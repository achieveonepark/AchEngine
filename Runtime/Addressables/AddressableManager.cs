#if ACHENGINE_ADDRESSABLES
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AchEngine.Assets.Internal;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

#if ENABLE_UNITASK
using Cysharp.Threading.Tasks;
#endif

namespace AchEngine.Assets
{
    using Addressables = UnityEngine.AddressableAssets.Addressables;

    /// <summary>
    /// Addressable 에셋의 초기화, 로드, 해제를 단순하게 제공하는 정적 진입점입니다.
    /// </summary>
    public static class AddressableManager
    {
        private static readonly Dictionary<string, AsyncOperationHandle<SceneInstance>> SceneHandles = new();

        private static AddressableResourceProvider _resourceProvider;
        private static Task _initializationTask;

        /// <summary>
        /// Addressables 초기화가 완료되었는지 반환합니다.
        /// </summary>
        public static bool IsInitialized => _resourceProvider != null;

        /// <summary>
        /// 저수준 원격 콘텐츠 핸들이 필요한 경우 접근합니다.
        /// </summary>
        public static RemoteContentManager RemoteContent => RemoteContentManager.Instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetDomain()
        {
            _resourceProvider?.ReleaseAll();
            _resourceProvider = null;
            _initializationTask = null;
            SceneHandles.Clear();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInitialize()
        {
            if (AddressableManagerSettings.Instance.autoInitialize)
                _ = InitializeAsync();
        }

        /// <summary>
        /// Addressables를 초기화합니다. 동시에 여러 번 호출해도 하나의 초기화 작업만 수행합니다.
        /// </summary>
        public static AchTask InitializeAsync()
        {
            if (IsInitialized)
                return AchTask.CompletedTask;

            _initializationTask ??= InitializeCoreAsync();
            return ToAchTask(_initializationTask);
        }

        /// <summary>
        /// 주소의 에셋을 로드합니다. 첫 로드 후에는 같은 주소의 핸들을 재사용합니다.
        /// </summary>
        public static AchTask<T> LoadAsync<T>(string address)
            where T : UnityEngine.Object
        {
            return ToAchTask(LoadCoreAsync<T>(address));
        }

        /// <summary>
        /// 라벨 또는 주소에 일치하는 모든 에셋을 로드합니다.
        /// </summary>
        public static AchTask<T[]> LoadAllAsync<T>(string label)
            where T : UnityEngine.Object
        {
            return ToAchTask(LoadAllCoreAsync<T>(label));
        }

        /// <summary>
        /// 주소의 프리팹을 생성합니다. 프리팹 에셋은 캐시에 유지되며 Release로 해제합니다.
        /// </summary>
        public static AchTask<GameObject> InstantiateAsync(
            string address,
            Transform parent = null,
            bool instantiateInWorldSpace = false)
        {
            return ToAchTask(InstantiateCoreAsync(address, parent, instantiateInWorldSpace));
        }

        /// <summary>
        /// 주소의 프리팹을 지정한 위치와 회전값으로 생성합니다.
        /// </summary>
        public static AchTask<GameObject> InstantiateAsync(
            string address,
            Vector3 position,
            Quaternion rotation,
            Transform parent = null)
        {
            return ToAchTask(InstantiateCoreAsync(address, position, rotation, parent));
        }

        /// <summary>
        /// 생성한 인스턴스를 제거합니다. 원본 프리팹 캐시는 Release로 별도 해제합니다.
        /// </summary>
        public static void ReleaseInstance(GameObject instance)
        {
            if (instance != null)
                UnityEngine.Object.Destroy(instance);
        }

        /// <summary>
        /// 주소 또는 라벨에 연결된 캐시 핸들을 해제합니다.
        /// </summary>
        public static void Release(string key)
        {
            _resourceProvider?.Release(key);
        }

        /// <summary>
        /// 로드된 모든 에셋과 위치 메타데이터 캐시를 해제합니다.
        /// </summary>
        public static void ReleaseAll()
        {
            _resourceProvider?.ReleaseAll();
        }

        /// <summary>
        /// 주소가 성공적으로 로드되어 캐시에 유지되는지 반환합니다.
        /// </summary>
        public static bool IsLoaded(string address)
        {
            return _resourceProvider != null && _resourceProvider.IsLoaded(address);
        }

        /// <summary>
        /// Addressable 씬을 로드합니다. 같은 주소의 씬이 이미 로드 중이거나 로드되었다면 그 작업을 재사용합니다.
        /// </summary>
        public static AchTask<SceneInstance> LoadSceneAsync(
            string address,
            LoadSceneMode mode = LoadSceneMode.Additive,
            bool activateOnLoad = true)
        {
            return ToAchTask(LoadSceneCoreAsync(address, mode, activateOnLoad));
        }

        /// <summary>
        /// Addressable로 로드한 씬을 언로드합니다. 관리하지 않는 주소면 false를 반환합니다.
        /// </summary>
        public static AchTask<bool> UnloadSceneAsync(string address)
        {
            return ToAchTask(UnloadSceneCoreAsync(address));
        }

        /// <summary>
        /// 지정한 라벨의 다운로드 크기를 반환합니다.
        /// </summary>
        public static AchTask<long> GetDownloadSizeAsync(string label)
        {
            return ToAchTask(GetDownloadSizeCoreAsync(label));
        }

        /// <summary>
        /// 지정한 라벨의 종속성을 다운로드합니다.
        /// </summary>
        public static AchTask DownloadDependenciesAsync(
            string label,
            Action<DownloadProgress> onProgress = null)
        {
            return ToAchTask(DownloadDependenciesCoreAsync(label, onProgress));
        }

        /// <summary>
        /// 업데이트 가능한 카탈로그 목록을 반환합니다.
        /// </summary>
        public static AchTask<List<string>> CheckForCatalogUpdatesAsync()
        {
            return ToAchTask(CheckForCatalogUpdatesCoreAsync());
        }

        /// <summary>
        /// 카탈로그를 업데이트하고 이전 위치 메타데이터 캐시를 비웁니다.
        /// </summary>
        public static AchTask<List<IResourceLocator>> UpdateCatalogsAsync(IEnumerable<string> catalogs = null)
        {
            return ToAchTask(UpdateCatalogsCoreAsync(catalogs));
        }

        internal static IReadOnlyDictionary<string, HandleEntry> GetCacheSnapshot()
        {
            return _resourceProvider?.GetCacheSnapshot()
                   ?? new Dictionary<string, HandleEntry>();
        }

        private static async Task InitializeCoreAsync()
        {
            try
            {
                var handle = Addressables.InitializeAsync();
                await AwaitHandleAsync(handle, "Addressables 초기화");
                _resourceProvider = new AddressableResourceProvider();
                Debug.Log("[AchEngine Addressables] 초기화 완료");
            }
            finally
            {
                _initializationTask = null;
            }
        }

        private static async Task<T> LoadCoreAsync<T>(string address)
            where T : UnityEngine.Object
        {
            await EnsureInitializedAsync();
            return await _resourceProvider.LoadAsync<T>(address);
        }

        private static async Task<T[]> LoadAllCoreAsync<T>(string label)
            where T : UnityEngine.Object
        {
            await EnsureInitializedAsync();
            return await _resourceProvider.LoadAllAsync<T>(label);
        }

        private static async Task<GameObject> InstantiateCoreAsync(
            string address,
            Transform parent,
            bool instantiateInWorldSpace)
        {
            var prefab = await LoadCoreAsync<GameObject>(address);
            return UnityEngine.Object.Instantiate(prefab, parent, instantiateInWorldSpace);
        }

        private static async Task<GameObject> InstantiateCoreAsync(
            string address,
            Vector3 position,
            Quaternion rotation,
            Transform parent)
        {
            var prefab = await LoadCoreAsync<GameObject>(address);
            return UnityEngine.Object.Instantiate(prefab, position, rotation, parent);
        }

        private static async Task<SceneInstance> LoadSceneCoreAsync(
            string address,
            LoadSceneMode mode,
            bool activateOnLoad)
        {
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentException("씬 주소는 비어 있을 수 없습니다.", nameof(address));

            await EnsureInitializedAsync();

            if (SceneHandles.TryGetValue(address, out var cachedHandle) && cachedHandle.IsValid())
                return await AwaitHandleAsync(cachedHandle, $"씬 로드 ({address})");

            var handle = Addressables.LoadSceneAsync(address, mode, activateOnLoad);
            SceneHandles[address] = handle;

            try
            {
                return await AwaitHandleAsync(handle, $"씬 로드 ({address})");
            }
            catch
            {
                SceneHandles.Remove(address);
                if (handle.IsValid())
                    Addressables.Release(handle);
                throw;
            }
        }

        private static async Task<bool> UnloadSceneCoreAsync(string address)
        {
            if (!SceneHandles.TryGetValue(address, out var sceneHandle) || !sceneHandle.IsValid())
                return false;

            var unloadHandle = Addressables.UnloadSceneAsync(sceneHandle);
            await AwaitHandleAsync(unloadHandle, $"씬 언로드 ({address})");
            SceneHandles.Remove(address);
            return true;
        }

        private static async Task<long> GetDownloadSizeCoreAsync(string label)
        {
            await EnsureInitializedAsync();
            return await AwaitHandleAsync(Addressables.GetDownloadSizeAsync(label), $"다운로드 크기 조회 ({label})");
        }

        private static async Task DownloadDependenciesCoreAsync(
            string label,
            Action<DownloadProgress> onProgress)
        {
            await EnsureInitializedAsync();
            var handle = RemoteContent.DownloadDependenciesAsync(label, onProgress);
            await AwaitHandleAsync(handle, $"종속성 다운로드 ({label})");
        }

        private static async Task<List<string>> CheckForCatalogUpdatesCoreAsync()
        {
            await EnsureInitializedAsync();
            return await AwaitHandleAsync(Addressables.CheckForCatalogUpdates(), "카탈로그 업데이트 확인");
        }

        private static async Task<List<IResourceLocator>> UpdateCatalogsCoreAsync(IEnumerable<string> catalogs)
        {
            await EnsureInitializedAsync();
            var result = await AwaitHandleAsync(Addressables.UpdateCatalogs(catalogs), "카탈로그 업데이트");
            _resourceProvider?.ClearLocations();
            return result;
        }

        private static Task EnsureInitializedAsync()
        {
            return IsInitialized ? Task.CompletedTask : InitializeCoreAsyncOnce();
        }

        private static Task InitializeCoreAsyncOnce()
        {
            _initializationTask ??= InitializeCoreAsync();
            return _initializationTask;
        }

        private static async Task<T> AwaitHandleAsync<T>(AsyncOperationHandle<T> handle, string operation)
        {
            var result = await handle.Task;
            if (handle.Status == AsyncOperationStatus.Succeeded)
                return result;

            throw new InvalidOperationException($"{operation}에 실패했습니다.", handle.OperationException);
        }

        private static async Task AwaitHandleAsync(AsyncOperationHandle handle, string operation)
        {
            await handle.Task;
            if (handle.Status != AsyncOperationStatus.Succeeded)
                throw new InvalidOperationException($"{operation}에 실패했습니다.", handle.OperationException);
        }

        private static AchTask ToAchTask(Task task)
        {
#if ENABLE_UNITASK
            return AchTask.FromUniTask(task.AsUniTask());
#else
            return AchTask.FromTask(task);
#endif
        }

        private static AchTask<T> ToAchTask<T>(Task<T> task)
        {
#if ENABLE_UNITASK
            return AchTask<T>.FromUniTask(task.AsUniTask());
#else
            return AchTask<T>.FromTask(task);
#endif
        }
    }
}
#endif
