# AchEngine の紹介

AchEngine は、Unity 開発でよく使われる機能を 1 つの UPM パッケージにまとめた **統合開発ツールキット** です。

各モジュールは独立して利用でき、VContainer、MemoryPack、Addressables などの選択パッケージが無くてもコア機能はそのまま動作します。

## モジュール構成

| モジュール | 説明 | 選択パッケージ |
|---|---|---|
| **DI** | VContainer ラッパー、ServiceLocator | `jp.hadashikick.vcontainer` |
| **UI System** | レイヤーベースの View 管理、プーリング、トランジション | - |
| **Table Loader** | Google Sheets から C# データパイプラインを生成 | `com.cysharp.memorypack` |
| **Addressables** | アセットキャッシュ、自動グループ管理、リモート配信 | `com.unity.addressables` |
| **Localization** | JSON ローカライゼーション、キーコード生成 | `com.unity.textmeshpro` (選択) |

## パッケージ情報

- **Package ID:** `com.achieve.engine`
- **バージョン:** 1.0.1
- **最小 Unity バージョン:** 6000.3
- **必須依存:** `com.unity.ugui`

## 選択パッケージ

以下のパッケージをインストールすると、該当するシンボルが **自動的に定義** され、AchEngine の高度な機能が有効になります。
インストールしなくてもコンパイルエラーは発生せず、該当機能のみが無効になります。

```
jp.hadashikick.vcontainer   -> DI コンテナを有効化         → ACHENGINE_VCONTAINER
com.cysharp.memorypack      -> バイナリシリアライズを有効化 → ACHENGINE_MEMORYPACK
com.unity.addressables      -> Addressables モジュールを有効化 → ACHENGINE_ADDRESSABLES
com.unity.textmeshpro       -> TMP Localization を有効化   → ACHENGINE_LOCALIZATION_TMP
com.unity.entities          -> ECS ラッパーを有効化         → ACHENGINE_ENTITIES
```

:::tip
選択パッケージのインストール状況は **Project Settings > AchEngine** の Overview から一目で確認でき、ボタンからそのままインストールできます。
:::
