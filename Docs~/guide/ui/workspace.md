# UI Workspace 사용법

UI Workspace는 씬의 UIView를 시각적으로 관리하는 에디터 창입니다.
**Tools › AchEngine › UI Workspace** 또는 **Project Settings › AchEngine › UI Workspace** 에서 열 수 있습니다.

## 초기 설정

### 1. UIRoot 생성

**Project Settings › AchEngine › UI Workspace → UI Root 생성** 버튼 클릭.

씬에 아래 계층이 자동으로 생성됩니다.

```
[UIRoot]
 ├── Layer_Background  (Canvas, SortingOrder: 0)
 ├── Layer_Screen      (Canvas, SortingOrder: 10)
 ├── Layer_Popup       (Canvas, SortingOrder: 20)
 ├── Layer_Overlay     (Canvas, SortingOrder: 30)
 └── Layer_Tooltip     (Canvas, SortingOrder: 40)
```

### 2. UIViewCatalog 생성

**Create › AchEngine › UI View Catalog** 로 Catalog 에셋을 생성합니다.

`UIRoot` 컴포넌트의 **Catalog** 필드에 드래그합니다.

### 3. View 프리팹 등록

Catalog에 View 프리팹을 등록합니다.

| 필드 | 설명 |
|---|---|
| **ID** | `Show("이 ID")` 로 열 때 쓰는 문자열 |
| **Prefab** | UIView 컴포넌트가 붙은 프리팹 |
| **Layer** | 렌더 레이어 |
| **Pool Size** | 사전 생성 인스턴스 수 (0 = 필요 시 생성) |

## View 열기 / 닫기

```csharp
var ui = ServiceLocator.Resolve<IUIService>();

// ── 열기 ──────────────────────────────────────────────
ui.Show<MainMenuView>();                            // 타입
ui.Show("MainMenu");                                // 문자열 ID
ui.Show<ItemDetailView>(v => v.SetItem(item));      // 타입 + 초기화 콜백
ui.Show("ItemDetail", v => ((ItemDetailView)v)      // ID + 콜백
    .SetItem(item));

// ── 닫기 ──────────────────────────────────────────────
ui.Close<MainMenuView>();                           // 타입
ui.Close("MainMenu");                               // ID
ui.CloseAll();                                      // 전체
ui.CloseLayer(UILayerId.Popup);                     // 레이어 전체
```

## View 프리팹 만들기

### 기본 View

```
[GameObject]
 ├── Canvas Group  (페이드 트랜지션용)
 ├── UIView 컴포넌트  ← 반드시 필요
 └── UI 요소들...
```

```csharp
public class MainMenuView : UIView
{
    [SerializeField] private Button _playButton;
    [SerializeField] private Button _settingsButton;

    protected override void OnInitialize()
    {
        _playButton.onClick.AddListener(OnPlay);
        _settingsButton.onClick.AddListener(OnSettings);
    }

    private void OnPlay()
    {
        ServiceLocator.Resolve<ISceneService>().LoadInGame(1);
    }

    private void OnSettings()
    {
        ServiceLocator.Resolve<IUIService>().Show<SettingsPopup>();
    }
}
```

### Pool 활성화

같은 View를 자주 열고 닫는 경우 Pool을 사용해 GC를 줄입니다.

Catalog의 **Pool Size**를 1 이상으로 설정하면 닫힐 때 Destroy 대신 Pool로 반환됩니다.

```csharp
public class DamageNumberView : UIView
{
    public override UILayerId Layer => UILayerId.Overlay;

    // Pool 반환 시 상태 초기화
    protected override void OnClosed()
    {
        GetComponent<Text>().text = "";
    }
}
```

### Single Instance

같은 View가 중복으로 열리는 것을 막습니다.

```csharp
public class LoadingView : UIView
{
    public override bool SingleInstance => true;
    public override UILayerId Layer     => UILayerId.Overlay;
}
```

## UIBootstrapper 설정

씬 시작 시 자동으로 열 View를 지정합니다.

```
[UIBootstrapper] 컴포넌트
 └── Auto Open Views: [MainMenuView, BGMView]
```

## 유용한 컴포넌트

### UICloseButton

가장 가까운 부모 `UIView`를 닫는 버튼입니다.
코드 없이 Inspector에서 연결만 하면 됩니다.

```
[SettingsPopup (UIView)]
 └── [CloseButton]  ← UICloseButton 컴포넌트 추가
```

### UIOpenButton

버튼 클릭 시 지정한 View를 여는 컴포넌트입니다.

```
[UIOpenButton]
 └── Target View ID: "SettingsPopup"
```

### UISafeAreaFitter

노치/펀치홀 영역을 피하는 SafeArea 적용 컴포넌트입니다.
각 레이어 Canvas 자식에 추가합니다.

## UI Workspace 창 활용

에디터에서 **Tools › AchEngine › UI Workspace** 를 열면:

- 씬의 모든 등록된 View 목록 확인
- 에디터 플레이 모드에서 View 강제 열기/닫기
- 레이어별 View 상태 실시간 확인
- 미등록 UIView 컴포넌트 경고 감지
