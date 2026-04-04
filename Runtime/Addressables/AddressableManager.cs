using System;
using System.Collections.Generic;
using AchEngine.Assets.Internal;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace AchEngine.Assets
{
    using Addressables = UnityEngine.AddressableAssets.Addressables;

    /// <summary>
    /// Addressable 에셋 로드를 위한 중앙 관리자입니다.
    /// 자동 핸들 캐싱, 참조 카운팅, 씬 단위 수명 주기 관리를 제공합니다.
    /// </summary>
    public class AddressableManager : MonoBehaviour
    {
        private sealed class InstanceEntry
        {
            public string CacheKey { get; }
            public AsyncOperationHandle<GameObject> InstanceHandle { get; }
            public ManagedInstanceTracker Tracker { get; }

            public InstanceEntry(
                string cacheKey,
                AsyncOperationHandle<GameObject> instanceHandle,
                ManagedInstanceTracker tracker)
            {
                CacheKey = cacheKey;
                InstanceHandle = instanceHandle;
                Tracker = tracker;
            }
        }

        private readonly struct ResolvedAssetKey
        {
            public string CacheKey { get; }
            public string RuntimeKey { get; }

            public ResolvedAssetKey(string cacheKey, string runtimeKey)
            {
                CacheKey = cacheKey;
                RuntimeKey = runtimeKey;
            }
        }

        private static AddressableManager _instance;
        private static bool _applicationQuitting;

        public static AddressableManager Instance
        {
            get
            {
                if (_applicationQuitting)
                    return null;

                if (_instance == null)
                {
                    _instance = FindObjectOfType<AddressableManager>();
                    if (_instance == null)
                    {
                        var go = new GameObject("[AddressableManager]");
                        _instance = go.AddComponent<AddressableManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        public bool IsInitialized { get; private set; }

        private readonly AssetHandleCache _handleCache = new();
        private readonly AssetHandleRegistry _assetHandleRegistry = new();
        private readonly SceneHandleTracker _sceneTracker = new();
        private readonly SceneAssetRegistry _sceneRegistry = new();
        private readonly Dictionary<GameObject, InstanceEntry> _instantiatedObjects = new();
        private AsyncOperationHandle<IResourceLocator>? _initHandle;

        /// <summary>
        /// 원격 콘텐츠 다운로드 작업에 접근합니다.
        /// </summary>
        public RemoteContentManager RemoteContent => RemoteContentManager.Instance;

        #region Unity Lifecycle

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            SceneManager.sceneUnloaded += OnSceneUnloaded;

            if (AddressableManagerSettings.Instance.autoInitialize)
                InitializeAsync();
        }

        private void OnDestroy()
        {
            SceneManager.sceneUnloaded -= OnSceneUnloaded;

            if (_instance == this)
            {
                ReleaseAll();
                _instance = null;
            }
        }

        private void OnApplicationQuit()
        {
            _applicationQuitting = true;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoCreate()
        {
            _applicationQuitting = false;
            if (AddressableManagerSettings.Instance.autoInitialize)
                _ = Instance;
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Addressables 시스템을 초기화합니다. 여러 번 호출해도 안전합니다.
        /// </summary>
        public AsyncOperationHandle<IResourceLocator> InitializeAsync()
        {
            if (_initHandle.HasValue && _initHandle.Value.IsValid())
                return _initHandle.Value;

            _initHandle = Addressables.InitializeAsync();
            _initHandle.Value.Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    IsInitialized = true;
                    Debug.Log("[AddressableManager] Initialized successfully.");
                }
                else
                {
                    Debug.LogError("[AddressableManager] Initialization failed.");
                }
            };

            return _initHandle.Value;
        }

        #endregion

        #region Asset Registration

        /// <summary>
        /// 문자열 키에 Addressable 주소 또는 런타임 키를 등록합니다.
        /// 이후 LoadAssetAsync / InstantiateAsync에서 이 키를 그대로 사용할 수 있습니다.
        /// </summary>
        public void RegisterAssetHandle(string key, string addressOrRuntimeKey)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or empty.", nameof(key));

            if (string.IsNullOrWhiteSpace(addressOrRuntimeKey))
                throw new ArgumentException("Address cannot be null or empty.", nameof(addressOrRuntimeKey));

            if (_handleCache.Contains(key))
            {
                throw new InvalidOperationException(
                    $"Cannot re-register asset handle '{key}' while it is still loaded. Release it first.");
            }

            if (_handleCache.Contains(addressOrRuntimeKey) && !string.Equals(key, addressOrRuntimeKey, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Runtime key '{addressOrRuntimeKey}' is already loaded without a registered alias. Release it before registering '{key}'.");
            }

            if (_assetHandleRegistry.TryGetKeyByRuntimeKey(addressOrRuntimeKey, out var existingKey)
                && existingKey != key)
            {
                throw new InvalidOperationException(
                    $"Runtime key '{addressOrRuntimeKey}' is already registered to '{existingKey}'.");
            }

            _assetHandleRegistry.Register(key, addressOrRuntimeKey);
        }

        /// <summary>
        /// 문자열 키에 AssetReference의 런타임 키를 등록합니다.
        /// 이후 LoadAssetAsync / InstantiateAsync에서 이 키를 그대로 사용할 수 있습니다.
        /// </summary>
        public void RegisterAssetHandle(string key, AssetReference reference)
        {
            if (reference == null)
                throw new ArgumentNullException(nameof(reference));

            var runtimeKey = reference.RuntimeKey?.ToString();
            if (string.IsNullOrWhiteSpace(runtimeKey))
                throw new ArgumentException("AssetReference runtime key is null or empty.", nameof(reference));

            RegisterAssetHandle(key, runtimeKey);
        }

        /// <summary>
        /// 등록된 에셋 핸들 키를 제거합니다.
        /// 이미 로드된 핸들이 있다면 캐시 키는 유지되며, 이후 재로드부터만 등록이 해제됩니다.
        /// </summary>
        public bool UnregisterAssetHandle(string key)
        {
            return _assetHandleRegistry.Unregister(key);
        }

        /// <summary>
        /// 문자열 키에 에셋 핸들이 등록되어 있는지 확인합니다.
        /// </summary>
        public bool IsAssetHandleRegistered(string key)
        {
            return _assetHandleRegistry.Contains(key);
        }

        #endregion

        #region Asset Loading

        /// <summary>
        /// 주소 또는 등록된 키로 에셋을 로드합니다. 핸들을 캐시하며, 이후 호출은
        /// 참조 카운트가 증가한 캐시 핸들을 반환합니다.
        /// </summary>
        public AsyncOperationHandle<T> LoadAssetAsync<T>(string keyOrAddress)
        {
            var resolved = ResolveAssetKey(keyOrAddress);
            return LoadAssetAsync<T>(resolved);
        }

        /// <summary>
        /// AssetReference로 에셋을 로드합니다.
        /// </summary>
        public AsyncOperationHandle<T> LoadAssetAsync<T>(AssetReference reference)
        {
            if (reference == null)
                throw new ArgumentNullException(nameof(reference));

            var runtimeKey = reference.RuntimeKey?.ToString();
            if (string.IsNullOrWhiteSpace(runtimeKey))
                throw new ArgumentException("AssetReference runtime key is null or empty.", nameof(reference));

            return LoadAssetAsync<T>(new ResolvedAssetKey(runtimeKey, runtimeKey));
        }

        /// <summary>
        /// 라벨 또는 키와 일치하는 여러 에셋을 로드합니다.
        /// </summary>
        public AsyncOperationHandle<IList<T>> LoadAssetsAsync<T>(string label, Action<T> callback = null)
        {
            return Addressables.LoadAssetsAsync(label, callback);
        }

        private AsyncOperationHandle<T> LoadAssetAsync<T>(ResolvedAssetKey resolved)
        {
            if (_handleCache.TryGet<T>(resolved.CacheKey, out var cachedHandle))
                return cachedHandle;

            var handle = Addressables.LoadAssetAsync<T>(resolved.RuntimeKey);
            var activeScene = SceneManager.GetActiveScene();

            handle.Completed += op =>
            {
                if (op.Status == AsyncOperationStatus.Succeeded)
                {
                    if (!_handleCache.Contains(resolved.CacheKey))
                    {
                        _handleCache.Add(resolved.CacheKey, op, activeScene);
                        if (AddressableManagerSettings.Instance.autoReleaseOnSceneUnload)
                            _sceneTracker.Track(activeScene, resolved.CacheKey);
                    }
                }
                else
                {
                    Debug.LogError(
                        $"[AddressableManager] Failed to load asset. Key: {resolved.CacheKey}, RuntimeKey: {resolved.RuntimeKey}");
                }
            };

            return handle;
        }

        #endregion

        #region Instantiation

        /// <summary>
        /// 주소 또는 등록된 키로 프리팹을 인스턴스화합니다.
        /// 내부적으로 캐시된 프리팹 핸들을 재사용합니다.
        /// </summary>
        public AsyncOperationHandle<GameObject> InstantiateAsync(
            string keyOrAddress,
            Transform parent = null,
            bool instantiateInWorldSpace = false)
        {
            var resolved = ResolveAssetKey(keyOrAddress);
            var prefabHandle = LoadAssetAsync<GameObject>(resolved);
            var instanceHandle = Addressables.ResourceManager.CreateChainOperation(prefabHandle, dependency =>
            {
                if (dependency.Status != AsyncOperationStatus.Succeeded)
                {
                    return Addressables.ResourceManager.CreateCompletedOperation<GameObject>(
                        null,
                        $"Prefab load failed for '{resolved.CacheKey}'.");
                }

                var prefab = dependency.Result;
                if (prefab == null)
                {
                    return Addressables.ResourceManager.CreateCompletedOperation<GameObject>(
                        null,
                        $"Loaded prefab is null for '{resolved.CacheKey}'.");
                }

                var instance = UnityEngine.Object.Instantiate(prefab, parent, instantiateInWorldSpace);
                return Addressables.ResourceManager.CreateCompletedOperation(instance, string.Empty);
            });

            instanceHandle.Completed += op =>
            {
                if (op.Status == AsyncOperationStatus.Succeeded && op.Result != null)
                {
                    RegisterManagedInstance(op.Result, resolved.CacheKey, op);
                }
                else
                {
                    Release(resolved.CacheKey);
                }
            };

            return instanceHandle;
        }

        /// <summary>
        /// 주소 또는 등록된 키로 프리팹을 특정 위치와 회전값으로 인스턴스화합니다.
        /// 내부적으로 캐시된 프리팹 핸들을 재사용합니다.
        /// </summary>
        public AsyncOperationHandle<GameObject> InstantiateAsync(
            string keyOrAddress,
            Vector3 position,
            Quaternion rotation,
            Transform parent = null)
        {
            var resolved = ResolveAssetKey(keyOrAddress);
            var prefabHandle = LoadAssetAsync<GameObject>(resolved);
            var instanceHandle = Addressables.ResourceManager.CreateChainOperation(prefabHandle, dependency =>
            {
                if (dependency.Status != AsyncOperationStatus.Succeeded)
                {
                    return Addressables.ResourceManager.CreateCompletedOperation<GameObject>(
                        null,
                        $"Prefab load failed for '{resolved.CacheKey}'.");
                }

                var prefab = dependency.Result;
                if (prefab == null)
                {
                    return Addressables.ResourceManager.CreateCompletedOperation<GameObject>(
                        null,
                        $"Loaded prefab is null for '{resolved.CacheKey}'.");
                }

                var instance = UnityEngine.Object.Instantiate(prefab, position, rotation, parent);
                return Addressables.ResourceManager.CreateCompletedOperation(instance, string.Empty);
            });

            instanceHandle.Completed += op =>
            {
                if (op.Status == AsyncOperationStatus.Succeeded && op.Result != null)
                {
                    RegisterManagedInstance(op.Result, resolved.CacheKey, op);
                }
                else
                {
                    Release(resolved.CacheKey);
                }
            };

            return instanceHandle;
        }

        private void RegisterManagedInstance(
            GameObject instance,
            string cacheKey,
            AsyncOperationHandle<GameObject> instanceHandle)
        {
            var tracker = instance.GetComponent<ManagedInstanceTracker>();
            if (tracker == null)
                tracker = instance.AddComponent<ManagedInstanceTracker>();

            tracker.Initialize(OnManagedInstanceDestroyed);
            _instantiatedObjects[instance] = new InstanceEntry(cacheKey, instanceHandle, tracker);
        }

        private void OnManagedInstanceDestroyed(GameObject instance)
        {
            if (instance == null)
                return;

            if (_instantiatedObjects.TryGetValue(instance, out var entry))
                CleanupInstanceEntry(instance, entry, destroyInstance: false);
        }

        private void CleanupInstanceEntry(GameObject instance, InstanceEntry entry, bool destroyInstance)
        {
            _instantiatedObjects.Remove(instance);

            if (entry.Tracker != null)
                entry.Tracker.SuppressDestroyCallback();

            if (destroyInstance && instance != null)
                Destroy(instance);

            _handleCache.Release(entry.CacheKey);

            if (entry.InstanceHandle.IsValid())
                entry.InstanceHandle.Release();
        }

        #endregion

        #region Release

        /// <summary>
        /// 주소 또는 등록된 키로 캐시된 에셋 핸들을 해제합니다.
        /// 참조 카운트를 감소시키며 0이 되면 실제로 해제합니다.
        /// </summary>
        public void Release(string keyOrAddress)
        {
            _handleCache.Release(ResolveCacheKey(keyOrAddress));
        }

        /// <summary>
        /// AssetReference로 캐시된 에셋 핸들을 해제합니다.
        /// </summary>
        public void Release(AssetReference reference)
        {
            if (reference == null)
                throw new ArgumentNullException(nameof(reference));

            var runtimeKey = reference.RuntimeKey?.ToString();
            if (string.IsNullOrWhiteSpace(runtimeKey))
                throw new ArgumentException("AssetReference runtime key is null or empty.", nameof(reference));

            Release(runtimeKey);
        }

        /// <summary>
        /// 인스턴스화된 GameObject를 해제합니다.
        /// 내부적으로 생성한 인스턴스라면 관련 핸들과 캐시 참조도 함께 정리합니다.
        /// </summary>
        public void ReleaseInstance(GameObject instance)
        {
            if (instance == null)
                return;

            if (_instantiatedObjects.TryGetValue(instance, out var entry))
            {
                CleanupInstanceEntry(instance, entry, destroyInstance: true);
                return;
            }

            Destroy(instance);
        }

        /// <summary>
        /// 모든 캐시 핸들과 인스턴스화된 오브젝트를 해제합니다.
        /// </summary>
        public void ReleaseAll()
        {
            var instances = new List<KeyValuePair<GameObject, InstanceEntry>>(_instantiatedObjects);
            foreach (var kvp in instances)
            {
                if (kvp.Key != null)
                    CleanupInstanceEntry(kvp.Key, kvp.Value, destroyInstance: true);
            }

            _handleCache.ReleaseAll();
            _sceneTracker.Clear();
            _sceneRegistry.Clear();
            _instantiatedObjects.Clear();
        }

        #endregion

        #region Scene Management

        /// <summary>
        /// Addressables를 통해 씬을 로드합니다.
        /// </summary>
        public AsyncOperationHandle<SceneInstance> LoadSceneAsync(
            string address,
            LoadSceneMode mode = LoadSceneMode.Additive,
            bool activateOnLoad = true)
        {
            var handle = Addressables.LoadSceneAsync(address, mode, activateOnLoad);

            handle.Completed += op =>
            {
                if (op.Status == AsyncOperationStatus.Succeeded)
                {
                    _sceneRegistry.Register(address, op);
                    Debug.Log($"[AddressableManager] Scene loaded: {address}");
                }
                else
                {
                    Debug.LogError($"[AddressableManager] Failed to load scene: {address}");
                }
            };

            return handle;
        }

        /// <summary>
        /// Addressables로 로드한 씬을 언로드합니다.
        /// 해당 씬이 활성 상태였을 때 로드된 모든 에셋 핸들을 자동으로 해제합니다.
        /// </summary>
        public AsyncOperationHandle<SceneInstance> UnloadSceneAsync(string address)
        {
            if (!_sceneRegistry.TryGet(address, out var sceneHandle))
            {
                Debug.LogError($"[AddressableManager] Scene not found in registry: {address}");
                return default;
            }

            var scene = sceneHandle.Result.Scene;
            var unloadHandle = Addressables.UnloadSceneAsync(sceneHandle);

            unloadHandle.Completed += op =>
            {
                if (op.Status == AsyncOperationStatus.Succeeded)
                {
                    _sceneRegistry.Remove(address);
                    Debug.Log($"[AddressableManager] Scene unloaded: {address}");
                }
            };

            return unloadHandle;
        }

        #endregion

        #region Query

        /// <summary>
        /// 주소 또는 등록된 키의 에셋이 현재 로드되어 캐시되어 있는지 확인합니다.
        /// </summary>
        public bool IsLoaded(string keyOrAddress)
        {
            return _handleCache.Contains(ResolveCacheKey(keyOrAddress));
        }

        /// <summary>
        /// 주소 또는 등록된 키의 현재 참조 카운트를 가져옵니다.
        /// </summary>
        public int GetReferenceCount(string keyOrAddress)
        {
            return _handleCache.GetReferenceCount(ResolveCacheKey(keyOrAddress));
        }

        /// <summary>
        /// 디버깅 및 대시보드용으로 모든 캐시 핸들 엔트리의 스냅샷을 가져옵니다.
        /// </summary>
        internal IReadOnlyDictionary<string, HandleEntry> GetCacheSnapshot()
        {
            return _handleCache.GetAllEntries();
        }

        #endregion

        #region Internal

        private ResolvedAssetKey ResolveAssetKey(string keyOrAddress)
        {
            if (_assetHandleRegistry.TryGetRuntimeKey(keyOrAddress, out var runtimeKey))
                return new ResolvedAssetKey(keyOrAddress, runtimeKey);

            if (_assetHandleRegistry.TryGetKeyByRuntimeKey(keyOrAddress, out var registeredKey))
                return new ResolvedAssetKey(registeredKey, keyOrAddress);

            return new ResolvedAssetKey(keyOrAddress, keyOrAddress);
        }

        private string ResolveCacheKey(string keyOrAddress)
        {
            if (_handleCache.Contains(keyOrAddress) || _assetHandleRegistry.Contains(keyOrAddress))
                return keyOrAddress;

            if (_assetHandleRegistry.TryGetKeyByRuntimeKey(keyOrAddress, out var registeredKey))
                return registeredKey;

            return keyOrAddress;
        }

        private void OnSceneUnloaded(Scene scene)
        {
            if (!AddressableManagerSettings.Instance.autoReleaseOnSceneUnload)
                return;

            _sceneTracker.ReleaseAllForScene(scene, _handleCache);
            Debug.Log($"[AddressableManager] Released assets for unloaded scene: {scene.name}");
        }

        #endregion
    }
}
