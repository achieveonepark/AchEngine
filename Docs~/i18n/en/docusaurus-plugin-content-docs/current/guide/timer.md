# AchTimer

`AchTimer` is an async/await-based timer utility.
It provides two usage modes: simple waiting and real-time progress tracking.

## Simple Wait

Use `Wait` or `WaitRealtime` when you only need to wait for completion.

```csharp
// Wait 3 seconds in game time
await AchTimer.Wait(3f);

// Wait 3 seconds in real time (unaffected by Time.timeScale)
await AchTimer.WaitRealtime(3f);
```

| Method | Description |
|---|---|
| `Wait(seconds)` | Waits based on game time (`Time.deltaTime`) |
| `WaitRealtime(seconds)` | Waits based on real time (`Time.unscaledDeltaTime`). Not affected by slow-motion or pause |

## Progress Tracking (`AchTimerHandle`)

Use `AchTimer.Start()` when you need to display progress, query remaining time, or cancel manually.
The returned `AchTimerHandle` can be `await`ed directly.

```csharp
var timer = AchTimer.Start(5f);
await timer;
```

### AchTimerHandle Properties

| Property | Type | Description |
|---|---|---|
| `Duration` | `float` | Total duration of the timer in seconds |
| `Elapsed` | `float` | Time elapsed so far in seconds |
| `Remaining` | `float` | Time remaining in seconds. Never goes below 0 |
| `Progress` | `float` | Completion ratio (0 = start, 1 = done) |
| `IsDone` | `bool` | True when the timer has finished or been cancelled |
| `IsCancelled` | `bool` | True if the timer was cancelled |
| `Task` | `Task` | The underlying Task representing completion |

### await handle vs await handle.Task

Both forms behave identically. A `TaskCanceledException` is thrown on cancellation.

```csharp
// Both are equivalent
await timer;
await timer.Task;
```

Referencing `Task` directly is useful when chaining with `Task.WhenAny` or handling completion elsewhere.

```csharp
var timer = AchTimer.Start(5f);

// Track progress in Update while handling completion separately
_ = timer.Task.ContinueWith(_ => Debug.Log("Done"));
```

### Cancel()

Calling `Cancel()` stops the timer immediately and sets `IsCancelled` to `true`.
Has no effect on an already-completed handle.

```csharp
var timer = AchTimer.Start(10f);

// Cancel manually when a condition is met
if (playerDied)
    timer.Cancel();
```

## useUnscaledTime Parameter

Set `useUnscaledTime: true` in `AchTimer.Start()` to measure real elapsed time regardless of `Time.timeScale`.

```csharp
// Advances at normal speed even during slow-motion
var timer = AchTimer.Start(3f, useUnscaledTime: true);
```

## CancellationToken Support

Pass an external `CancellationToken` to cancel the timer when the token is cancelled.

```csharp
var cts = new CancellationTokenSource();

// Waits 10 seconds — cancelled immediately when cts.Cancel() is called
await AchTimer.Wait(10f, cts.Token);

// Also works with Start()
var timer = AchTimer.Start(10f, cancellationToken: cts.Token);
```

## UIAchTimer Component

`UIAchTimer` is a MonoBehaviour that displays `AchTimerHandle` progress on a `Text` and/or `Slider` in real time.

**Inspector Fields**

| Field | Description |
|---|---|
| `Time Text` | `Text` component to display the time value (optional) |
| `Progress Slider` | `Slider` component to display progress (0–1) (optional) |
| `Show Remaining` | `true` shows remaining time; `false` shows elapsed time |
| `Format` | Format string for the time value. `{0}` is replaced with the time (default: `{0:F1}`) |

**Binding from Code**

```csharp
var timer = AchTimer.Start(5f);
GetComponent<UIAchTimer>().Bind(timer);
await timer;
```

The component automatically calls `Unbind()` when the timer completes.
Call `Unbind()` manually to detach early.

## Full Examples

```csharp
// Simple wait
await AchTimer.Wait(3f);

// Progress tracking
var timer = AchTimer.Start(5f);
timerDisplay.Bind(timer);
await timer;

// Cancel via CancellationToken
var cts = new CancellationTokenSource();
await AchTimer.Wait(10f, cts.Token);
```

```csharp
// Skill cast that cancels on movement
var cts = new CancellationTokenSource();
var castTimer = AchTimer.Start(2f, cancellationToken: cts.Token);
castBar.Bind(castTimer);

try
{
    await castTimer;
    // Cast complete
    FireSkill();
}
catch (TaskCanceledException)
{
    // Interrupted by movement
    ShowCancelMessage();
}
```

## Related Docs

- [UI System Overview](/guide/ui/)
- [Button Extras](/guide/ui/button-extras)
