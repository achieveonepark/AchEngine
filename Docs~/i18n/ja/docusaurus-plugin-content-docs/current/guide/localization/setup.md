# 設定 & データベース

## 1. LocalizationSettings の作成

**Project Settings › AchEngine › Localization** を開くと、
設定がない場合に `Assets/Resources/LocalizationSettings.asset` を自動生成します。

## 2. LocaleDatabase の作成

**Database 生成** ボタンをクリックし、保存場所を選択します。

```
Assets/
└── GameData/
    └── LocaleDatabase.asset
```

## 3. JSON ファイルの追加

各ロケールの JSON ファイルを `LocaleDatabase` に登録します。

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

```json
// en.json — English
{
  "menu.start": "Start Game",
  "menu.settings": "Settings",
  "dialog.confirm": "OK",
  "item.sword.name": "Iron Sword",
  "item.sword.desc": "A plain iron sword."
}
```

JSON キーは **ドット記法** でネストせずにフラットに記述します。

## 4. ロケール設定

**Project Settings › AchEngine › Localization › ロケール設定** で:

| 項目 | 説明 |
|---|---|
| **デフォルトロケール** | アプリの初回起動時に使用するロケール |
| **フォールバックロケール** | 現在のロケールにキーがない場合に使用するロケール |
| **システム言語の自動検出** | デバイス言語と一致するロケールに自動設定 |
| **アプリ起動時の自動初期化** | `LocalizationManager.Initialize()` を Awake 時に自動呼び出し |

## ランタイム初期化

自動初期化がオフの場合は手動で初期化します。

```csharp
private async void Start()
{
    await LocalizationManager.InitializeAsync();
    Debug.Log("Localization 준비 완료");
}
```

## エディターウィンドウ

**エディターを開く** ボタンをクリックすると `LocalizationEditorWindow` が開きます。
すべてのロケールの翻訳をテーブル形式で編集できます。

| 機能 | 説明 |
|---|---|
| キーの追加/削除 | キーを追加するとすべてのロケールに空の値が作成される |
| CSV インポート | 翻訳作業結果の CSV をインポート |
| JSON エクスポート/インポート | ロケール JSON ファイルを直接インポート/エクスポート |
