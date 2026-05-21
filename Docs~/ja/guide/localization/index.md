# Localization — 概要

AchEngine Localization は **JSON/CSV ベース** の多言語システムです。
ロケール切り替え、フォールバック、システム言語の自動検出、タイプセーフなキー定数のコード生成をサポートします。

## 主要コンポーネント

| クラス | 役割 |
|---|---|
| `LocalizationManager` | ロケール切り替え、テキスト取得のファサード |
| `LocalizationSettings` | 設定 ScriptableObject (Resources に配置) |
| `LocaleDatabase` | ロケール一覧および JSON ファイルのマッピング |
| `LocalizedString` | ランタイム多言語テキストのラッパー |
| `L` (生成クラス) | タイプセーフなキー定数 (コード生成の結果) |

## 基本的な使い方

```csharp
using AchEngine.Localization;

// 現在のロケールのテキストを取得
string text = LocalizationManager.Get("menu.start");

// タイプセーフなキー (コード生成後)
string text2 = LocalizationManager.Get(L.Menu.Start);

// ロケールを変更
LocalizationManager.SetLocale("ja");

// ロケール変更イベントを購読
LocalizationManager.OnLocaleChanged += OnLocaleChanged;
```

## JSON 形式

```json
// ko.json — 한국어
{
  "menu.start": "게임 시작",
  "menu.settings": "설정",
  "dialog.confirm": "확인",
  "item.sword.name": "철 검",
  "item.sword.desc": "평범한 철 검입니다."
}
```

JSON キーは **ドット記法** でネストせずにフラットに記述します。

## TMP コンポーネントの自動更新

`LocalizedText` コンポーネントを TextMeshPro オブジェクトに追加すると、
ロケールが変更されたときに自動でテキストを更新します。

```
[TextMeshProUGUI]
  └── LocalizedText  ← キー: "menu.start"
```

:::info TMP サポート
TextMeshPro (`com.unity.textmeshpro`) がインストールされている場合、`LocalizedText` コンポーネントが有効になります。
:::

## FontAsset Maker

**AchEngine › Localization › FontAsset Maker** でフォントを 1 つ指定してボタンを押すと、
LocaleDatabase の実際の翻訳文字列、プロジェクトの CSV/JSON/TXT TextAsset、基本 ASCII を集めて静的な TMP FontAsset を生成します。
ランタイムで新しい韓国語文字列を組み合わせる場合は、`Include Korean glyph preset` をオンにして韓国語全体のプリセットも一緒に焼き込めます。

生成された FontAsset は `Assets/Fonts/Generated` に保存されます。

## 次のステップ

- [設定 & データベース](/ja/guide/localization/setup)
- [キー定数のコード生成](/ja/guide/localization/codegen)
