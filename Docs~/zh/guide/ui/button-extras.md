# 按钮扩展组件

`AchButtonCooldown` 与 `AchButtonHold` 是附加在既有 `Button` 组件上的扩展组件。
两个组件都要求同一个 GameObject 上存在 `Button` 才能工作。

## AchButtonCooldown

点击后立即禁用按钮,经过指定时间后再重新启用。
内置在标签上显示剩余时间的倒计时功能。

### Inspector 字段

| 字段 | 说明 |
|---|---|
| `Cooldown` | 点击后到下一次点击的最小等待时间 (秒)。默认值 `1` |
| `Show Countdown` | 是否在标签上显示剩余冷却时间 |
| `Countdown Label` | 用于显示倒计时的 `Text` 组件 (可选) |
| `On Cooldown Start` | 冷却开始时触发的 `UnityEvent` |
| `On Cooldown End` | 冷却结束时触发的 `UnityEvent` |

### API

| 成员 | 说明 |
|---|---|
| `IsCoolingDown` | 当前是否处于冷却中 |
| `StartCooldown()` | 手动启动冷却。已在进行中则重启计时器 |
| `ResetCooldown()` | 立即重置冷却并重新启用按钮 |

### 使用示例

```csharp
// 在脚本中直接控制冷却
var cooldown = GetComponent<AchButtonCooldown>();

// 由外部条件触发冷却 (例如等待服务器响应前锁定)
cooldown.StartCooldown();

// 收到服务器响应后立即解除
cooldown.ResetCooldown();

// 检查当前冷却状态
if (!cooldown.IsCoolingDown)
    Debug.Log("按钮可用");
```

在 Inspector 中将 `On Cooldown Start` 事件连接到启用加载转圈的函数,
将 `On Cooldown End` 事件连接到禁用转圈的函数,
无需额外代码即可实现视觉反馈。

---

## AchButtonHold

长按按钮时按指定间隔重复触发事件。
适用于音量调整、数量增减等需要在按住时连续改变数值的场景。

### Inspector 字段

| 字段 | 说明 |
|---|---|
| `Initial Delay` | 首次重复事件触发前的初始等待时间 (秒)。默认值 `0.5` |
| `Repeat Interval` | 初始延迟之后,每次重复事件之间的间隔 (秒)。默认值 `0.1` |
| `On Hold Fire` | 重复触发时执行的 `UnityEvent` |

### API

| 成员 | 说明 |
|---|---|
| `IsHolding` | 当前是否按住按钮 |

### 使用示例

应用于音量滑块的增减按钮的示例。

```csharp
// 在 VolumePlusButton GameObject 上附加 AchButtonHold,
// 并将下面的方法连接到 On Hold Fire 事件。

public void IncreaseVolume()
{
    AudioManager.Volume = Mathf.Clamp01(AudioManager.Volume + 0.05f);
}
```

```
[Button — 音量 +]
 └── [AchButtonHold]
       Initial Delay   : 0.5
       Repeat Interval : 0.1
       On Hold Fire    → IncreaseVolume()
```

按下按钮 0.5 秒后,每 0.1 秒重复调用 `IncreaseVolume()`。

---

## 两个组件配合使用

`AchButtonCooldown` 与 `AchButtonHold` 相互独立,可以同时附加到同一个按钮上。

```
[Button GameObject]
 ├── Button
 ├── AchButtonCooldown   (点击后冷却 2 秒)
 └── AchButtonHold       (长按时重复触发)
```

## 相关文档

- [UI 系统概述](/zh/guide/ui/)
- [AchTimer](/zh/guide/timer)
