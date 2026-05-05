# 01 Basic Usage

이 샘플은 `AddressableManager`의 가장 기본적인 사용 흐름을 보여줍니다.

## 포함 내용

- `InitializeAsync()`로 안전하게 초기화하기
- `RegisterAssetHandle()`로 문자열 키를 등록해두고 재사용하기
- `LoadAssetAsync<Sprite>()`로 단일 에셋 로드하기
- 같은 주소를 다시 로드해 참조 수 증가 확인하기
- `InstantiateAsync()`로 프리팹 생성하기
- `Release()`와 `ReleaseInstance()`로 수동 해제하기

## 준비

1. `Project Settings > Addressables Manager`를 엽니다.
2. 상단에 `AddressableAssetSettings` 생성 배너가 보이면 먼저 생성합니다.
3. 감시 폴더를 하나 추가합니다.
4. 감시 폴더 안에 스프라이트와 프리팹을 넣습니다.
5. `AchieveOne > Addressables Manager > Build Content`를 실행합니다.
6. 샘플 씬이나 테스트 오브젝트에 `BasicUsageDemo`를 붙입니다.
7. `spriteAddress`, `prefabAddress`에 실제 Addressable 주소를 입력합니다.
8. 등록 키 방식도 함께 보려면 `registerAssetHandlesBeforeRun`을 켜고 `spriteHandleKey`, `prefabHandleKey`를 그대로 사용합니다.
9. 등록 키는 샘플처럼 첫 로드 전에 미리 등록하는 흐름을 권장합니다.

## 확인 포인트

- 첫 번째 스프라이트 로드 후 참조 수가 `1`이 됩니다.
- 같은 주소를 다시 로드하면 참조 수가 증가합니다.
- 샘플 마지막 단계에서 모든 스프라이트 핸들을 해제하면 `IsLoaded`가 `False`가 됩니다.
