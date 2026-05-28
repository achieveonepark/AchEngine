# AchTimer

`AchTimer`はasync/awaitベースのタイマーユーティリティです。
単純な待機からリアルタイムな進行状況の追跡まで、2種類の使用方法を提供します。

## 単純な待機

完了を待つだけでよい場合は、`Wait`または`WaitRealtime`を使用してください。

```csharp
// 게임 시간 기준 3초 대기
await AchTimer.Wait(3f);

// 실제 시간 기준 3초 대기 (Time.timeScale 영향 없음)
await AchTimer.WaitRealtime(3f);
```

| メソッド | 説明 |
|---|---|
| `Wait(seconds)` | ゲーム時間(`Time.deltaTime`)を基準に待機 |
| `WaitRealtime(seconds)` | 実時間(`Time.unscaledDeltaTime`)を基準に待機。スローモーション・一時停止の影響を受けない |

## 進行状況の追跡 (`AchTimerHandle`)

進行率の表示、残り時間の取得、手動キャンセルが必要な場合は`AchTimer.Start()`を使用してください。
返された`AchTimerHandle`はそのまま`await`できます。

```csharp
var timer = AchTimer.Start(5f);
await timer;
```

### AchTimerHandleプロパティ

| プロパティ | 型 | 説明 |
|---|---|---|
| `Duration` | `float` | タイマーの総継続時間(秒) |
| `Elapsed` | `float` | これまでの経過時間(秒) |
| `Remaining` | `float` | 残り時間(秒)。0未満にはならない |
| `Progress` | `float` | 完了割合(0 = 開始、1 = 完了) |
| `IsDone` | `bool` | 完了またはキャンセルされたかどうか |
| `IsCancelled` | `bool` | キャンセルされたかどうか |
| `Task` | `Task` | 完了を表すTaskオブジェクト |

### await handle と await handle.Task

両者は同一の挙動をします。キャンセル時には`TaskCanceledException`がスローされます。

```csharp
// 두 방법 모두 동일
await timer;
await timer.Task;
```

`Task`を直接参照すれば、他の場所で完了を処理したり、`Task.WhenAny`などに活用できます。

```csharp
var timer = AchTimer.Start(5f);

// 진행률은 다른 Update 로직에서 참조하고
// 완료는 별도로 처리
_ = timer.Task.ContinueWith(_ => Debug.Log("완료"));
```

### Cancel()

`Cancel()`を呼び出すと、タイマーは即座に停止し、`IsCancelled`が`true`になります。
すでに完了したハンドルに対しては何の効果もありません。

```csharp
var timer = AchTimer.Start(10f);

// 조건 충족 시 수동 취소
if (playerDied)
    timer.Cancel();
```

## useUnscaledTimeパラメータ

`AchTimer.Start()`の`useUnscaledTime`を`true`に設定すると、`Time.timeScale`に関係なく実際の経過時間を基準に動作します。

```csharp
// 슬로우모션 중에도 정상 속도로 진행
var timer = AchTimer.Start(3f, useUnscaledTime: true);
```

## CancellationTokenサポート

外部の`CancellationToken`を渡すと、トークンがキャンセルされた時にタイマーも一緒にキャンセルされます。

```csharp
var cts = new CancellationTokenSource();

// 10초 대기 — cts.Cancel() 호출 시 즉시 중단
await AchTimer.Wait(10f, cts.Token);

// Start()에도 동일하게 사용 가능
var timer = AchTimer.Start(10f, cancellationToken: cts.Token);
```

## UIAchTimerコンポーネント

`UIAchTimer`は、`AchTimerHandle`の進行状況を`Text`と`Slider`にリアルタイムで表示するMonoBehaviourです。

**Inspectorフィールド**

| フィールド | 説明 |
|---|---|
| `Time Text` | 時間値を表示する`Text`コンポーネント(任意) |
| `Progress Slider` | 進行率(0~1)を表示する`Slider`コンポーネント(任意) |
| `Show Remaining` | `true`なら残り時間、`false`なら経過時間を表示 |
| `Format` | 時間表示用のフォーマット文字列。`{0}`が時間値に置換されます(デフォルト: `{0:F1}`) |

**スクリプトからのバインド**

```csharp
var timer = AchTimer.Start(5f);
GetComponent<UIAchTimer>().Bind(timer);
await timer;
```

タイマーが完了するとコンポーネントが自動的に`Unbind()`を呼び出します。
手動で解除する場合は`Unbind()`を直接呼び出してください。

## 全体の使用例

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

## 関連ドキュメント

- [UIシステム概要](/guide/ui/)
- [ボタン拡張コンポーネント](/guide/ui/button-extras)
