# 원격 콘텐츠

AWS S3, Google Cloud Storage 등 클라우드 스토리지에 에셋을 호스팅하여
앱 업데이트 없이 콘텐츠를 배포할 수 있습니다.

## 원격 구성

**Project Settings › AchEngine › Addressables › 원격 구성** 에서 설정합니다.

| 항목 | 설명 |
|---|---|
| **클라우드 공급자** | AWS S3 / Google Cloud Storage / Custom |
| **버킷 이름** | 클라우드 스토리지 버킷 이름 |
| **버킷 리전** | AWS 리전 (예: `ap-northeast-2`) |
| **카탈로그 URL** | 원격 카탈로그 파일 URL |
| **번들 URL** | 번들 파일 기본 URL |

클라우드 공급자와 버킷 정보를 입력하면 **생성된 URL** 이 자동으로 표시됩니다.

```
AWS S3:
  https://{버킷명}.s3.{리전}.amazonaws.com/[BuildTarget]

Google Cloud Storage:
  https://storage.googleapis.com/{버킷명}/[BuildTarget]
```

## 빌드 설정

| 항목 | 설명 |
|---|---|
| **플레이어 빌드 전 자동 빌드** | Player Build 시 Addressables를 자동으로 먼저 빌드 |
| **플레이 모드에서 기존 빌드 강제 사용** | 에디터 플레이 시 빌드된 번들 사용 (개발 환경 재현) |

## 빌드 실행

| 버튼 | 동작 |
|---|---|
| **콘텐츠 빌드** | 변경된 에셋만 빌드 |
| **클린 빌드** | 모든 캐시를 제거하고 전체 빌드 |

에디터 메뉴 **AchEngine › Addressables › Build Content** 로도 실행할 수 있습니다.

## 원격 다운로드 코드

```csharp
using AchEngine.Assets;

// 원격 카탈로그 업데이트 확인
var catalogsToUpdate = await AddressableManager.CheckForCatalogUpdates();

if (catalogsToUpdate.Count > 0)
{
    // 카탈로그 업데이트
    await AddressableManager.UpdateCatalogs(catalogsToUpdate);

    // 다운로드 크기 확인
    var size = await AddressableManager.GetDownloadSize("remote_content");
    Debug.Log($"다운로드 크기: {size / 1024 / 1024f:F1} MB");

    // 다운로드 진행
    var progress = new DownloadProgress();
    progress.OnProgress += (current, total) => { /* UI 업데이트 */ };
    await AddressableManager.DownloadDependenciesAsync("remote_content", progress);
}
```
