# 원격 콘텐츠

## 원격 콘텐츠 설정

**Project Settings › AchEngine › Addressables › 원격 구성** 섹션에서:

| 항목 | 설명 |
|---|---|
| **클라우드 공급자** | AWS S3, Google Cloud Storage, Azure Blob 등 |
| **버킷 URL** | 원격 에셋이 배포될 기본 URL |
| **빌드 경로** | 번들 파일 출력 경로 |
| **로드 경로** | 런타임에 번들을 내려받을 URL |

## 카탈로그 업데이트 확인

```csharp
// 새 카탈로그가 있으면 업데이트
var result = await AddressableManager.CheckForCatalogUpdatesAsync();
if (result.Count > 0)
{
    await AddressableManager.UpdateCatalogsAsync(result);
}
```

## 원격 에셋 크기 확인 후 다운로드

```csharp
long size = await AddressableManager.GetDownloadSizeAsync("remote_assets");
if (size > 0)
{
    // 사용자에게 다운로드 여부 확인 후
    await AddressableManager.DownloadDependenciesAsync("remote_assets");
}
```

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
