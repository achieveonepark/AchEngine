# 02 Scene Management

이 샘플은 Addressable 씬 로드·언로드와 명시적 에셋 캐시 해제를 확인하기 위한 예제입니다. 씬은 자동으로 정리되지 않으므로 로드한 주소로 `UnloadSceneAsync`를 호출해야 합니다.

## 포함 내용

- `LoadSceneAsync()`로 Addressable 씬을 Additive 로드하기
- 로드한 씬에서 에셋을 로드하기
- 씬 언로드 전에 `Release()`로 관련 에셋 캐시를 해제하기
- `UnloadSceneAsync()` 결과 확인하기

## 준비

1. 실행용 씬과 별도로 Addressable로 사용할 씬을 하나 준비합니다.
2. 그 씬에서 함께 테스트할 프리팹 또는 에셋 주소를 하나 준비합니다.
3. 감시 폴더를 쓰는 경우 씬과 에셋이 감시 폴더에 포함되도록 설정합니다.
4. `AchEngine > Addressables > Build Content`를 실행합니다.
5. `SceneManagementDemo`를 현재 실행용 씬의 오브젝트에 붙입니다.
6. `sceneAddress`, `assetInSceneAddress`를 실제 주소로 입력합니다.

## 확인 포인트

- 씬 로드 후 지정한 에셋이 로드됩니다.
- 씬 언로드 전 `assetInSceneAddress`가 로드된 상태여야 합니다.
- 언로드 과정에서 `Release(assetInSceneAddress)` 후 `UnloadSceneAsync(sceneAddress)`가 순서대로 호출됩니다.
- `UnloadSceneAsync`가 `true`를 반환하면 이 API가 추적하던 씬이 정상적으로 언로드된 것입니다.
