# UIBindingManager *(ACHENGINE_R3)*

`UIBindingManager`はR3の`Subject<T>`を利用した型ベースのpub/subメッセージバスです。
UIコンポーネント間の依存関係なしにイベントをやり取りできます。

R3パッケージ(`com.cysharp.r3`)がインストールされている場合のみ有効化されます。

## 発行 (Publish)

```csharp
using AchEngine.UI;

// メッセージタイプの定義
public struct GoldChangedMessage
{
    public int Amount;
}

// 発行
UIBindingManager.Publish(new GoldChangedMessage { Amount = 500 });
```

## 購読 (Subscribe)

```csharp
using AchEngine.UI;

// 購読 — IDisposableを保存しておき、OnDestroyで解除してください
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

## ユーティリティメソッド

```csharp
// 特定の型のSubjectが登録されているかを確認
bool exists = UIBindingManager.Contains<GoldChangedMessage>();

// すべてのSubjectを初期化 (シーン遷移時など)
UIBindingManager.ClearAll();
```

## API一覧

| メソッド | 説明 |
|---|---|
| `Publish<T>(T)` | メッセージ発行 |
| `Subscribe<T>(Action<T>)` | メッセージ購読、`IDisposable`を返す |
| `Contains<T>()` | 該当する型のSubjectの存在有無 |
| `ClearAll()` | すべてのSubjectを削除 |

> R3がない場合、`UIBindingManager`はコンパイルから除外されます。インストール状況は**Window › AchEngine › AchEngine Info**で確認してください。
