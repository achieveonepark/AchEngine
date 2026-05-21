# AchMover

`AchMover`は、アタッチするだけで即座に動作する2Dキャラクター移動コンポーネントです。
**Rigidbody2Dを使用しません** — 独自の衝突処理（Move-and-Slide）、重力、傾斜、階段、地面スナップをすべて自前で実行します。
必要なコンポーネントは`CapsuleCollider2D`ひとつだけで、自動的に追加・管理されます。

## 使い方

キャラクターのGameObjectに**Ach Mover**コンポーネントを追加するだけです。

| モード | 操作 |
|---|---|
| **Platformer** | `A` / `D` で左右移動、`Space` / `W` / `↑` でジャンプ |
| **TopDown** | `WASD` / 矢印キーで上下左右移動 |

> **New Input System** 完全対応 — Player SettingsでInput System Packageに切り替えても自動的に`Keyboard.current` / `Gamepad.current`経由で動作します。

> 同じGameObjectに`Rigidbody2D`が存在する場合、transform移動と衝突するため自動的に`simulated = false`に設定されます。

## Inspector

### Movement

| フィールド | デフォルト値 | 説明 |
|---|---|---|
| `MoveSpeed` | 5 | 移動速度 (Units/sec) |
| `JumpForce` | 12 | ジャンプ力 (`UseGravity = true` 専用) |
| `Mode` | Platformer | `Platformer` / `TopDown` |

### Physics

| フィールド | デフォルト値 | 説明 |
|---|---|---|
| `UseGravity` | true | 重力・地面検出の有効/無効 |
| `GravityScale` | 3 | 重力倍率 |
| `FallMultiplier` | 2 | 落下時の追加重力倍率 — 大きいほど重い感触 |
| `MaxFallSpeed` | 20 | 最大落下速度 |

### Slopes & Stairs

| フィールド | デフォルト値 | 説明 |
|---|---|---|
| `MaxSlopeAngle` | 50° | 登れる最大傾斜角度。これを超える表面は壁として扱われ滑り落ちる |
| `StepHeight` | 0.3 | 自動で登れる階段・段差の最大高さ (Units) |

### Control

| フィールド | デフォルト値 | 説明 |
|---|---|---|
| `Movable` | true | `false`の場合は入力を遮断し、コードからのみ制御可能 |
| `FlipSprite` | true | 移動方向に応じて`transform.localScale.x`を反転 — 子オブジェクト（武器、エフェクト、ヒットボックスなど）も一緒に反転 |

## 独自の衝突システム

AchMoverは毎`FixedUpdate`で以下の手順を実行します。

1. **速度計算** — 入力、重力、ジャンプを総合して当該フレームの速度を決定
2. **Move-and-Slide** — `CapsuleCollider2D.Cast`で進行方向を検査し、衝突面に沿ってスライド。最大4回繰り返しでコーナーや複数面を処理
3. **階段の自動昇り** — 壁に阻まれた際、`StepHeight`以内の段差なら自動的に登る（持ち上げ → 横移動 → 下ろす）
4. **地面判定** — コライダーの下に向けてカプセルキャストし、表面角度が`MaxSlopeAngle`以下ならgrounded
5. **地面スナップ** — 斜面を下る際に表面から離れないように再度密着させる
6. **ペネトレーション補正** — 最後に残った重なりを確認して押し出す

## 状態プロパティ

```csharp
bool    isGrounded   = mover.IsGrounded;    // 地面に接地しているか (UseGravity = true 専用)
bool    isMoving     = mover.IsMoving;      // 現在移動中か (入力ベース)
Vector2 velocity     = mover.Velocity;      // 現在の速度
Vector2 groundNormal = mover.GroundNormal;  // 足元表面の法線 (なければ Vector2.up)
```

## ジョイスティック / カスタム入力の接続

```csharp
// オンスクリーンジョイスティックを接続
mover.InputProvider = () => joystick.Direction;

// 解除 (キーボード入力に戻す)
mover.InputProvider = null;
```

## コード制御 API

`Movable`の値に関わらず、いつでも呼び出すことができます。

```csharp
mover.Jump();                                   // ジャンプ (UseGravity = true 専用)
mover.Teleport(new Vector2(10f, 0f));           // テレポート
mover.SetVelocity(new Vector2(-5f, 4f));        // 速度を直接設定 (ノックバックなど)
mover.AddForce(Vector2.left * 10f);             // Impulse形式で速度に加算
mover.Stop();                                   // 即時停止
```

> Rigidbody2Dがないため、`AddForce`は常に即座に速度へ加算されるImpulseとして動作します。`forceMode`引数は互換性のために残っていますが無視されます。

### ノックバックの例

```csharp
mover.Movable = false;
mover.SetVelocity(new Vector2(-6f, 5f));
await Task.Delay(400);
mover.Movable = true;
```

## UseGravity の組み合わせ

| Mode | UseGravity | 動作 |
|---|---|---|
| Platformer | true | 重力 + ジャンプ + 傾斜 + 階段、左右移動 |
| Platformer | false | 重力なし、左右移動のみ（垂直方向は自由） |
| TopDown | false | 重力なし、4方向自由移動 |
| TopDown | true | 4方向移動 + 自由落下（特殊なケース） |

## AchFollower — AI 追跡

`AchFollower`は指定したターゲットに向かって移動する独立したコンポーネントです。
`AchMover`とはまったく関係なく、どのGameObjectにも単独で使用できます。

| フィールド | デフォルト値 | 説明 |
|---|---|---|
| `Target` | null | 追跡対象のTransform |
| `MoveSpeed` | 5 | 移動速度 (Units/sec) |
| `StopDistance` | 0.5 | この距離以下になると停止 |

```csharp
// ターゲット設定
follower.SetTarget(player.transform);

// ターゲット解除
follower.ClearTarget();
```

`transform.position`を直接移動します。

A*経路に沿った高度な移動については、[A\* 経路探索](./pathfinding) ドキュメントを参照してください。

## AchProjectile — 発射体

直進、誘導など多様な発射体を1つのコンポーネントで管理します。
Inspectorの**Type**ドロップダウンで方式を選択すると、関連するフィールドのみ表示されます。

### 共通フィールド

| フィールド | デフォルト値 | 説明 |
|---|---|---|
| `Type` | Straight | 発射体の移動方式 |
| `MoveSpeed` | 10 | 移動速度 (Units/sec) |

### Type別フィールド

| Type | フィールド | デフォルト値 | 説明 |
|---|---|---|---|
| **Straight** | `Direction` | Vector2.right | 初期移動方向 |
| **Homing** | `Target` | null | 追跡対象のTransform |
| **Homing** | `TurnSpeed` | 180 | 秒間最大旋回角度（度） |

```csharp
// 発射方向設定 (Straight・Homing共通)
projectile.Launch(Vector2.right);

// 誘導ターゲット設定 / 解除 (Homing)
projectile.SetTarget(enemy.transform);
projectile.ClearTarget();
```

> ターゲットを失った**Homing**発射体は、最後の進行方向に直進し続けます。

## トリガー/衝突イベント

Rigidbody2Dがないため、このGameObjectから直接`OnCollisionEnter2D`は発生しません。
`OnTriggerEnter2D`は**相手側のトリガー**にRigidbody2Dがあれば正常に動作します（多くのトリガーゾーンはKinematic Rigidbody2D + Triggerで構成すれば問題ありません）。
