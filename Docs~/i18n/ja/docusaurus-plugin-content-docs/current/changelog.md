# 変更履歴

## 1.0.5

**新機能**
- `AchTask` / `AchTask<T>` を追加しました。UniTask と `System.Threading.Tasks.Task` を単一の API に統合する非同期ラッパーです。`com.cysharp.unitask` パッケージが検出されると `ENABLE_UNITASK` シンボルが自動定義されて UniTask で動作し、無い場合は `Task` にフォールバックします。`Delay`、`DelayRealtime`、`WaitUntil`、`WhenAll`、`WhenAny`、`CompletedTask`、暗黙的変換を提供します。

**ドキュメント**
- `AchTask` の日本語・英語ドキュメントを追加しました (`guide/async`)。

## 1.0.4

**破壊的変更**
- `SoundManager` を削除しました。すべての利用箇所を新しい `AudioManager` に置き換える必要があります。

**新機能**
- `AudioManager` を追加しました。`SoundManager` の置き換えで、BGM クロスフェード (`PlayBgm(clip, fadeDuration)`、`StopBgm(fadeDuration)`)、BGM 音量フェード (`SetBgmVolume(volume, fadeDuration)`)、チャンネル別ミュート (`MuteBgm`、`MuteSfx`、`MuteAll`)、8 チャンネル同時再生 SFX プール、3D 空間オーディオ (`PlaySfxAt(clip, worldPosition)`) を提供します。
- `AchTimer` を追加しました。`AchTimer.Wait(seconds)` / `AchTimer.WaitRealtime(seconds)` でシンプルな待機を処理でき、`AchTimer.Start(duration)` は `Elapsed`、`Remaining`、`Progress`(0–1)、`IsDone`、`Cancel()`、直接 `await` などを提供する `AchTimerHandle` を返します。`CancellationToken` と `useUnscaledTime` をサポートします。内部の `AchTimerRunner` はアプリ起動時に自動生成されるため、シーンへの設置は不要です。
- `UIAchTimer` コンポーネントを追加しました。`Bind(handle)` / `Unbind()` で `AchTimerHandle` を `Text` や `Slider` に接続し、進行状況をリアルタイムに表示します。
- `AchButtonCooldown` コンポーネントを追加しました。クリック直後に `Button` を無効化し、指定時間後に自動で再有効化します。カウントダウン用 `Text` ラベルや `OnCooldownStart` / `OnCooldownEnd` UnityEvent を内蔵しており、`StartCooldown()`、`ResetCooldown()`、`IsCoolingDown` を提供します。
- `AchButtonHold` コンポーネントを追加しました。ボタンを押している間、設定された間隔で UnityEvent を繰り返し発火します。`InitialDelay` と `RepeatInterval` を調整できます。
- `AchDebugConsole` を追加しました。Unity のレンダースレッドに影響しないネイティブ UI オーバーレイのデバッグコンソールです。Android ではドラッグ可能な `WindowManager` オーバーレイ (`SYSTEM_ALERT_WINDOW` 権限が必要)、iOS では `UIWindowLevelAlert + 100` の `UIWindow`、エディターでは `DrawEditorGUI()` による IMGUI フォールバックとして動作します。API: `Show()`、`Hide()`、`Toggle()`、`Clear()`、`IsVisible`。
- `RedDot.ClearAll()` を追加しました。すべてのノードのカウントを一度に 0 にリセットします。
- `RedDotBadge` にクリッククリア機能を追加しました。`Clear On Click`(既定値 `true`)と `Button` フィールドを追加し、関連付けられたボタンを押すと対応するキーの `RedDot.Clear(key)` が自動的に呼び出されます。

**ドキュメント**
- ソースコードと食い違っていた API 関連の誤り 15 件を修正しました。`ServiceLocator.Get<T>()` → `Resolve<T>()`、`UIView` ライフサイクルフックのシグネチャ修正 (`object payload`)、`CloseSelf()` の修正、存在しない `Show<T>()` / `Close<T>()` / `CloseLayer()` オーバーロードの削除、`AchEngineScope` ↔ `ServiceLocator` ライフサイクル図の修正、`IServiceBuilder` 登録構文の修正、`ISaveService.Configure()` の所有権明示、パスファインディングドキュメントから `Rigidbody2D.MovePosition()` 参照の削除、`Selectable<T>.mChanged` イベント名の修正、`Build()` の GET/POST 限定の明示。
- 新機能全体に対する日本語・英語ドキュメントを追加しました: `AudioManager`、`AchTimer` + `UIAchTimer`、`AchButtonCooldown` + `AchButtonHold`、`AchDebugConsole`。
- RedDot ドキュメントに `ClearAll()` とクリッククリアバッジの使い方を追加しました。

## 1.0.3

- `SaveManager`、`ISaveService`、`LocalSaveService` を追加しました。保存ロジックを `PlayerManager` から分離した抽象化レイヤーで、同期/非同期 API の両方を提供し、将来的に Firestore、AWS などのクラウドバックエンドへ差し替えられるよう設計しています。
- `PlayerManager` から保存・読み込みロジックを削除しました。現在は型別のデータコンテナ管理 (`Add`、`Get`、`Remove`) のみを担当します。
- `AchProjectile` を追加しました。Rigidbody2D を必要としない直線・誘導弾統合の発射体コンポーネントです。
- `AchFollower` を完全な独立コンポーネントへリファクタリングしました。`AchMover` への依存を削除しています。
- FontAsset Maker に多言語 FontAsset ビルド機能を追加しました。日本語・英語・韓国語をマルチチェックで選択し、それぞれ個別の `*_TMP.asset` ファイルを生成できます。
- すべてのランタイム非同期 API を `System.Threading.Tasks.Task` に統一しました。中間抽象だった `AchTask` を削除しています。

## 1.0.2

- Unity Entities 向けの選択 ECS ヘルパーを追加しました。World、CommandBuffer、Baker、System、DI ラッパーを含みます。
- Managers、Singleton、Log、WebRequest、PlayerData、QuickSave などのゲームフレームワークランタイムモジュールを追加しました。
- Unity オブジェクト、UI コンポーネント、コレクション、文字列、デリゲート、Task、共通ユーティリティを扱う Runtime Extensions アセンブリを追加しました。
- A* Pathfinding ユーティリティと Grid Baker を追加しました。
- AchMover ベースの移動ヘルパーを追加しました。
- RedDot 通知バッジのランタイム機能を追加しました。
- Drag、Object Touch、Binding、Open/Close Button などの UI コンポーネントヘルパーを追加しました。
- AchEngine の主要システムをまとめて紹介する 3 シーン構成の Full Sample を追加しました。
- Addressables、DI、Localization、Table、UI のドキュメントを日本語/英語の両方で補強しました。
- Domain Reload がオフになった Enter Play Mode でも静的状態が初期化されるよう対応しました。
- ドキュメントサイト、Mermaid 図、相互リンク、JSON 処理関連の問題を修正しました。
- Editor Decorators モジュールと関連ドキュメントを削除しました。
- ルート README をドキュメントリンク中心のシンプルなランディングページに整理しました。

## 1.0.1

- Table JSON データを Google Sheets インポート用の CSV としてエクスポートするツールを追加しました。個別ファイルおよびフォルダ単位の変換をサポートします。
