# Draggable & 터치 오브젝트

AchEngine은 2D 월드 오브젝트의 드래그와 터치 처리를 위한 세 가지 컴포넌트를 제공합니다.

## Draggable

`MonoBehaviour`에 `Draggable` 컴포넌트를 붙이면 마우스/터치로 드래그할 수 있습니다.
`Physics2DRaycaster`가 없으면 자동으로 메인 카메라에 추가됩니다.

```csharp
using AchEngine.UI;

public class CardView : Draggable
{
    protected override void Start()
    {
        base.Start(); // Physics2DRaycaster 자동 추가

        OnTouchDown += () => Debug.Log("카드 집음");
        OnTouching  += pos => Debug.Log($"드래그 중: {pos}");
        OnTouchUp   += hits => Debug.Log($"드랍, 충돌체: {hits.Length}개");
    }
}
```

### 이벤트

| 이벤트 | 시그니처 | 설명 |
|---|---|---|
| `OnTouchDown` | `Action` | 포인터 누름 직후 |
| `OnTouching` | `Action<Vector3>` | 드래그 중 (월드 좌표) |
| `OnTouchUp` | `Action<Collider2D[]>` | 드랍 시, 겹친 Collider2D 목록 전달 |

### 프로퍼티

| 프로퍼티 | 설명 |
|---|---|
| `originalPos` | 드래그 시작 시점의 월드 좌표 (`protected`) |

## TouchableObject

탭/클릭만 처리하는 오브젝트에 사용합니다. `OnTouched()`를 오버라이드하세요.

```csharp
using AchEngine.UI;

public class EnemyObject : TouchableObject
{
    protected override void OnTouched()
    {
        Debug.Log("적 클릭됨");
    }
}
```

> `TouchableObject`는 `ObjectTouchManager`에 의해 감지됩니다. 씬에 `ObjectTouchManager`가 있어야 합니다.

## ObjectTouchManager

씬에 하나만 존재하는 싱글턴 매니저입니다. 매 프레임 마우스 왼쪽 버튼 클릭을 감지하고,
2D 레이캐스트로 `TouchableObject`를 찾아 `OnTouched()`를 호출합니다.

```csharp
// 씬에 빈 GameObject를 만들고 ObjectTouchManager 컴포넌트를 추가하세요.
// 별도의 설정 없이 자동으로 동작합니다.
```

> `ObjectTouchManager`는 `MonoSingleton<ObjectTouchManager>`를 상속하므로,
씬에 중복 인스턴스가 있으면 자동으로 제거됩니다.
