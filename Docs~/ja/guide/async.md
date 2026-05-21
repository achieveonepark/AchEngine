# AchTask

`AchTask`は、UniTaskと`System.Threading.Tasks.Task`を1つのAPIに統合する非同期ラッパーです。
プロジェクトにUniTaskがインストールされていれば自動的にUniTaskを使用し、なければ`Task`にフォールバックします。

## インストール検出

`com.cysharp.unitask`パッケージがプロジェクトに存在すると、`ENABLE_UNITASK`シンボルが自動的に定義されます。
別途設定は不要で、asmdefの`versionDefines`が検出するため、コードやドキュメントを書き換える必要はありません。

| 環境 | 内部実装 |
|---|---|
| UniTask インストール済み | `UniTask` / `UniTask<T>` |
| UniTask なし | `System.Threading.Tasks.Task` / `Task<T>` |

## await サポート

`AchTask`は直接`await`できます。

```csharp
await AchTask.Delay(2f);
await AchTask.WaitUntil(() => isReady);
```

## 静的ファクトリ

### Delay / DelayRealtime

```csharp
// ゲーム時間基準で待機 (UniTask: Time.timeScale反映 / Taskフォールバック: 壁時計)
await AchTask.Delay(1.5f);

// 実経過時間基準で待機 (timeScale無関係)
await AchTask.DelayRealtime(1.5f);

// CancellationToken対応
var cts = new CancellationTokenSource();
await AchTask.Delay(5f, cts.Token);
```

> **注意**: `Delay`はUniTask環境では`Time.timeScale`を反映します。
> スローモーション中も実時間で待機したい場合は`DelayRealtime`を使用してください。

### WaitUntil

```csharp
// 条件がtrueになるまで待機
await AchTask.WaitUntil(() => player.IsGrounded);

// キャンセル可能
await AchTask.WaitUntil(() => dialogueFinished, cts.Token);
```

### WhenAll / WhenAny

```csharp
var a = AchTask.Delay(1f);
var b = AchTask.Delay(2f);
var c = AchTask.Delay(3f);

// 3つすべてが完了するまで
await AchTask.WhenAll(a, b, c);

// 最初に完了したものを待つ
await AchTask.WhenAny(a, b, c);
```

### CompletedTask

```csharp
// すでに完了したタスク (条件分岐の戻り値などに便利)
return condition ? DoWorkAsync() : AchTask.CompletedTask;
```

## 戻り値 — AchTask\<T\>

```csharp
public static AchTask<string> FetchNameAsync()
{
    return AchTask<string>.FromResult("Player");
}

string name = await FetchNameAsync();
```

## 暗黙的変換

UniTaskまたはTaskと相互変換できます。

```csharp
// UniTask環境
UniTask uniTask = someAchTask;          // AchTask → UniTask
AchTask achTask = someUniTask;          // UniTask → AchTask

// Taskフォールバック環境
Task task      = someAchTask.AsTask();  // AchTask → Task
AchTask from   = someTask;              // Task → AchTask (暗黙的)
```

## 全体の例

```csharp
// 単純な順次待機
async AchTask LoadSequenceAsync(CancellationToken ct)
{
    await AchTask.Delay(0.5f, ct);      // フェードイン
    await AchTask.WaitUntil(() => dataLoaded, ct);
    await AchTask.Delay(0.2f, ct);      // 遷移の余裕
}

// 複数タスクの並列実行
async AchTask InitAllAsync()
{
    await AchTask.WhenAll(
        LoadAudioAsync(),
        LoadTableAsync(),
        LoadUserDataAsync()
    );
}

// 先に終わった方に分岐
async AchTask RaceAsync()
{
    await AchTask.WhenAny(
        AchTask.Delay(3f),              // タイムアウト
        AchTask.WaitUntil(() => input)  // ユーザー入力
    );
}
```

## 関連ドキュメント

- [AchTimer — 進捗追跡タイマー](/ja/guide/timer)
- [WebRequest — 非同期HTTPリクエスト](/ja/guide/web-request)
