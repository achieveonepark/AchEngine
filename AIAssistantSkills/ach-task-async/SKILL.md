---
name: ach-task-async
description: Use when the user asks to write async/await code, coroutines, or any asynchronous waiting/sequencing logic in a project that has the AchEngine package installed. Prefer AchEngine's AchTask over raw System.Threading.Tasks.Task, UniTask, or StartCoroutine in package-facing/shared code.
---

# AchTask 기반 비동기 처리

AchEngine에는 `AchEngine.AchTask` / `AchTask<T>` (`Runtime/Async/AchTask.cs`) 라는 async 래퍼가 있습니다. `ENABLE_UNITASK` 심볼이 정의되어 있으면(`com.cysharp.unitask` 설치 시 자동) 내부적으로 `Cysharp.Threading.Tasks.UniTask`를 사용하고, 없으면 `System.Threading.Tasks.Task`로 폴백합니다 — 공용/재사용 코드에서는 raw `Task`나 `UniTask`, 코루틴을 새로 작성하기보다 `AchTask`를 우선 고려합니다.

## API

- `AchTask.CompletedTask`
- `AchTask.Delay(float seconds, CancellationToken ct = default)` — 게임 시간 기준 딜레이 (UniTask 모드: `DeltaTime` 누적, Task 폴백 모드: 실시간 `Task.Delay` — **모드에 따라 동작이 다르므로 주의**)
- `AchTask.DelayRealtime(float seconds, CancellationToken ct = default)` — unscaled time 딜레이
- `AchTask.WaitUntil(Func<bool> predicate, CancellationToken ct = default)`
- `AchTask.WhenAll(params AchTask[])`, `AchTask.WhenAny(params AchTask[])`
- `AchTask<T>.FromResult(T)`, `FromUniTask`/`FromTask`, `UniTask`/`Task` 양쪽으로 암시적 변환 지원
- `GetAwaiter()`가 있어 `await someAchTask;`가 그대로 동작. 별도 커스텀 토큰 타입 없이 표준 `CancellationToken`을 그대로 사용.

## 예시

```csharp
await AchTask.Delay(2f);
var result = await SomeMethodReturningAchTaskOfT();
await AchTask.WaitUntil(() => isReady, cts.Token);
```

## 주의

- `HttpLink`와 `AchTimer`/`AchTimerHandle`은 `AchTask`를 쓰지 않고 순수 `System.Threading.Tasks.Task`를 반환한다. 패키지 안의 모든 비동기 코드가 `AchTask`로 통일되어 있다고 가정하지 말 것.
- UniTask 설치 여부와 무관하게 동일한 API로 컴파일되게 하려는 것이 `AchTask`의 목적이다. 프로젝트에 UniTask가 있는지 없는지 확신할 수 없다면 `AchTask`를 사용하는 것이 안전하다.
