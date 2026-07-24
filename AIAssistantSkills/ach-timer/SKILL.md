---
name: ach-timer
description: Use when the user asks to wait for a duration, delay an action, implement a cooldown/countdown, or show timer progress in the UI, in a project that has the AchEngine package installed. Use AchEngine's AchTimer instead of StartCoroutine(WaitForSeconds(...)) or a hand-rolled async delay loop.
---

# AchTimer 기반 대기/타이머

`AchEngine` 루트 네임스페이스 (`AchTimer.cs`, `AchTimerHandle.cs`, 내부 `AchTimerRunner.cs`). "무언가를 기다려야 한다"는 요청에는 코루틴이나 직접 만든 async 딜레이 대신 이것을 사용한다. 별도 부트스트랩 불필요 — `AchTimerRunner`가 `[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)]`로 자동 생성되고 `DontDestroyOnLoad`된다.

## API

- `AchTimer.Wait(float seconds, CancellationToken ct = default)` → `Task` — 게임/스케일 타임 기준.
- `AchTimer.WaitRealtime(float seconds, CancellationToken ct = default)` → `Task` — unscaled time 기준.
- `AchTimer.Start(float duration, bool useUnscaledTime = false, CancellationToken ct = default)` → `AchTimerHandle` — 진행률/취소 등 상태가 필요할 때.
- **`AchTimerHandle`**: `Duration`, `Elapsed`, `Remaining`, `Progress`(0~1), `IsDone`, `IsCancelled`, `Task` 프로퍼티 + `GetAwaiter()`(직접 `await handle;` 가능), `Cancel()`.

## 예시

```csharp
await AchTimer.Wait(3f);              // 단순 딜레이
await AchTimer.WaitRealtime(1.5f);     // Time.timeScale 무시

var timer = AchTimer.Start(5f);
_ = timer.Task;                       // fire-and-forget
Debug.Log(timer.Progress);            // UI에서 폴링 (UIAchTimer 컴포넌트 참고)
timer.Cancel();
```

UI에 카운트다운을 표시해야 하면 `AchTimerHandle`을 직접 폴링하는 대신 `UIAchTimer` 컴포넌트(`Bind(handle)`/`Unbind()`)를 `Text`/`Slider`에 붙이는 것도 고려한다.
