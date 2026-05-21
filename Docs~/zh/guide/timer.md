# AchTimer

`AchTimer` 是基于 async/await 的计时器工具。
提供两种使用方式:简单等待和实时进度跟踪。

## 简单等待

只需等待完成时,使用 `Wait` 或 `WaitRealtime`。

```csharp
// 게임 시간 기준 3초 대기
await AchTimer.Wait(3f);

// 실제 시간 기준 3초 대기 (Time.timeScale 영향 없음)
await AchTimer.WaitRealtime(3f);
```

| 方法 | 说明 |
|---|---|
| `Wait(seconds)` | 基于游戏时间(`Time.deltaTime`)等待 |
| `WaitRealtime(seconds)` | 基于真实时间(`Time.unscaledDeltaTime`)等待。不受慢动作或暂停影响 |

## 进度跟踪 (`AchTimerHandle`)

如果需要显示进度、查询剩余时间或手动取消,请使用 `AchTimer.Start()`。
返回的 `AchTimerHandle` 可以直接 `await`。

```csharp
var timer = AchTimer.Start(5f);
await timer;
```

### AchTimerHandle 属性

| 属性 | 类型 | 说明 |
|---|---|---|
| `Duration` | `float` | 计时器总持续时间(秒) |
| `Elapsed` | `float` | 当前已经过的时间(秒) |
| `Remaining` | `float` | 剩余时间(秒)。不会小于 0 |
| `Progress` | `float` | 完成比例(0 = 开始,1 = 完成) |
| `IsDone` | `bool` | 是否完成或已取消 |
| `IsCancelled` | `bool` | 是否已取消 |
| `Task` | `Task` | 表示完成的 Task 对象 |

### await handle 与 await handle.Task

两种形式的行为完全相同。取消时会抛出 `TaskCanceledException`。

```csharp
// 두 방법 모두 동일
await timer;
await timer.Task;
```

直接引用 `Task` 时,可以在其他地方处理完成,或用于 `Task.WhenAny` 等场景。

```csharp
var timer = AchTimer.Start(5f);

// 진행률은 다른 Update 로직에서 참조하고
// 완료는 별도로 처리
_ = timer.Task.ContinueWith(_ => Debug.Log("완료"));
```

### Cancel()

调用 `Cancel()` 会立即停止计时器,并将 `IsCancelled` 设为 `true`。
对已完成的句柄无任何效果。

```csharp
var timer = AchTimer.Start(10f);

// 조건 충족 시 수동 취소
if (playerDied)
    timer.Cancel();
```

## useUnscaledTime 参数

将 `AchTimer.Start()` 的 `useUnscaledTime` 设置为 `true`,则无论 `Time.timeScale` 如何,都按实际经过的时间运行。

```csharp
// 슬로우모션 중에도 정상 속도로 진행
var timer = AchTimer.Start(3f, useUnscaledTime: true);
```

## CancellationToken 支持

传入外部的 `CancellationToken`,当该 token 被取消时,计时器也会一同取消。

```csharp
var cts = new CancellationTokenSource();

// 10초 대기 — cts.Cancel() 호출 시 즉시 중단
await AchTimer.Wait(10f, cts.Token);

// Start()에도 동일하게 사용 가능
var timer = AchTimer.Start(10f, cancellationToken: cts.Token);
```

## UIAchTimer 组件

`UIAchTimer` 是一个将 `AchTimerHandle` 的进度实时显示到 `Text` 和 `Slider` 上的 MonoBehaviour。

**Inspector 字段**

| 字段 | 说明 |
|---|---|
| `Time Text` | 用于显示时间值的 `Text` 组件(可选) |
| `Progress Slider` | 用于显示进度(0~1)的 `Slider` 组件(可选) |
| `Show Remaining` | 为 `true` 时显示剩余时间,`false` 时显示已用时间 |
| `Format` | 时间显示格式字符串。`{0}` 会被替换为时间值(默认值:`{0:F1}`) |

**从脚本中绑定**

```csharp
var timer = AchTimer.Start(5f);
GetComponent<UIAchTimer>().Bind(timer);
await timer;
```

计时器完成时,组件会自动调用 `Unbind()`。
若需手动解除绑定,请直接调用 `Unbind()`。

## 完整使用示例

```csharp
// 단순 대기
await AchTimer.Wait(3f);

// 진행 상황 추적
var timer = AchTimer.Start(5f);
timerDisplay.Bind(timer);
await timer;

// CancellationToken으로 취소
var cts = new CancellationTokenSource();
await AchTimer.Wait(10f, cts.Token);
```

```csharp
// 스킬 시전 중 이동하면 취소되는 예시
var cts = new CancellationTokenSource();
var castTimer = AchTimer.Start(2f, cancellationToken: cts.Token);
castBar.Bind(castTimer);

try
{
    await castTimer;
    // 시전 완료
    FireSkill();
}
catch (TaskCanceledException)
{
    // 이동으로 취소됨
    ShowCancelMessage();
}
```

## 相关文档

- [UI 系统概述](/zh/guide/ui/)
- [按钮扩展组件](/zh/guide/ui/button-extras)
