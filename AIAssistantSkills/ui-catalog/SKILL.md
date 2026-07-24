---
name: ui-catalog
description: Use when the user asks to create a new UI screen, popup, panel, or menu, or to open/close/switch UI in a project that has the AchEngine package installed. Register the view in AchEngine's UIViewCatalog and open it through UIView/IUIService/the UI facade instead of hand-instantiating prefabs or managing Canvas objects directly.
---

# 카탈로그 기반 UI 시스템

`AchEngine.UI` 네임스페이스 (`Runtime/UI/*`, `Editor/UI/*`). 새 UI 화면/팝업은 프리팹을 직접 Instantiate하지 말고 카탈로그에 등록한 뒤 `UI.Show`로 연다.

## 핵심 개념

- **`UIViewCatalog`** (`ScriptableObject`, `Assets/Create/AchEngine/View Catalog` 메뉴) — `UIViewCatalogEntry`(`Id`, `Prefab`(`UIView`), `Layer`(`UILayerId`), `Pooled`, `PrewarmCount`, `SingleInstance`) 목록. 이것이 "카탈로그"이며 뷰를 여기 등록해야 `Id`로 찾을 수 있다.
- **`UILayerId`** — `Background(0) < Screen(10) < Popup(20) < Overlay(30) < Tooltip(40)`. 숫자가 클수록 위에 렌더링.
- **`UIView`** (추상 `MonoBehaviour`, `[DisallowMultipleComponent]`) — 모든 화면/팝업 프리팹의 베이스 클래스. `OnInitialize()`(최초 1회, 버튼 바인딩 등), `OnBeforeOpen(object payload)`, `OnOpened(object payload)`, `OnBeforeClose()`, `OnClosed()` 오버라이드. `ViewId`, `Layer`, `Service`, `IsVisible`, `IsClosing`, `RectTransform`, `LastPayload` 프로퍼티. 스스로 닫으려면 `CloseSelf()`. `UITransitionSettings`/`UITransitionMode`(`Fade`, `FadeScale`)로 전환 효과 설정.
- **`IUIService` / `UIService`** — `Show(id, payload=null)` → `UIView`, `Show<T>(id, payload)` → `T`, `Close(id, closeAll=false)`, `Close(UIView)`, `CloseTopmost()`, `CloseAll()`, `TryGetOpen(id, out view)`/`TryGetOpen<T>(out view)`, `IsOpen(id)`, `Prewarm()`, `Catalog`, `Root`(`UIRoot`), `IsInitialized`.
- **`UI` static facade** (`AchEngine.UI.UI`) — `UI.Show(id, payload)`, `UI.Show<T>(...)`, `UI.Close(...)`, `UI.CloseTopmost()`, `UI.CloseAll()`, `UI.TryGetOpen(...)`, `UI.IsOpen(id)`, `UI.IsReady`. 서비스가 등록되기 전에 호출하면 `InvalidOperationException`.

## 부트스트랩

- **DI 없이**: `UIBootstrapper`(`MonoBehaviour`)를 씬에 두고 `catalog`(+ 선택적 `root`)를 지정하면 `Awake()`에서 `UIService`를 생성/초기화하고 `UI.SetService(...)`로 등록한다.
- **VContainer 사용 시**: `AchEngineScope`(`vcontainer-di` 스킬 참고)가 `UIService`를 자동으로 컨테이너에 등록하고 `UI.SetService(...)`도 호출하므로 `UI.Show(...)`가 그대로 동작한다.

## 컴포넌트 / 에디터 도구

`UICloseButton`/`UIOpenButton`(버튼에 카탈로그 id 바인딩), `Draggable`, `TouchableObject`/`ObjectTouchManager`, `UISafeAreaFitter`, `UIAchTimer`(`ach-timer` 스킬과 연동), `UIBindingManager`. 에디터: `AchEngine/UI Workspace` 메뉴, `AchEngineUIWorkspaceWindow`(카탈로그 항목 추가/제거/검증을 위한 UI Toolkit 창) — 카탈로그를 손으로 수정하기보다 이 창을 사용한다.

## 예시

```csharp
public class MainMenuView : UIView
{
    protected override void OnInitialize()
        => btnStart.onClick.AddListener(() => UI.Show("GameHUD"));
}

public class MonsterDetailPopup : UIView
{
    protected override void OnOpened(object payload)
    {
        if (payload is int monsterId /* ... */) { /* ... */ }
    }
}
// 호출부: UI.Show("MonsterDetail", monsterId);
```

## 주의

프리팹은 `UIViewCatalog`에 고유한 `Id`로 등록되어 있어야 `UI.Show("Id")`가 찾을 수 있다 — 속성 기반 자동 탐색은 없다. `UIBootstrapper` 또는 `AchEngineScope`가 아직 실행되지 않았으면 `UI.Show`가 예외를 던지므로 `UI.IsReady`로 가드한다.
