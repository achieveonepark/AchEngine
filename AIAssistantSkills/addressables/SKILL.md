---
name: addressables
description: Use when the user asks to load, instantiate, unload, or download assets/scenes via Addressables in a project that has the AchEngine package installed. Use AchEngine's AddressableManager instead of calling UnityEngine.AddressableAssets.Addressables directly — it caches location metadata and asset handles behind a simple AchTask API.
---

# AchEngine Addressables 래퍼

`AchEngine.Assets` 네임스페이스 (`Runtime/Addressables/*`, `Editor/Addressables/*`). `com.unity.addressables`가 설치되어 있을 때만 컴파일된다(`#if ACHENGINE_ADDRESSABLES`). Addressable 에셋/씬을 다룰 때는 `UnityEngine.AddressableAssets.Addressables`를 직접 호출하지 말고 아래 진입점을 사용한다 — 위치 메타데이터와 로드 핸들을 캐싱하고 `AchTask`로 결과를 반환한다.

## `AddressableManager` (정적 API)

`AddressableManagerSettings.Instance.autoInitialize`가 켜져 있으면 `[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)]`에서 초기화를 시작한다. `LoadAsync` 계열은 초기화가 아직 끝나지 않았더라도 자동으로 대기한다.

- 초기화: `InitializeAsync()`(멱등), `IsInitialized`
- 로드: `LoadAsync<T>(address)`, `LoadAllAsync<T>(label)` — 같은 키는 같은 요청 타입의 로드 핸들을 재사용하며, 다른 타입의 요청에는 호환되지 않는 캐시 핸들을 재사용하지 않는다.
- 인스턴스화: `InstantiateAsync(address, parent=null, instantiateInWorldSpace=false)` + position/rotation 오버로드 — 프리팹을 캐시에서 로드한 뒤 생성한다. 생성한 인스턴스는 `ReleaseInstance()`로 제거하고, 원본 프리팹 캐시는 `Release(address)`로 해제한다.
- 해제: `Release(key)`, `ReleaseAll()`. 참조 카운트는 없으므로 같은 키를 공유하는 코드에서는 한 소유자만 해제 시점을 결정한다.
- 씬: `LoadSceneAsync(address, LoadSceneMode mode = Additive, activateOnLoad = true)` → `AchTask<SceneInstance>`, `UnloadSceneAsync(address)` → `AchTask<bool>`. 씬과 에셋 캐시는 명시적으로 해제한다.
- 조회/디버그: `IsLoaded(address)`.
- 원격 콘텐츠: `GetDownloadSizeAsync(label)`, `DownloadDependenciesAsync(label, Action<DownloadProgress> onProgress=null)`, `CheckForCatalogUpdatesAsync()`, `UpdateCatalogsAsync(IEnumerable<string>)`. 카탈로그 업데이트는 위치 메타데이터 캐시를 자동으로 비운다.
- 초기화·다운로드 크기·종속성 다운로드·카탈로그 확인·업데이트의 임시 Addressables 핸들은 작업 완료 후 자동 해제된다. 로드된 에셋과 씬은 사용이 끝난 뒤 `Release` 또는 `UnloadSceneAsync`로 명시적으로 해제한다.
- 설정: `AddressableManagerSettings`(`ScriptableObject`, `Resources/AddressableManagerSettings`) — `cloudProvider`(`AWSS3`/`GoogleCloudStorage`/`Custom`), `bucketName`/`bucketRegion`, `remoteCatalogUrl`/`remoteBundleUrl`, `autoInitialize`.

## 예시

```csharp
var sprite = await AddressableManager.LoadAsync<Sprite>("icon_sword");
var go = await AddressableManager.InstantiateAsync("enemy_prefab", parent: spawnPoint);
AddressableManager.ReleaseInstance(go);
AddressableManager.Release("enemy_prefab");
AddressableManager.Release("icon_sword");

var scene = await AddressableManager.LoadSceneAsync("Level1", LoadSceneMode.Additive);
// 나중에 씬 언로드
await AddressableManager.UnloadSceneAsync("Level1");
```

## 에디터 도구

`AchEngine/Addressables/Build Content` / `Clean Build` 메뉴, watched-folder 자동 마킹(`AddressableAutoMarker`), Project Settings의 클라우드/카탈로그 설정 UI.

## 참고 샘플

`Samples~/01_Addressables BasicUsage`(로드/캐시 재사용/인스턴스화/해제 흐름), `02_Addressables SceneManagement`(씬과 관련 에셋의 명시적 해제), `03_Addressables RemoteContent`(다운로드 사이즈 조회, 진행률 콜백, 카탈로그 업데이트).
