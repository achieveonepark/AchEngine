# キー定数のコード生成

文字列キーをハードコーディングするとタイプミスが発生しやすくなります。
AchEngine は JSON キーを **タイプセーフなネストクラス** に変換するコードジェネレーターを提供します。

## 変換例

JSON キー:
```json
{
  "menu.start": "게임 시작",
  "menu.settings": "설정",
  "dialog.confirm": "확인",
  "item.sword.name": "철 검"
}
```

生成された C# クラス:
```csharp
// 자동 생성 — 직접 수정하지 마세요
public static class L
{
    public static class Menu
    {
        public const string Start    = "menu.start";
        public const string Settings = "menu.settings";
    }

    public static class Dialog
    {
        public const string Confirm = "dialog.confirm";
    }

    public static class Item
    {
        public static class Sword
        {
            public const string Name = "item.sword.name";
        }
    }
}
```

## コード生成設定

**Project Settings › AchEngine › Localization › キー定数のコード生成** で:

| 項目 | デフォルト値 |
|---|---|
| **クラス名** | `L` |
| **名前空間** | (空の場合はグローバル名前空間) |
| **出力パス** | `Assets/Generated/` |

**キー定数生成** ボタンをクリックすると `{出力パス}/{クラス名}.cs` ファイルが生成されます。

## LocalizedString コンポーネント

Inspector でキーを指定する際は `LocalizedString` 型を使用します。

```csharp
public class ItemNameDisplay : MonoBehaviour
{
    [SerializeField] private LocalizedString _nameKey;

    private void Start()
    {
        GetComponent<Text>().text = _nameKey.Value;
    }
}
```

Inspector で `_nameKey` フィールドにキーを入力すると、カスタム PropertyDrawer が
現在のロケールの翻訳プレビューを表示します。
