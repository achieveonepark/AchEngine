# RedDotシステム

`RedDot`は通知バッジ(赤い点)を管理する静的ファサードです。
キー階層構造(`/`区切り)をサポートし、子ノードのカウントが親に自動集計されます。

## API

```csharp
namespace AchEngine.UI
{
    public static class RedDot
    {
        // 特定ノードのカウントを設定
        public static void Set(string key, int count);

        // カウントにdelta値を加算
        public static void Add(string key, int delta);

        // 集計されたカウントを取得 (自身 + すべての子の合算)
        public static int Get(string key);

        // カウントが0より大きい場合true
        public static bool HasDot(string key);

        // カウントを0に初期化
        public static void Clear(string key);

        // カウント変更イベントの購読
        public static void Subscribe(string key, Action<int> handler);

        // 購読解除
        public static void Unsubscribe(string key, Action<int> handler);
    }
}
```

## 全体初期化

`RedDot.ClearAll()` — すべてのキーのカウントを一度に0で初期化します。
シーン遷移やゲーム再起動時に便利です。

```csharp
// シーン遷移前にすべてのレッドドットを初期化
RedDot.ClearAll();
SceneManager.LoadScene("MainMenu");
```

## 階層構造

キーに`/`を使用すると自動的にツリーが構成されます。
子ノードの合算カウントが親に自動反映されます。

```
"Shop"          → "Shop/New" + "Shop/Sale" の合算
"Shop/New"      → 直接設定したカウント
"Shop/Sale"     → 直接設定したカウント
```

```csharp
RedDot.Set("Shop/New", 3);    // Shop/New = 3
RedDot.Set("Shop/Sale", 1);   // Shop/Sale = 1

RedDot.Get("Shop");           // → 4 (3 + 1 自動集計)
RedDot.HasDot("Shop");        // → true
```

## 使用例

```csharp
// 新規アイテム入手
RedDot.Set("Shop/New", newItemCount);

// クエスト完了
RedDot.Add("Quest/Daily", 1);

// 既読処理
RedDot.Clear("Quest/Daily");

// メインメニューボタン — ShopまたはQuestのいずれかがあれば表示
bool showOnMainMenu = RedDot.HasDot("Shop") || RedDot.HasDot("Quest");
```

## RedDotBadgeコンポーネント

`RedDotBadge`はUI GameObjectに付けるMonoBehaviourです。
指定したキーのカウントを自動的に検出し、dotオブジェクトを有効化・無効化します。

| フィールド | 説明 |
|---|---|
| `Key` | 購読するRedDotキー (`"Shop"`, `"Quest/Daily"`など) |
| `Dot` | カウント > 0のときに有効化するGameObject |
| `Count Label` | (任意) カウントを表示する`Text`コンポーネント。2以上のときのみ表示 |
| `Clear On Click` | (bool, デフォルトtrue) ボタンクリック時に該当キーを自動Clear |
| `Button` | クリックを検出する`Button`コンポーネント。nullの場合は自動クリア動作なし |

```
[Button GameObject]
 └── [RedDotBadge]  Key = "Shop"
      └── [DotImage]  (カウント > 0で有効化)
           └── [Text]  (任意: "3"等の数字を表示)
```

`OnEnable`時に自動的に購読し、`OnDisable`時に解除します。
シーン遷移やオブジェクトの無効化にも安全です。

### クリックでレッドドットを消す

`Clear On Click`がtrueで`Button`が接続されている場合、ボタン押下時に該当キーのカウントが自動的に0で初期化されます。
追加のコードなしで「既読処理」動作を実装できます。

```csharp
// コードで同じ動作を直接実装する場合
_shopButton.onClick.AddListener(() =>
{
    RedDot.Clear("Shop");
    OpenShopPanel();
});
```

## コードから直接購読

コンポーネントなしで直接購読することもできます。

```csharp
private void OnEnable()
{
    RedDot.Subscribe("Shop", OnShopChanged);
}

private void OnDisable()
{
    RedDot.Unsubscribe("Shop", OnShopChanged);
}

private void OnShopChanged(int count)
{
    _shopButton.SetDotVisible(count > 0);
}
```

## EnterPlayModeサポート

`RedDot`は`[RuntimeInitializeOnLoadMethod(SubsystemRegistration)]`によって
ドメインリロードなしでエディタ再生時に自動初期化されます。

## 関連ドキュメント

- [UIシステム概要](/ja/guide/ui/)
- [UIView & ライフサイクル](/ja/guide/ui/views)
