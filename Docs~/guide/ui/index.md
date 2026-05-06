# UI 시스템 — 개요

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

```mermaid
block-beta
    columns 1
    tooltip["🔔 Tooltip&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;SortingOrder: 40\n툴팁, 알림"]
    overlay["⬛ Overlay&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;SortingOrder: 30\n전체화면 오버레이, 로딩"]
    popup["💬 Popup&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;SortingOrder: 20\n팝업, 다이얼로그"]
    screen["🖥 Screen&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;SortingOrder: 10\n기본 화면, 메인 UI"]
    bg["🌄 Background&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&ensp;SortingOrder:  0\n배경 화면, 배경 애니메이션"]

    style tooltip  fill:#1a1a3a,stroke:#8b5cf6,color:#c4b5fd
    style overlay  fill:#1a2a3a,stroke:#f59e0b,color:#fcd34d
    style popup    fill:#1a3a2a,stroke:#10b981,color:#6ee7b7
    style screen   fill:#1e3a5f,stroke:#3b82f6,color:#93c5fd
    style bg       fill:#162032,stroke:#64748b,color:#94a3b8
```

## View 열기 / 닫기

```csharp
var ui = ServiceLocator.Resolve<IUIService>();

// ── 열기 ──────────────────────────────────────────────
ui.Show<MainMenuView>();                            // 타입
ui.Show("MainMenu");                                // 문자열 ID
ui.Show<ItemDetailView>(v => v.SetItem(item));      // 타입 + 초기화 콜백
ui.Show("ItemDetail", v => ((ItemDetailView)v)
    .SetItem(item));                                // ID + 콜백

// ── 닫기 ──────────────────────────────────────────────
ui.Close<MainMenuView>();                           // 타입
ui.Close("MainMenu");                               // ID
ui.CloseAll();                                      // 전체
ui.CloseLayer(UILayerId.Popup);                     // 레이어 전체
```

## 다음 단계

- [UIView & 수명 주기](/guide/ui/views)
- [UI Workspace](/guide/ui/workspace)
