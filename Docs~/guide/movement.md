# AchMover

`AchMover`는 붙이기만 하면 바로 동작하는 2D 캐릭터 이동 컴포넌트입니다.
**Rigidbody2D를 사용하지 않습니다** — 자체 충돌 처리(Move-and-Slide), 중력, 경사면, 계단, 지면 스냅을 모두 직접 수행합니다.
필요한 컴포넌트는 `CapsuleCollider2D` 하나뿐이며, 자동으로 추가·관리됩니다.

## 사용법

캐릭터 GameObject에 **Ach Mover** 컴포넌트를 추가하면 끝입니다.

| 모드 | 조작 |
|---|---|
| **Platformer** | `A` / `D` 좌우 이동, `Space` / `W` / `↑` 점프 |
| **TopDown** | `WASD` / 방향키 상하좌우 이동 |

> **New Input System** 완전 지원 — Player Settings에서 Input System Package로 전환해도 자동으로 `Keyboard.current` / `Gamepad.current` 경로로 동작합니다.

> 같은 GameObject에 `Rigidbody2D`가 있으면 transform 이동과 충돌하므로 자동으로 `simulated = false` 처리됩니다.

## Inspector

### Movement

| 필드 | 기본값 | 설명 |
|---|---|---|
| `MoveSpeed` | 5 | 이동 속도 (Units/sec) |
| `JumpForce` | 12 | 점프 힘 (`UseGravity = true` 전용) |
| `Mode` | Platformer | `Platformer` / `TopDown` |

### Physics

| 필드 | 기본값 | 설명 |
|---|---|---|
| `UseGravity` | true | 중력·지면 감지 활성 여부 |
| `GravityScale` | 3 | 중력 배율 |
| `FallMultiplier` | 2 | 낙하 시 추가 중력 배율 — 높을수록 묵직한 느낌 |
| `MaxFallSpeed` | 20 | 최대 낙하 속도 |

### Slopes & Stairs

| 필드 | 기본값 | 설명 |
|---|---|---|
| `MaxSlopeAngle` | 50° | 등반 가능한 최대 경사 각도. 이 각도를 넘는 표면은 벽으로 처리되어 미끄러져 내려옴 |
| `StepHeight` | 0.3 | 자동으로 올라갈 수 있는 최대 계단/턱 높이 (Units) |

### Control

| 필드 | 기본값 | 설명 |
|---|---|---|
| `Movable` | true | `false`이면 입력 차단, 코드로만 제어 가능 |
| `FlipSprite` | true | 이동 방향에 따라 `transform.localScale.x`를 반전 — 자식(무기·이펙트·히트박스 등)도 함께 뒤집힘 |

## 자체 충돌 시스템

AchMover는 매 `FixedUpdate` 마다 다음 절차를 수행합니다.

1. **속도 계산** — 입력·중력·점프를 종합해 이번 프레임의 속도 결정
2. **Move-and-Slide** — `CapsuleCollider2D.Cast`로 진행 방향 검사 후, 충돌 표면을 따라 슬라이드. 최대 4회 반복으로 코너·다중 표면 처리
3. **계단 자동 등반** — 벽에 막혔을 때 `StepHeight` 이내의 턱이라면 자동으로 올라감 (위로 들어 올림 → 가로 이동 → 다시 아래로)
4. **지면 검사** — 콜라이더 아래로 캡슐 캐스트, 표면 각도가 `MaxSlopeAngle` 이하이면 grounded
5. **지면 스냅** — 경사면을 내려갈 때 표면에서 떨어지지 않도록 다시 밀착
6. **페네트레이션 보정** — 마지막에 잔여 겹침 확인 후 밀어냄

## 상태 프로퍼티

```csharp
bool    isGrounded   = mover.IsGrounded;    // 지면 접지 여부 (UseGravity = true 전용)
bool    isMoving     = mover.IsMoving;      // 현재 이동 중 여부 (입력 기준)
Vector2 velocity     = mover.Velocity;      // 현재 속도
Vector2 groundNormal = mover.GroundNormal;  // 발 밑 표면의 법선 (없으면 Vector2.up)
```

## 조이스틱 / 커스텀 입력 연결

```csharp
// 온스크린 조이스틱 연결
mover.InputProvider = () => joystick.Direction;

// 해제 (키보드로 복귀)
mover.InputProvider = null;
```

## 코드 제어 API

`Movable`과 무관하게 언제든 호출할 수 있습니다.

```csharp
mover.Jump();                                   // 점프 (UseGravity = true 전용)
mover.Teleport(new Vector2(10f, 0f));           // 순간이동
mover.SetVelocity(new Vector2(-5f, 4f));        // 속도 직접 설정 (넉백 등)
mover.AddForce(Vector2.left * 10f);             // Impulse 형태로 속도에 가산
mover.Stop();                                   // 즉시 정지
```

> Rigidbody2D가 없으므로 `AddForce`는 항상 즉시 속도에 가산되는 Impulse처럼 동작합니다. `forceMode` 인자는 호환성을 위해 남아 있지만 무시됩니다.

### 넉백 예시

```csharp
mover.Movable = false;
mover.SetVelocity(new Vector2(-6f, 5f));
await Task.Delay(400);
mover.Movable = true;
```

## UseGravity 조합

| Mode | UseGravity | 동작 |
|---|---|---|
| Platformer | true | 중력 + 점프 + 경사면 + 계단, 좌우 이동 |
| Platformer | false | 중력 없음, 좌우 이동만 (수직 자유) |
| TopDown | false | 중력 없음, 4방향 자유 이동 |
| TopDown | true | 4방향 이동 + 자유 낙하 (특수한 경우) |

## AchFollower — AI 추적

`AchFollower`를 추가하면 `Target`을 향해 자동으로 따라갑니다.
속도는 같은 GameObject의 `AchMover.MoveSpeed`로 설정합니다.

| 필드 | 기본값 | 설명 |
|---|---|---|
| `Target` | null | 따라갈 대상 Transform (플레이어 등) |
| `StopDistance` | 0.5 | 이 거리 이하이면 정지 |

```csharp
// Inspector 또는 코드로 Target 지정
follower.Target = player.transform;
```

> `AchFollower`는 내부적으로 `Movable = false`로 설정합니다.  
> 키보드·조이스틱 입력이 자동으로 차단되므로 별도 처리가 필요 없습니다.

A* 경로를 따라가는 고급 이동은 [A\* 길찾기](./pathfinding) 문서를 참고하세요.

## 트리거/충돌 이벤트

Rigidbody2D가 없으므로 이 GameObject에서 직접 `OnCollisionEnter2D`는 발생하지 않습니다.
`OnTriggerEnter2D`는 **상대편 트리거**에 Rigidbody2D가 있다면 정상 동작합니다 (대부분의 트리거 존은 Kinematic Rigidbody2D + Trigger로 구성하면 됩니다).
