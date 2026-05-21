# インストール

## UPM — Git URL でインストール (推奨)

Unity エディターで **Window › Package Manager** を開き、
**`+` ボタン → Add package from git URL...** を選択した後、以下の URL を入力してください。

```
https://github.com/achieveonepark/AchEngine.git
```

特定のバージョンに固定したい場合はタグを付けます。

```
https://github.com/achieveonepark/AchEngine.git#1.0.0
```

## manifest.json でインストール

プロジェクトの `Packages/manifest.json` を直接編集する方法です。

```json
{
  "dependencies": {
    "com.engine.achieve": "https://github.com/achieveonepark/AchEngine.git",
    ...
  }
}
```

## 選択パッケージのインストール

AchEngine は以下のパッケージを選択的にサポートします。インストールすると該当機能が自動的に有効化されます。
**Window › AchEngine › AchEngine Info** ウィンドウから、各パッケージのインストール状況と有効になる機能を確認できます。

| パッケージ | Package ID | 有効化される機能 |
|---|---|---|
| VContainer | `jp.hadashikick.vcontainer` | DI コンテナ (AchEngineScope、ServiceLocator) |
| MemoryPack | `com.cysharp.memorypack` | QuickSave シリアライズ (`USE_QUICK_SAVE`) |
| Addressables | `com.unity.addressables` | AddressableManager、RemoteContentManager |
| R3 | `com.cysharp.r3` | UIBindingManager (Reactive pub/sub) |

### VContainer の手動インストール

`Packages/manifest.json` の `scopedRegistries` に以下を追加してください。

```json
{
  "scopedRegistries": [
    {
      "name": "package.openupm.com",
      "url": "https://package.openupm.com",
      "scopes": ["jp.hadashikick.vcontainer"]
    }
  ],
  "dependencies": {
    "jp.hadashikick.vcontainer": "1.15.0"
  }
}
```

### MemoryPack の手動インストール

[MemoryPack GitHub](https://github.com/Cysharp/MemoryPack) のインストールガイドを参照してください。

### R3 のインストール

[R3 GitHub](https://github.com/Cysharp/R3) のインストールガイドを参照してください。

## インストールの確認

インストールが完了すると、Unity コンソールにエラーが無く、
メニューに **Window › AchEngine › AchEngine Info** 項目が表示されるはずです。
パッケージのインストール状況はこのウィンドウから一目で確認できます。
