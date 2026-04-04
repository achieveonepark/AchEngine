# 트랜지션

`UITransitionSettings`으로 View의 등장/퇴장 애니메이션을 정의합니다.

## UITransitionMode

```csharp
public enum UITransitionMode
{
    None,        // 즉시 표시/숨김
    Fade,        // 알파값 페이드
    SlideLeft,   // 왼쪽에서 슬라이드
    SlideRight,  // 오른쪽에서 슬라이드
    SlideUp,     // 위에서 슬라이드
    SlideDown,   // 아래에서 슬라이드
    Scale,       // 크기 스케일
}
```

## UIView에 트랜지션 설정

Inspector에서 `UIView` 컴포넌트의 **Open Transition** / **Close Transition** 필드를 설정합니다.

```csharp
public class PopupView : UIView
{
    // Inspector에서 설정하거나 코드로 재정의 가능
    [SerializeField] private UITransitionSettings _openTransition;
    [SerializeField] private UITransitionSettings _closeTransition;
}
```

## UITransitionSettings 필드

| 필드 | 설명 |
|---|---|
| `Mode` | 트랜지션 종류 (Fade, Slide 등) |
| `Duration` | 재생 시간 (초) |
| `Curve` | 애니메이션 커브 |

## 커스텀 트랜지션

`OnBeforeOpen()` / `OnBeforeClose()` 를 오버라이드하여 직접 구현할 수 있습니다.

```csharp
protected override void OnBeforeOpen()
{
    // DOTween, LeanTween, Unity Animation 등 자유롭게 사용
    transform.localScale = Vector3.zero;
    transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
}

protected override void OnBeforeClose()
{
    transform.DOScale(Vector3.zero, 0.2f)
        .SetEase(Ease.InBack)
        .OnComplete(OnCloseComplete); // 완료 후 OnClosed 호출
}
```
