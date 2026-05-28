# UIBindingManager <Badge type="tip" text="ACHENGINE_R3" />

`UIBindingManager` 是基于 R3 的 `Subject<T>` 实现的基于类型的 pub/sub 消息总线。
可以在 UI 组件之间无依赖地收发事件。

仅在安装了 R3 包 (`com.cysharp.r3`) 时启用。

## 发布 (Publish)

```csharp
using AchEngine.UI;

// 定义消息类型
public struct GoldChangedMessage
{
    public int Amount;
}

// 发布
UIBindingManager.Publish(new GoldChangedMessage { Amount = 500 });
```

## 订阅 (Subscribe)

```csharp
using AchEngine.UI;

// 订阅 —— 请保存 IDisposable 并在 OnDestroy 中释放
private IDisposable _subscription;

private void OnEnable()
{
    _subscription = UIBindingManager.Subscribe<GoldChangedMessage>(msg =>
    {
        goldLabel.text = msg.Amount.ToString();
    });
}

private void OnDisable()
{
    _subscription?.Dispose();
}
```

## 工具方法

```csharp
// 检查特定类型的 Subject 是否已注册
bool exists = UIBindingManager.Contains<GoldChangedMessage>();

// 清空所有 Subject (例如场景切换时)
UIBindingManager.ClearAll();
```

## API 概览

| 方法 | 说明 |
|---|---|
| `Publish<T>(T)` | 发布消息 |
| `Subscribe<T>(Action<T>)` | 订阅消息,返回 `IDisposable` |
| `Contains<T>()` | 是否存在该类型的 Subject |
| `ClearAll()` | 移除所有 Subject |

> 若未安装 R3,`UIBindingManager` 将从编译中排除。可在 **Window › AchEngine › AchEngine Info** 查看是否已安装。
