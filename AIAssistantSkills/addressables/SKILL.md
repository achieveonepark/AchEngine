---
name: addressables
description: Use when the user asks to load, instantiate, unload, or download assets/scenes via Addressables in a project that has the AchEngine package installed. Use AchEngine's AddressableManager instead of calling UnityEngine.AddressableAssets.Addressables directly — it adds ref-counted caching and automatic per-scene cleanup.
---

# AchEngine Addressables 래퍼

`AchEngine.Assets` 네임스페이스 (`Runtime/Addressables/*`, `Editor/Addressables/*`). `com.unity.addressables`가 설치되어 있을 때만 컴파일된다(`#if ACHENGINE_ADDRESSABLES`). Addressable 에셋/씬을 다룰 때는 `UnityEngine.AddressableAssets.Addressables`를 직접 호출하지 말고 아래 진입점을 사용한다 — 핸들 캐싱, 참조 카운팅, 씬 단위 자동 정리를 제공한다.

## `AddressableManager` (MonoBehaviour 싱글톤, `AddressableManager.Instance`)

`AddressableManagerSettings.Instance.autoInitialize`가 켜져 있으면 `[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)]`로 자동 생성된다.

- 초기화: `InitializeAsync()`(멱등), `IsInitialized`
- 별칭 등록: `RegisterAssetHandle(string key, string addressOrRuntimeKey)` / `RegisterAssetHandle(string key, AssetReference)`, `UnregisterAssetHandle`, `IsAssetHandleRegistered` — 주소를 하드코딩하지 않고 논리적 키로 로드 가능.
- 로드: `LoadAssetAsync<T>(string keyOrAddress)`, `LoadAssetAsync<T>(AssetReference)`, `LoadAssetsAsync<T>(label, Action<T> callback=null)` — 같은 키를 다시 로드하면 캐시된 핸들의 refcount만 증가한다(재로드 안 함).
- 인스턴스화: `InstantiateAsync(keyOrAddress, parent=null, instantiateInWorldSpace=false)` + position/rotation 오버로드 — 프리팹 로드+Instantiate를 묶어서 처리하고, 인스턴스가 씬에서 파괴될 때 핸들도 같이 정리되도록 내부 `ManagedInstanceTracker`를 자동 부착한다.
- 해제: `Release(string keyOrAddress)`, `Release(AssetReference)`, `ReleaseInstance(GameObject)`, `ReleaseAll()`.
- 씬: `LoadSceneAsync(address, LoadSceneMode mode = Additive, activateOnLoad = true)` → `AsyncOperationHandle<SceneInstance>`, `UnloadSceneAsync(address)` — 해당 씬이 활성 상태일 때 로드된 에셋 핸들을 자동 추적해 씬 언로드 시 같이 해제한다(`AddressableManagerSettings.autoReleaseOnSceneUnload`, 기본 true).
- 조회/디버그: `IsLoaded(keyOrAddress)`, `GetReferenceCount(keyOrAddress)`.
- 원격 콘텐츠: `RemoteContent` → `RemoteContentManager.Instance`의 `GetDownloadSizeAsync(label|keys)`, `DownloadDependenciesAsync(label|keys, Action<DownloadProgress> onProgress=null)`, `CheckForCatalogUpdatesAsync()`, `UpdateCatalogsAsync(List<string>)`.
- 설정: `AddressableManagerSettings`(`ScriptableObject`, `Resources/AddressableManagerSettings`) — `cloudProvider`(`AWSS3`/`GoogleCloudStorage`/`Custom`), `bucketName`/`bucketRegion`, `remoteCatalogUrl`/`remoteBundleUrl`, `autoInitialize`, `autoReleaseOnSceneUnload`.

## 예시

```csharp
var sprite = await AddressableManager.Instance.LoadAssetAsync<Sprite>("icon_sword");
var go = await AddressableManager.Instance.InstantiateAsync("enemy_prefab", parent: spawnPoint);
AddressableManager.Instance.ReleaseInstance(go);
AddressableManager.Instance.Release("icon_sword");

var handle = await AddressableManager.Instance.LoadSceneAsync("Level1", LoadSceneMode.Additive);
// ... 나중에
await AddressableManager.Instance.UnloadSceneAsync("Level1"); // 해당 씬 로드 중 얻은 에셋 핸들도 자동 해제
```

## 에디터 도구

`AchEngine/Addressables/Build Content` / `Clean Build` 메뉴, watched-folder 자동 마킹(`AddressableAutoMarker`), Project Settings의 클라우드/카탈로그 설정 UI.

## 참고 샘플

`Samples~/01_Addressables BasicUsage`(로드/refcount/인스턴스화/해제 흐름), `02_Addressables SceneManagement`(씬 로드 후 씬 스코프 자동 해제), `03_Addressables RemoteContent`(다운로드 사이즈 조회, 진행률 콜백, 카탈로그 업데이트).
