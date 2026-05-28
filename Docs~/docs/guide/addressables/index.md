# Addressables — 개요

AchEngine Addressables 모듈은 Unity Addressable Asset System을 래핑하여
**참조 카운팅 기반 캐싱**, **씬 단위 수명 주기 관리**, **감시 폴더 자동 그룹화**를 제공합니다.

:::info 선택적 모듈
`com.unity.addressables` 패키지가 설치된 경우에만 활성화됩니다.
**Project Settings › AchEngine** Overview에서 **설치** 버튼으로 바로 설치할 수 있습니다.
:::

## 핵심 구성 요소

| 클래스 | 역할 |
|---|---|
| `AddressableManager` | 에셋 로드/언로드, 참조 카운팅 |
| `AssetHandleCache` | 로드된 핸들 캐싱 |
| `SceneHandleTracker` | 씬별 핸들 추적 및 자동 해제 |
| `AddressableManagerSettings` | 런타임 설정 ScriptableObject |

## 기본 사용

```csharp
using AchEngine.Assets;

// 에셋 로드 (참조 카운트 +1)
var handle = await AddressableManager.LoadAsync<Sprite>("icon_sword");
spriteRenderer.sprite = handle.Result;

// 씬 로드
await AddressableManager.LoadSceneAsync("GameScene");

// 에셋 해제 (참조 카운트 -1, 0이 되면 실제 해제)
AddressableManager.Release("icon_sword");
```

### 다운로드 진행률

```csharp
var progress = new DownloadProgress();
progress.OnProgress += (downloaded, total) =>
    progressBar.value = downloaded / (float)total;

await AddressableManager.DownloadDependenciesAsync("remote_assets", progress);
```

## 다음 단계

- [감시 폴더 & 그룹](/guide/addressables/folders)
- [원격 콘텐츠](/guide/addressables/remote)
