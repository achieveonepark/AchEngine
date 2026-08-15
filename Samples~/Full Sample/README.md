# Full Sample — AchEngine 전체 기능 데모

AchEngine의 모든 기능을 3개 씬으로 구성한 프로토타입 템플릿입니다.

## 씬 구성

| 씬 | 파일 | 주요 기능 |
|---|---|---|
| **TitleScene** | `Scenes/TitleScene.cs` | HttpLink 서버 핑, TimeManager, SoundManager BGM, TitleView |
| **LobbyScene** | `Scenes/LobbyScene.cs` | PlayerManager, ConfigManager, UIBindingManager(골드), LobbyView |
| **IngameScene** | `Scenes/IngameScene.cs` | PoolManager(카드), TimeManager 틱, Draggable, TouchableObject |

## 사용된 AchEngine 기능

| 기능 | 클래스 | 사용 위치 |
|---|---|---|
| 매니저 DI | `AchManagerInstaller` | Bootstrap |
| 설정 저장 | `ConfigManager` | SettingsPopup |
| BGM/SFX | `SoundManager` | TitleScene, LobbyScene, IngameScene |
| 씬 전환 | `AchSceneManager` | TitleView, LobbyView, IngameHUDView |
| 입력 잠금 | `InputManager` | TitleScene, IngameScene |
| 서버 시간/틱 | `TimeManager` | TitleScene, IngameScene |
| 오브젝트 풀 | `PoolManager` | IngameScene, DraggableCard |
| 플레이어 데이터 | `PlayerManager` + `InventoryContainer` | FullSampleBootstrap, InventoryPopup |
| IAP 5.4.1 | `IAPManager` | PurchasePopup |
| HTTP 요청 | `HttpLink` | TitleScene (서버 핑) |
| Reactive 메시지 | `UIBindingManager` _(R3 필요)_ | LobbyView, IngameHUDView, GameplayManager |
| 드래그 | `Draggable` | DraggableCard |
| 터치 오브젝트 | `TouchableObject` | CardSlot |
| 터치 매니저 | `ObjectTouchManager` | IngameScene 자동 싱글턴 |
| 설정 SO | `AchScriptableObject` | GameConfig |
| UI 뷰 | `UIView` | TitleView, LobbyView, SettingsPopup, IngameHUDView, InventoryPopup, PurchasePopup |
| 싱글턴 | `MonoSingleton` | GameplayManager |

## 씬별 설정 방법

### 공통 (모든 씬)
1. 씬에 `AchEngineScope` GameObject를 만들고 `AchManagerInstaller` 컴포넌트 추가
2. 씬에 `UIBootstrapper`와 `UIRoot` 추가 후 `UIViewCatalog` 할당
3. UIViewCatalog에 아래 ID로 프리팹 등록:

| View ID | 클래스 | Layer |
|---|---|---|
| `Title` | `TitleView` | Screen |
| `Lobby` | `LobbyView` | Screen |
| `IngameHUD` | `IngameHUDView` | Screen |
| `Settings` | `SettingsPopup` | Popup |
| `Inventory` | `InventoryPopup` | Popup |
| `Purchase` | `PurchasePopup` | Popup |

### Bootstrap 씬 (시작 씬)
- 빈 GameObject에 `FullSampleBootstrap` 컴포넌트 추가
- `titleSceneName` 필드에 첫 씬 이름 입력 (기본값: `"TitleScene"`)

### IngameScene
- `GameplayManager` 컴포넌트를 빈 GameObject에 추가
- `slots` 배열에 `CardSlot` 컴포넌트들 연결
- `cardSpawnRoot` Transform 설정
- 카드 프리팹 루트에 `DraggableCard` 컴포넌트 추가
- `ObjectTouchManager` 컴포넌트를 빈 GameObject에 추가

## 선택적 기능 (패키지 설치 필요)

### R3 (`com.cysharp.r3`)
R3가 설치된 경우 `UIBindingManager`가 활성화되어 HP/점수/골드가 Reactive하게 업데이트됩니다.
없으면 `#if ACHENGINE_R3` 블록이 비활성화되며 기본 폴백으로 동작합니다.

### QuickSave (`USE_QUICK_SAVE` 심볼)
`FullSampleBootstrap.cs`에서 주석 처리된 `pm.Configure()` / `pm.Load()` 를 활성화하세요:
```csharp
pm.Configure(encryptionKey: "YourKey12345678!", version: 1);
pm.Load();
```

### IAP (`com.unity.purchasing` 5.4.1)
`PurchasePopup`은 세 가지 샘플 소비성 상품을 등록합니다. `IAPReceiptValidatorBehaviour`를 상속한
게임별 서버 검증 컴포넌트를 만들고 PurchasePopup의 `Receipt Validator` 필드에 연결하세요.
검증기 안에서 영수증 검증과 거래 ID 기반 보상 저장이 성공한 경우에만 주문을 확정합니다.
소비성 상품의 보상은 반드시 서버에서 멱등적으로 저장해야 합니다.

Unity 에디터에서는 `IAPManager`가 Fake Store를 사용하며, `Receipt Validator`를 연결하지 않아도
구매 팝업에서 가짜 구매 흐름을 테스트할 수 있습니다. 이때 주문은 에디터에서만 자동 확정됩니다.
Android/iOS 등 실제 빌드에서는 반드시 게임별 영수증 검증기를 연결하세요.

```csharp
using System;
using System.Threading.Tasks;

public sealed class GameReceiptValidator : IAPReceiptValidatorBehaviour
{
    public override Task<IAPReceiptValidationResult> ValidateAndFulfillAsync(IAPPurchase purchase)
    {
        // 게임 서버 요청, 인증, 보상 지급 저장을 구현합니다.
        throw new NotImplementedException();
    }
}
```

검증 서버는 아래 정보를 받고, 같은 거래 ID가 다시 전달되면 기존 보상 지급 결과를 반환해야 합니다.
Apple StoreKit 2에서는 `appleJwsRepresentation`을 Apple 서버 API로 검증하세요. 응답의 `isValid`와
`isFulfilled`가 모두 `true`일 때만 Unity IAP 주문이 확정됩니다.

```json
// 요청
{
  "transactionId": "스토어 거래 ID",
  "receipt": "Unity IAP 영수증",
  "appleJwsRepresentation": "Apple StoreKit 2 JWS",
  "appleAppAccountToken": "Apple 앱 계정 토큰",
  "products": [{ "productId": "com.sample.gold_100", "productType": "Consumable", "quantity": 1 }]
}

// 응답
{
  "transactionId": "스토어 거래 ID",
  "isValid": true,
  "isFulfilled": true,
  "message": "보상 지급 완료"
}
```

## 파일 구조

```
Full Sample/
├── Bootstrap/
│   └── FullSampleBootstrap.cs     # 진입점, 매니저 초기화
├── Data/
│   ├── GameConfig.cs              # AchScriptableObject 설정
│   ├── InventoryItem.cs           # PlayerDataBase 아이템
│   └── InventoryContainer.cs     # PlayerDataContainerBase
├── Messages/                      # UIBindingManager 메시지 타입
│   ├── GoldChangedMessage.cs
│   ├── HpChangedMessage.cs
│   └── ScoreChangedMessage.cs
├── Scenes/
│   ├── TitleScene.cs              # IScene — 타이틀
│   ├── LobbyScene.cs              # IScene — 로비
│   └── IngameScene.cs             # IScene — 인게임
├── UI/
│   ├── TitleView.cs
│   ├── LobbyView.cs
│   ├── SettingsPopup.cs
│   ├── IngameHUDView.cs
│   ├── InventoryPopup.cs
│   └── PurchasePopup.cs
└── Game/
    ├── DraggableCard.cs           # Draggable 카드
    ├── CardSlot.cs                # TouchableObject 슬롯
    └── GameplayManager.cs        # MonoSingleton 게임 로직
```
