# Draggable 与触摸对象

AchEngine 提供了三个组件用于处理 2D 世界对象的拖拽与触摸。

## Draggable

在 `MonoBehaviour` 上附加 `Draggable` 组件后,即可通过鼠标/触摸进行拖拽。
若不存在 `Physics2DRaycaster`,将自动添加到主摄像机上。

```csharp
using AchEngine.UI;

public class CardView : Draggable
{
    protected override void Start()
    {
        base.Start(); // 自动添加 Physics2DRaycaster

        OnTouchDown += () => Debug.Log("拾起卡牌");
        OnTouching  += pos => Debug.Log($"拖拽中: {pos}");
        OnTouchUp   += hits => Debug.Log($"放下,碰撞体: {hits.Length} 个");
    }
}
```

### 事件

| 事件 | 签名 | 说明 |
|---|---|---|
| `OnTouchDown` | `Action` | 指针按下后立即 |
| `OnTouching` | `Action<Vector3>` | 拖拽中 (世界坐标) |
| `OnTouchUp` | `Action<Collider2D[]>` | 放下时,传递重叠的 Collider2D 列表 |

### 属性

| 属性 | 说明 |
|---|---|
| `originalPos` | 开始拖拽时的世界坐标 (`protected`) |

## TouchableObject

用于只处理点按/点击的对象。请重写 `OnTouched()`。

```csharp
using AchEngine.UI;

public class EnemyObject : TouchableObject
{
    protected override void OnTouched()
    {
        Debug.Log("敌人被点击");
    }
}
```

> `TouchableObject` 由 `ObjectTouchManager` 检测。场景中必须存在 `ObjectTouchManager`。

## ObjectTouchManager

场景中仅存在一份的单例管理器。每帧检测鼠标左键点击,
通过 2D 射线寻找 `TouchableObject` 并调用其 `OnTouched()`。

```csharp
// 在场景中创建一个空的 GameObject 并添加 ObjectTouchManager 组件。
// 无需额外设置即可自动工作。
```

> `ObjectTouchManager` 继承自 `MonoSingleton<ObjectTouchManager>`,
若场景中存在重复实例,会自动移除。
