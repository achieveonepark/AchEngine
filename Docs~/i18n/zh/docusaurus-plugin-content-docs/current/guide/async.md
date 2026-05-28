# AchTask

`AchTask` 是将 UniTask 与 `System.Threading.Tasks.Task` 统一为单一 API 的异步包装器。
如果项目中安装了 UniTask 则自动使用 UniTask,否则回退到 `Task`。

## 安装检测

如果项目中存在 `com.cysharp.unitask` 包,则自动定义 `ENABLE_UNITASK` 符号。
无需额外配置,asmdef 的 `versionDefines` 会自动检测,因此无需更改代码和文档。

| 环境 | 内部实现 |
|---|---|
| 已安装 UniTask | `UniTask` / `UniTask<T>` |
| 未安装 UniTask | `System.Threading.Tasks.Task` / `Task<T>` |

## await 支持

`AchTask` 可以直接 `await`。

```csharp
await AchTask.Delay(2f);
await AchTask.WaitUntil(() => isReady);
```

## 静态工厂方法

### Delay / DelayRealtime

```csharp
// 게임 시간 기준 대기 (UniTask: Time.timeScale 반영 / Task 폴백: 벽시계)
await AchTask.Delay(1.5f);

// 실제 경과 시간 기준 대기 (timeScale 무관)
await AchTask.DelayRealtime(1.5f);

// CancellationToken 지원
var cts = new CancellationTokenSource();
await AchTask.Delay(5f, cts.Token);
```

> **注意**:在 UniTask 环境下,`Delay` 会反映 `Time.timeScale`。
> 如需在慢动作期间仍按真实时间等待,请使用 `DelayRealtime`。

### WaitUntil

```csharp
// 조건이 참이 될 때까지 대기
await AchTask.WaitUntil(() => player.IsGrounded);

// 취소 가능
await AchTask.WaitUntil(() => dialogueFinished, cts.Token);
```

### WhenAll / WhenAny

```csharp
var a = AchTask.Delay(1f);
var b = AchTask.Delay(2f);
var c = AchTask.Delay(3f);

// 셋 모두 완료될 때까지
await AchTask.WhenAll(a, b, c);

// 가장 먼저 끝나는 하나가 완료될 때까지
await AchTask.WhenAny(a, b, c);
```

### CompletedTask

```csharp
// 이미 완료된 태스크 (조건 분기 반환 등에 유용)
return condition ? DoWorkAsync() : AchTask.CompletedTask;
```

## 返回值 —— AchTask\<T\>

```csharp
public static AchTask<string> FetchNameAsync()
{
    return AchTask<string>.FromResult("Player");
}

string name = await FetchNameAsync();
```

## 隐式转换

可与 UniTask 或 Task 相互转换。

```csharp
// UniTask 환경
UniTask uniTask = someAchTask;          // AchTask → UniTask
AchTask achTask = someUniTask;          // UniTask → AchTask

// Task 폴백 환경
Task task      = someAchTask.AsTask();  // AchTask → Task
AchTask from   = someTask;              // Task → AchTask (암묵적)
```

## 完整示例

```csharp
// 단순 순차 대기
async AchTask LoadSequenceAsync(CancellationToken ct)
{
    await AchTask.Delay(0.5f, ct);      // 페이드 인
    await AchTask.WaitUntil(() => dataLoaded, ct);
    await AchTask.Delay(0.2f, ct);      // 전환 여유
}

// 여러 작업 병렬 실행
async AchTask InitAllAsync()
{
    await AchTask.WhenAll(
        LoadAudioAsync(),
        LoadTableAsync(),
        LoadUserDataAsync()
    );
}

// 먼저 끝나는 쪽으로 분기
async AchTask RaceAsync()
{
    await AchTask.WhenAny(
        AchTask.Delay(3f),              // 타임아웃
        AchTask.WaitUntil(() => input)  // 유저 입력
    );
}
```

## 相关文档

- [AchTimer —— 进度跟踪计时器](/guide/timer)
- [WebRequest —— 异步 HTTP 请求](/guide/web-request)
