# 변경 내역

## 1.3.1

**개선**
- 에디터에서 `IAPManager`가 Unity IAP Fake Store를 명시적으로 사용하도록 했습니다.
- 에디터에서는 `ReceiptValidator` 또는 `PurchaseProcessor` 없이도 가짜 주문을 자동 확정하여 구매 UI 흐름을 테스트할 수 있도록 했습니다.
- Full Sample의 구매 팝업이 에디터 Fake Store 테스트 시 검증기 없이도 활성화됩니다. 실제 빌드에서는 기존처럼 검증기 설정이 필요합니다.

## 1.3.0

**신기능**
- Unity 6000.3 이상에서 Build Settings 씬을 선택하고 현재 편집 씬으로 바로 이동할 수 있는 `AchEngine/Scene Navigator` 메인 툴바를 추가했습니다.
- `Screen`, `Popup`, `Tooltip` 레이어에 `UISafeAreaFitter`를 자동 적용했습니다. `Background`와 `Overlay`는 전체 화면으로 유지됩니다.

**개선**
- Addressables 에셋·위치 캐시가 요청 타입을 확인해 호환되지 않는 핸들을 재사용하지 않도록 했습니다.
- 초기화, 다운로드, 카탈로그 확인·업데이트에 사용하는 임시 Addressables 핸들을 자동 해제합니다.
- Domain Reload가 꺼진 Play Mode 재진입 시 디버그 콘솔 로그 콜백이 중복 등록되지 않도록 했습니다.
- 런타임 탐색을 `FindFirstObjectByType`으로 갱신하고, 자식 `UIRoot`에 `DontDestroyOnLoad`를 중복 호출하지 않도록 했습니다.
- `IScene`을 선택적 씬 라이프사이클 훅으로 처리하여 구현하지 않은 씬에는 경고를 출력하지 않습니다.

**문서**
- Addressables, UI Safe Area, Scene Navigator, AI Assistant 스킬 문서와 1.2.0/1.3.0 변경 내역을 갱신했습니다.

## 1.2.0

- Addressables를 정적 `AddressableManager` 중심으로 재구성하고 `AchTask` 기반 에셋·씬·인스턴스화·해제·원격 다운로드·카탈로그 API를 제공했습니다.
- 위치 메타데이터·에셋 핸들 캐시, 씬 핸들 추적, Addressables 에디터 설정·감시 폴더 도구를 추가했습니다.
- Addressables 가이드, 통합 가이드, 샘플을 새 캐시 해제 흐름에 맞춰 갱신했습니다.

## 1.1.2

- iOS 네이티브 디버그 콘솔을 제거했습니다. Android 오버레이와 에디터 IMGUI 오버레이는 유지됩니다.

## 1.1.1

- `IIAPReceiptValidator` 확장 지점, Apple StoreKit 2 JWS·앱 계정 토큰, 수동 복원·미확정 주문 재시도 API를 추가했습니다.
- 구매 결과를 거래 ID에 연결해 동일 상품의 미확정 주문이 잘못된 요청을 완료하지 않도록 했습니다.
- IAP 단위 테스트와 Full Sample 구매 문서를 추가·갱신했습니다.
- iOS Xcode 빌드 오류를 수정했습니다.

## 1.1.0

- Unity IAP 5.4.1 기반 `IAPManager`와 서버 영수증 검증용 `HttpIAPReceiptValidator`를 추가했습니다.
- `AchManagerInstaller`에 `IAPManager`를 등록하고 `com.unity.purchasing` 의존성을 선언했습니다.

## 1.0.8

- AchEngine 내장 시스템을 Unity AI Assistant가 인식하도록 `AIAssistantSkills/`를 패키지에 추가했습니다.

## 1.0.7

- `com.unity.addressables`가 설치되지 않은 환경에서 Addressables 에디터가 컴파일되지 않던 문제를 수정했습니다.
- 누락된 `AGENTS.md.meta`를 추가했습니다.

## 1.0.6

**브레이킹 변경**
- `AchEngine.Extensions` 어셈블리와 `Runtime/Extensions` 확장 메서드를 제거했습니다.

## 1.0.5

- UniTask와 `System.Threading.Tasks.Task`를 통합하는 `AchTask` / `AchTask<T>`를 추가했습니다.
- `AchTask` 한국어·영어 문서를 추가했습니다.

## 1.0.4

**브레이킹 변경**
- `SoundManager`를 제거하고 `AudioManager`로 교체했습니다.

**신기능**
- `AudioManager`, `AchTimer`, `UIAchTimer`, `AchButtonCooldown`, `AchButtonHold`, `AchDebugConsole`를 추가했습니다.
- `RedDot.ClearAll()`과 `RedDotBadge` 클릭 클리어 기능을 추가했습니다.
- 전체 신규 기능과 주요 API 수정 사항을 한국어·영어 문서에 반영했습니다.

## 1.0.3

- `SaveManager`, `ISaveService`, `LocalSaveService`, `AchProjectile`을 추가했습니다.
- `AchFollower`를 독립 컴포넌트로 리팩터링하고 FontAsset 다국어 빌드를 추가했습니다.

## 1.0.2

- ECS, Managers, Singleton, Log, WebRequest, PlayerData, QuickSave, A* Pathfinding, AchMover, RedDot, UI 헬퍼와 Full Sample을 추가했습니다.
- Addressables, DI, Localization, Table, UI 문서와 Domain Reload 대응을 개선했습니다.

## 1.0.1

- Table JSON 데이터를 Google Sheets 임포트용 CSV로 내보내는 도구를 추가했습니다.
