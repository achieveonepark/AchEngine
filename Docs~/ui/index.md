# UI System

AchEngine UI System은 **레이어 기반** UI 관리 시스템입니다.
`UIViewCatalog`에 등록된 View를 ID 또는 타입으로 Show/Close할 수 있으며,
Object Pool, 트랜지션 애니메이션, Single Instance 모드를 내장합니다.

## 핵심 구성 요소

| 클래스 | 역할 |
|---|---|
| `UIRoot` | 모든 레이어의 루트 Canvas 관리자 |
| `UIBootstrapper` | 씬 시작 시 UI 시스템 초기화 |
| `IUIService` / `UI` | View 표시·숨기기 파사드 |
| `UIView` | 모든 View의 기본 클래스 |
| `UIViewCatalog` | View 프리팹 등록 ScriptableObject |
| `UIViewPool` | View 인스턴스 재사용 풀 |

## 레이어 구조

```
UIRoot
├── Background   (SortingOrder  0)  — 배경 화면, 배경 애니메이션
├── Screen       (SortingOrder 10)  — 기본 화면, 메인 UI
├── Popup        (SortingOrder 20)  — 팝업, 다이얼로그
├── Overlay      (SortingOrder 30)  — 전체화면 오버레이, 로딩
└── Tooltip      (SortingOrder 40)  — 툴팁, 알림
```

## 빠른 예시

```csharp
// IUIService (DI 주입 또는 ServiceLocator)
var ui = ServiceLocator.Resolve<IUIService>();

// 타입으로 열기
ui.Show<MainMenuView>();

// ID 문자열로 열기
ui.Show("GameHUD");

// 데이터 전달
ui.Show<ItemDetailView>(view => view.SetItem(selectedItem));

// 닫기
ui.Close<MainMenuView>();
ui.CloseAll();
```

## 설정 위치

**Project Settings › AchEngine › UI Workspace** 에서 현재 씬 상태 확인,
UIRoot 생성, UI Workspace 창을 바로 열 수 있습니다.
