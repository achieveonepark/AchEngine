# 레이어 시스템

AchEngine UI System은 5개의 미리 정의된 레이어를 제공합니다.
각 레이어는 독립된 Canvas를 가지며 SortingOrder로 렌더 순서를 제어합니다.

## 레이어 정의

| 레이어 | SortingOrder | 용도 |
|---|:---:|---|
| `Background` | 0 | 배경 화면, 배경 애니메이션 |
| `Screen` | 10 | 메인 화면, 기본 UI |
| `Popup` | 20 | 팝업, 다이얼로그, 확인창 |
| `Overlay` | 30 | 전체화면 오버레이, 로딩 화면 |
| `Tooltip` | 40 | 툴팁, 플로팅 알림 |

## 레이어 지정

`UIView`의 **Layer** 필드를 Inspector에서 설정하거나 코드로 지정합니다.

```csharp
public class LoadingView : UIView
{
    public override UILayerId Layer => UILayerId.Overlay;
}

public class MainMenuView : UIView
{
    public override UILayerId Layer => UILayerId.Screen;
}
```

## 레이어별 열기/닫기

```csharp
var ui = ServiceLocator.Resolve<IUIService>();

// 특정 레이어의 모든 View 닫기
ui.CloseLayer(UILayerId.Popup);

// Overlay 위에 있는 모든 View 닫기
ui.CloseAbove(UILayerId.Screen);
```

## UIRoot 계층 구조

```
[Canvas - UIRoot]
├── [Canvas - Background]   SortingOrder: 0
├── [Canvas - Screen]       SortingOrder: 10
├── [Canvas - Popup]        SortingOrder: 20
├── [Canvas - Overlay]      SortingOrder: 30
└── [Canvas - Tooltip]      SortingOrder: 40
```

:::tip SafeArea 지원
`UISafeAreaFitter` 컴포넌트를 레이어 Canvas의 자식에 추가하면
iOS/Android 노치 영역을 자동으로 피합니다.
:::
