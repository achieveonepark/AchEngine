# AchMover

`AchMover` 是一个挂载后即可立即生效的 2D 角色移动组件。
**它不使用 Rigidbody2D** —— 自身的碰撞处理(Move-and-Slide)、重力、斜坡、台阶和地面吸附全部由它直接完成。
所需的组件仅有一个 `CapsuleCollider2D`,并且会被自动添加和管理。

## 使用方法

只需在角色 GameObject 上添加 **Ach Mover** 组件即可。

| 模式 | 操作 |
|---|---|
| **Platformer** | `A` / `D` 左右移动,`Space` / `W` / `↑` 跳跃 |
| **TopDown** | `WASD` / 方向键 上下左右移动 |

> 完全支持 **New Input System** —— 即使在 Player Settings 中切换为 Input System Package,也会自动通过 `Keyboard.current` / `Gamepad.current` 路径工作。

> 如果同一 GameObject 上存在 `Rigidbody2D`,它会与 transform 移动产生冲突,因此会被自动设置为 `simulated = false`。

## Inspector

### Movement

| 字段 | 默认值 | 说明 |
|---|---|---|
| `MoveSpeed` | 5 | 移动速度 (Units/sec) |
| `JumpForce` | 12 | 跳跃力(仅 `UseGravity = true` 时有效) |
| `Mode` | Platformer | `Platformer` / `TopDown` |

### Physics

| 字段 | 默认值 | 说明 |
|---|---|---|
| `UseGravity` | true | 是否启用重力和地面检测 |
| `GravityScale` | 3 | 重力倍率 |
| `FallMultiplier` | 2 | 下落时的额外重力倍率 —— 数值越高,手感越沉重 |
| `MaxFallSpeed` | 20 | 最大下落速度 |

### Slopes & Stairs

| 字段 | 默认值 | 说明 |
|---|---|---|
| `MaxSlopeAngle` | 50° | 可攀爬的最大斜坡角度。超过该角度的表面会被视为墙壁,角色会沿其滑落 |
| `StepHeight` | 0.3 | 可自动登上的最大台阶/坎高度 (Units) |

### Control

| 字段 | 默认值 | 说明 |
|---|---|---|
| `Movable` | true | 若为 `false` 则屏蔽输入,只能通过代码控制 |
| `FlipSprite` | true | 根据移动方向翻转 `transform.localScale.x` —— 子对象(武器、特效、命中框等)也会一同翻转 |

## 自有碰撞系统

AchMover 在每次 `FixedUpdate` 时执行以下流程。

1. **速度计算** —— 综合输入、重力、跳跃,确定本帧的速度
2. **Move-and-Slide** —— 使用 `CapsuleCollider2D.Cast` 检测前进方向,然后沿碰撞表面滑动。最多迭代 4 次以处理拐角和多重表面
3. **台阶自动攀登** —— 被墙壁挡住时,若坎高在 `StepHeight` 以内则自动登上(向上抬起 → 横向移动 → 再向下落回)
4. **地面检测** —— 向碰撞体下方进行胶囊投射,若表面角度在 `MaxSlopeAngle` 以下则视为 grounded
5. **地面吸附** —— 下斜坡时重新贴合表面,以免脱离地面
6. **穿透修正** —— 最后检查残余重叠并将其推开

## 状态属性

```csharp
bool    isGrounded   = mover.IsGrounded;    // 지면 접지 여부 (UseGravity = true 전용)
bool    isMoving     = mover.IsMoving;      // 현재 이동 중 여부 (입력 기준)
Vector2 velocity     = mover.Velocity;      // 현재 속도
Vector2 groundNormal = mover.GroundNormal;  // 발 밑 표면의 법선 (없으면 Vector2.up)
```

## 连接摇杆 / 自定义输入

```csharp
// 온스크린 조이스틱 연결
mover.InputProvider = () => joystick.Direction;

// 해제 (키보드로 복귀)
mover.InputProvider = null;
```

## 代码控制 API

无论 `Movable` 取值如何,均可随时调用。

```csharp
mover.Jump();                                   // 점프 (UseGravity = true 전용)
mover.Teleport(new Vector2(10f, 0f));           // 순간이동
mover.SetVelocity(new Vector2(-5f, 4f));        // 속도 직접 설정 (넉백 등)
mover.AddForce(Vector2.left * 10f);             // Impulse 형태로 속도에 가산
mover.Stop();                                   // 즉시 정지
```

> 由于没有 Rigidbody2D,`AddForce` 始终像 Impulse 一样立即叠加到速度上。`forceMode` 参数为兼容性而保留,但会被忽略。

### 击退示例

```csharp
mover.Movable = false;
mover.SetVelocity(new Vector2(-6f, 5f));
await Task.Delay(400);
mover.Movable = true;
```

## UseGravity 组合

| Mode | UseGravity | 行为 |
|---|---|---|
| Platformer | true | 重力 + 跳跃 + 斜坡 + 台阶,左右移动 |
| Platformer | false | 无重力,仅左右移动(垂直自由) |
| TopDown | false | 无重力,4 方向自由移动 |
| TopDown | true | 4 方向移动 + 自由下落(特殊情况) |

## AchFollower —— AI 追踪

`AchFollower` 是一个朝指定目标移动的独立组件。
它与 `AchMover` 完全无关,可单独用于任何 GameObject。

| 字段 | 默认值 | 说明 |
|---|---|---|
| `Target` | null | 要追随的目标 Transform |
| `MoveSpeed` | 5 | 移动速度 (Units/sec) |
| `StopDistance` | 0.5 | 距离小于该值时停止 |

```csharp
// 대상 설정
follower.SetTarget(player.transform);

// 대상 해제
follower.ClearTarget();
```

它会直接移动 `transform.position`。

需要沿 A* 路径移动的高级寻路,请参阅 [A\* 寻路](./pathfinding) 文档。

## AchProjectile —— 投射物

用单个组件管理直线弹、追踪弹等多种投射物。
在 Inspector 的 **Type** 下拉菜单中选择方式后,只会显示相关字段。

### 通用字段

| 字段 | 默认值 | 说明 |
|---|---|---|
| `Type` | Straight | 投射物移动方式 |
| `MoveSpeed` | 10 | 移动速度 (Units/sec) |

### 各 Type 的字段

| Type | 字段 | 默认值 | 说明 |
|---|---|---|---|
| **Straight** | `Direction` | Vector2.right | 初始移动方向 |
| **Homing** | `Target` | null | 追踪目标 Transform |
| **Homing** | `TurnSpeed` | 180 | 每秒最大转向角度(度) |

```csharp
// 발사 방향 설정 (Straight · Homing 공통)
projectile.Launch(Vector2.right);

// 유도 대상 설정 / 해제 (Homing)
projectile.SetTarget(enemy.transform);
projectile.ClearTarget();
```

> 失去目标的 **Homing** 投射物会沿最后的前进方向直线移动。

## 触发器/碰撞事件

由于没有 Rigidbody2D,该 GameObject 本身不会触发 `OnCollisionEnter2D`。
若**对方触发器**带有 Rigidbody2D,则 `OnTriggerEnter2D` 可正常工作(大多数触发区域用 Kinematic Rigidbody2D + Trigger 配置即可)。
