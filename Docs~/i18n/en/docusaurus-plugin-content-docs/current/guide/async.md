# AchTask

`AchTask` is an async wrapper that unifies UniTask and `System.Threading.Tasks.Task` under a single API.
When UniTask is installed, it uses UniTask automatically; otherwise it falls back to `Task`.

## Auto-Detection

If the `com.cysharp.unitask` package is present in your project, the `ENABLE_UNITASK` symbol is defined automatically via `versionDefines` in the asmdef. No manual configuration is required.

| Environment | Internal implementation |
|---|---|
| UniTask installed | `UniTask` / `UniTask<T>` |
| No UniTask | `System.Threading.Tasks.Task` / `Task<T>` |

## await Support

`AchTask` can be awaited directly.

```csharp
await AchTask.Delay(2f);
await AchTask.WaitUntil(() => isReady);
```

## Static Factories

### Delay / DelayRealtime

```csharp
// Wait in game time (UniTask: respects Time.timeScale / Task fallback: wall-clock)
await AchTask.Delay(1.5f);

// Wait in real elapsed time (unaffected by timeScale)
await AchTask.DelayRealtime(1.5f);

// CancellationToken support
var cts = new CancellationTokenSource();
await AchTask.Delay(5f, cts.Token);
```

> **Note**: `Delay` respects `Time.timeScale` in UniTask environments.
> Use `DelayRealtime` if you need wall-clock time during slow-motion or pause.

### WaitUntil

```csharp
// Wait until a condition becomes true
await AchTask.WaitUntil(() => player.IsGrounded);

// With cancellation
await AchTask.WaitUntil(() => dialogueFinished, cts.Token);
```

### WhenAll / WhenAny

```csharp
var a = AchTask.Delay(1f);
var b = AchTask.Delay(2f);
var c = AchTask.Delay(3f);

// Wait for all to complete
await AchTask.WhenAll(a, b, c);

// Wait for the first to complete
await AchTask.WhenAny(a, b, c);
```

### CompletedTask

```csharp
// An already-completed task (useful for conditional returns)
return condition ? DoWorkAsync() : AchTask.CompletedTask;
```

## Return Values — AchTask\<T\>

```csharp
public static AchTask<string> FetchNameAsync()
{
    return AchTask<string>.FromResult("Player");
}

string name = await FetchNameAsync();
```

## Implicit Conversions

`AchTask` converts to and from the underlying type.

```csharp
// UniTask environment
UniTask uniTask = someAchTask;          // AchTask → UniTask
AchTask achTask = someUniTask;          // UniTask → AchTask

// Task fallback
Task task      = someAchTask.AsTask();  // AchTask → Task
AchTask from   = someTask;              // Task → AchTask (implicit)
```

## Full Examples

```csharp
// Simple sequential wait
async AchTask LoadSequenceAsync(CancellationToken ct)
{
    await AchTask.Delay(0.5f, ct);      // fade in
    await AchTask.WaitUntil(() => dataLoaded, ct);
    await AchTask.Delay(0.2f, ct);      // transition buffer
}

// Parallel initialization
async AchTask InitAllAsync()
{
    await AchTask.WhenAll(
        LoadAudioAsync(),
        LoadTableAsync(),
        LoadUserDataAsync()
    );
}

// Race between timeout and user input
async AchTask RaceAsync()
{
    await AchTask.WhenAny(
        AchTask.Delay(3f),              // timeout
        AchTask.WaitUntil(() => input)  // user input
    );
}
```

## Related Docs

- [AchTimer — progress-tracking timer](/guide/timer)
- [WebRequest — async HTTP](/guide/web-request)
