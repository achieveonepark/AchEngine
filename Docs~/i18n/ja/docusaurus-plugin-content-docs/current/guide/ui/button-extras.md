# ボタン拡張コンポーネント

`AchButtonCooldown`と`AchButtonHold`は既存の`Button`コンポーネントに追加で付ける拡張コンポーネントです。
両コンポーネントとも同じGameObjectに`Button`がある必要があります。

## AchButtonCooldown

クリック直後にボタンを無効化し、指定された時間が経過すると再び有効化します。
残り時間をラベルに表示するカウントダウン機能も内蔵されています。

### Inspectorフィールド

| フィールド | 説明 |
|---|---|
| `Cooldown` | クリック後、次のクリックまでの最小待機時間(秒)。デフォルト値 `1` |
| `Show Countdown` | 残りクールダウン時間をラベルに表示するかどうか |
| `Countdown Label` | カウントダウンを表示する`Text`コンポーネント (任意) |
| `On Cooldown Start` | クールダウンが開始されたときに発火する`UnityEvent` |
| `On Cooldown End` | クールダウンが終了したときに発火する`UnityEvent` |

### API

| メンバー | 説明 |
|---|---|
| `IsCoolingDown` | 現在クールダウン中かどうか |
| `StartCooldown()` | クールダウンを手動で開始。すでに進行中ならタイマーを再開始 |
| `ResetCooldown()` | クールダウンを即座にリセットしてボタンを再有効化 |

### 使用例

```csharp
// スクリプトでクールダウンを直接制御
var cooldown = GetComponent<AchButtonCooldown>();

// 外部条件でクールダウン開始 (例: サーバー応答までロック)
cooldown.StartCooldown();

// サーバー応答受信後に即座に解除
cooldown.ResetCooldown();

// 現在のクールダウン状態を確認
if (!cooldown.IsCoolingDown)
    Debug.Log("ボタン使用可能");
```

Inspectorで`On Cooldown Start`イベントにローディングスピナー有効化関数を、
`On Cooldown End`イベントにスピナー無効化関数を接続すれば、
追加コードなしで視覚フィードバックを実装できます。

---

## AchButtonHold

ボタンを長押しすると指定された間隔でイベントを繰り返し発火させます。
ボリューム調整や数量増減のように、押し続けている間に値を連続的に変更する際に便利です。

### Inspectorフィールド

| フィールド | 説明 |
|---|---|
| `Initial Delay` | 最初の繰り返しイベント発火までの初期待機時間(秒)。デフォルト値 `0.5` |
| `Repeat Interval` | 初期ディレイ後、各繰り返しイベント間の間隔(秒)。デフォルト値 `0.1` |
| `On Hold Fire` | 繰り返し発火時に実行される`UnityEvent` |

### API

| メンバー | 説明 |
|---|---|
| `IsHolding` | 現在ボタンを押している状態かどうか |

### 使用例

ボリュームスライダー増減ボタンへ適用する例です。

```csharp
// VolumePlusButton GameObjectにAchButtonHoldを付け、
// On Hold Fireイベントに以下のメソッドを接続します。

public void IncreaseVolume()
{
    AudioManager.Volume = Mathf.Clamp01(AudioManager.Volume + 0.05f);
}
```

```
[Button — ボリューム +]
 └── [AchButtonHold]
       Initial Delay   : 0.5
       Repeat Interval : 0.1
       On Hold Fire    → IncreaseVolume()
```

ボタンを押すと0.5秒後から0.1秒間隔で`IncreaseVolume()`が繰り返し呼び出されます。

---

## 2つのコンポーネントを併用

`AchButtonCooldown`と`AchButtonHold`は互いに独立しており、同じボタンに併せて付けることができます。

```
[Button GameObject]
 ├── Button
 ├── AchButtonCooldown   (クリック後2秒クールダウン)
 └── AchButtonHold       (長押しで繰り返し発火)
```

## 関連ドキュメント

- [UIシステム概要](/guide/ui/)
- [AchTimer](/guide/timer)
