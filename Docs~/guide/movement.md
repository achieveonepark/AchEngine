# AchMover

`AchMover`는 붙이기만 하면 바로 동작하는 2D 캐릭터 이동 컴포넌트입니다.
`Rigidbody2D`와 `CapsuleCollider2D`를 자동으로 추가·초기화하며, 중력·이동·지면 감지를 모두 직접 제어합니다.

## 사용법

캐릭터 GameObject에 **Ach Mover** 컴포넌트를 추가하면 끝입니다.

| 모드 | 조작 |
|---|---|
| **Platformer** | `A` / `D` 좌우 이동, `Space` / `W` / `↑` 점프 |
| **TopDown** | `WASD` / 방향키 상하좌우 이동 |

> **New Input System** 완전 지원 — Player Settings에서 Input System Package로 전환해도 자동으로 `Keyboard.current` / `Gamepad.current` 경로로 동작합니다.

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
| `UseGravity` | true | 중력·지면 감지 활성 여부 — `false`로 끄면 TopDown·비행 등 어떤 모드든 중력 없이 동작 |
| `GravityScale` | 3 | 중력 배율 (`UseGravity = true` 전용) |
| `FallMultiplier` | 2 | 낙하 시 추가 중력 배율 — 높을수록 묵직한 느낌 |
| `MaxFallSpeed` | 20 | 최대 낙하 속도 |

> `Rigidbody2D.gravityScale`은 항상 0으로 고정됩니다. 중력은 AchMover가 직접 적용하므로 물리 엔진 기본 중력과 충돌하지 않습니다.

### Control

| 필드 | 기본값 | 설명 |
|---|---|---|
| `Movable` | true | `false`이면 입력 차단, 코드로만 제어 가능 |
| `FlipSprite` | true | 이동 방향에 따라 `SpriteRenderer` 자동 반전 |

## 상태 프로퍼티

```csharp
bool    isGrounded = mover.IsGrounded;  // 지면 접지 여부 (UseGravity = true 전용)
bool    isMoving   = mover.IsMoving;    // 현재 이동 중 여부 (입력 기준)
Vector2 velocity   = mover.Velocity;    // 현재 Rigidbody2D 속도
```

## 조이스틱 / 커스텀 입력 연결

`InputProvider` 델리게이트를 설정하면 기본 키보드·게임패드 대신 외부 입력 소스를 사용합니다.

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
mover.AddForce(Vector2.left * 10f);             // 힘 적용
mover.Stop();                                   // 즉시 정지
```

### 넉백 예시

```csharp
mover.Movable = false;
mover.SetVelocity(new Vector2(-6f, 5f));
await Task.Delay(400);
mover.Movable = true;
```

## UseGravity 조합 예시

| Mode | UseGravity | 동작 |
|---|---|---|
| Platformer | true | 중력 + 점프, 좌우 이동 |
| Platformer | false | 중력 없음, 좌우 이동만 |
| TopDown | false | 중력 없음, 4방향 자유 이동 |
| TopDown | true | 4방향 이동 + 중력 (특수한 경우) |

## A* 길찾기와 함께 쓰기

`AStarPathfinder`로 구한 경로를 AchMover로 따라가게 만들 수 있습니다.
TopDown 모드와 잘 어울립니다.

```csharp
var path = AStarPathfinder.FindPath(baker.Grid, startCell, endCell, diagonal: true);

mover.Movable = false;
foreach (var cell in path)
{
    Vector2 target = baker.CellToWorld(cell);
    while (Vector2.Distance(transform.position, target) > 0.05f)
    {
        Vector2 dir = ((Vector2)target - (Vector2)transform.position).normalized;
        mover.SetVelocity(dir * mover.MoveSpeed);
        await Task.Yield();
    }
}
mover.Stop();
mover.Movable = true;
```

> 자세한 내용은 [A\* 길찾기](./pathfinding) 문서를 참고하세요.
