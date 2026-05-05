# 02 Scene Management

이 샘플은 Addressable 씬 로드/언로드와 씬 컨텍스트 기반 자동 해제를 확인하기 위한 예제입니다.

## 포함 내용

- `LoadSceneAsync()`로 Addressable 씬을 Additive 로드하기
- 로드한 씬을 활성 씬으로 전환한 뒤 에셋을 로드하기
- `autoReleaseOnSceneUnload`가 `true`일 때 자동 해제 결과 확인하기
- `UnloadSceneAsync()` 이후 핸들이 제거되는지 검증하기

## 준비

1. Addressable로 사용할 씬을 하나 준비합니다.
2. 그 씬 안에서 함께 테스트할 프리팹 또는 에셋 주소를 하나 준비합니다.
3. 감시 폴더를 쓰는 경우 씬과 에셋이 감시 폴더에 포함되도록 설정합니다.
4. `Build Content`를 실행합니다.
5. `SceneManagementDemo`를 현재 실행용 씬의 오브젝트에 붙입니다.
6. `sceneAddress`, `assetInSceneAddress`를 실제 주소로 입력합니다.

## 확인 포인트

- 씬 로드 후 같은 씬 컨텍스트에서 에셋이 로드됩니다.
- 씬 언로드 전 `assetInSceneAddress`가 로드된 상태여야 합니다.
- 씬 언로드 후 `AddressableManager.IsLoaded(assetInSceneAddress)`가 `False`가 되어야 합니다.
