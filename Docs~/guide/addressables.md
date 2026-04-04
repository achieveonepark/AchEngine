# Addressables

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

---

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

---

## 감시 폴더 & 그룹

감시 폴더를 등록하면 해당 폴더의 에셋이 지정한 Addressables 그룹에 자동으로 추가됩니다.

**Project Settings › AchEngine › Addressables › 감시 폴더** 섹션에서
**+ 폴더 추가** 버튼을 눌러 항목을 추가합니다.

### 감시 폴더 항목 구성

| 항목 | 설명 |
|---|---|
| **폴더 경로** | `Assets/` 로 시작하는 상대 경로 (예: `Assets/Art/Icons`) |
| **그룹 이름** | Addressables 그룹 이름 (없으면 자동 생성) |
| **주소 생성 방식** | 파일명 / 전체 경로 / GUID 중 선택 |
| **하위 폴더 포함** | 재귀적으로 하위 폴더 스캔 여부 |
| **라벨** | 쉼표로 구분된 Addressables 라벨 목록 |

### 주소 생성 방식 (AddressNamingMode)

| 값 | 생성되는 주소 예시 |
|---|---|
| `FileName` | `icon_sword` |
| `FullPath` | `Assets/Art/Icons/icon_sword.png` |
| `GUID` | `a1b2c3d4e5f6...` |

:::tip 자동 스캔
에셋을 추가하거나 삭제하면 Unity의 AssetPostprocessor가 자동으로 스캔을 트리거합니다.
:::

---

## 원격 콘텐츠 설정

**Project Settings › AchEngine › Addressables › 원격 구성** 섹션에서:

| 항목 | 설명 |
|---|---|
| **클라우드 공급자** | AWS S3, Google Cloud Storage, Azure Blob 등 |
| **버킷 URL** | 원격 에셋이 배포될 기본 URL |
| **빌드 경로** | 번들 파일 출력 경로 |
| **로드 경로** | 런타임에 번들을 내려받을 URL |

### 카탈로그 업데이트 확인

```csharp
// 새 카탈로그가 있으면 업데이트
var result = await AddressableManager.CheckForCatalogUpdatesAsync();
if (result.Count > 0)
{
    await AddressableManager.UpdateCatalogsAsync(result);
}
```

### 원격 에셋 크기 확인 후 다운로드

```csharp
long size = await AddressableManager.GetDownloadSizeAsync("remote_assets");
if (size > 0)
{
    // 사용자에게 다운로드 여부 확인 후
    await AddressableManager.DownloadDependenciesAsync("remote_assets");
}
```

---

## 빌드 설정

**Project Settings › AchEngine › Addressables › 빌드 설정**:

| 항목 | 설명 |
|---|---|
| **Play Mode Script** | Fast mode / Virtual mode / Packed mode 선택 |
| **빌드 후 자동 실행** | 빌드 완료 시 원격 서버 업로드 자동화 |
| **콘텐츠 빌드** | Addressables 번들 빌드 트리거 |

:::tip 개발 중 Fast Mode 사용
개발 중에는 Play Mode를 **Use Asset Database (Fast Mode)** 로 설정하면
빌드 없이 에셋을 직접 참조해 빠르게 반복 작업할 수 있습니다.
:::
