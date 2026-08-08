# 01 Basic Usage

이 샘플은 정적 `AddressableManager`의 기본 흐름을 보여줍니다. 모든 비동기 호출은 `AchTask`를 반환하며 일반적인 `await` 문법으로 사용합니다.

## 포함 내용

- `InitializeAsync()`로 안전하게 초기화하기
- `LoadAsync<Sprite>()`로 단일 에셋 로드하기
- 같은 주소를 다시 로드해 캐시 재사용 확인하기
- `InstantiateAsync()`로 프리팹 생성하기
- `Release()`와 `ReleaseInstance()`로 수동 해제하기

## 준비

1. `Project Settings > AchEngine > Addressables`를 엽니다.
2. `AddressableAssetSettings` 생성 안내가 보이면 먼저 생성합니다.
3. 감시 폴더를 추가하거나 Unity Addressables 창에서 에셋을 직접 Addressable로 지정합니다.
4. 감시 폴더 안에 스프라이트와 프리팹을 넣습니다.
5. `AchEngine > Addressables > Build Content`를 실행합니다.
6. 샘플 씬이나 테스트 오브젝트에 `BasicUsageDemo`를 붙입니다.
7. `spriteAddress`, `prefabAddress`에 실제 Addressable 주소를 입력합니다.

## 확인 포인트

- 첫 번째 스프라이트 로드 후 `IsLoaded`가 `True`가 됩니다.
- 같은 주소를 다시 로드해도 같은 캐시 핸들을 재사용합니다.
- 프리팹 인스턴스는 `ReleaseInstance`, 원본 프리팹과 스프라이트는 `Release`로 각각 해제됩니다.
- 샘플 마지막 단계에서 스프라이트를 해제하면 `IsLoaded`가 `False`가 됩니다. 캐시는 참조 횟수를 세지 않으므로 실제 소유자가 해제 시점을 결정해야 합니다.
