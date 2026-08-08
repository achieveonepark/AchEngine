# 03 Remote Content

이 샘플은 원격 콘텐츠 다운로드와 카탈로그 업데이트 흐름을 확인하기 위한 예제입니다.

## 포함 내용

- `GetDownloadSizeAsync()`로 현재 필요한 다운로드 크기 확인
- `DownloadDependenciesAsync()`로 라벨 기반 원격 콘텐츠 다운로드
- `DownloadProgress`를 UI에 표시하기
- `CheckForCatalogUpdatesAsync()`와 `UpdateCatalogsAsync()`로 카탈로그 갱신 확인

## 준비

1. Unity Addressables Groups에서 하나 이상의 그룹을 Remote Build/Load Path로 설정합니다.
2. 다운로드 대상으로 사용할 에셋에 라벨을 지정합니다.
3. 원격 번들을 실제로 접근 가능한 위치에 배포합니다.
4. `AchEngine > Addressables > Build Content`를 실행한 뒤 생성된 카탈로그와 번들을 원격 환경에 업로드합니다.
5. 샘플 오브젝트에 `RemoteContentDemo`를 붙이고 `remoteLabel`에 해당 라벨을 입력합니다.
6. 진행률 표시가 필요하면 `DownloadProgressUI`를 함께 연결합니다.

## 확인 포인트

- 다운로드 크기 확인 시 필요한 크기가 정상적으로 표시됩니다.
- 다운로드 중 진행률이 퍼센트와 바이트 단위로 갱신됩니다.
- 카탈로그 업데이트가 있으면 갱신 개수가 표시되고, 적용 후 최신 상태로 돌아옵니다. 업데이트 후 변경된 에셋을 다시 받아야 하면 해당 주소를 `Release`한 뒤 다시 로드합니다.
