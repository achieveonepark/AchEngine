# AchMover

`AchMover`는 붙이기만 하면 바로 동작하는 2D 캐릭터 이동 컴포넌트입니다.
`Rigidbody2D`와 `CapsuleCollider2D`를 자동으로 추가·설정하며, 레이어나 발 위치 등 별도 설정이 필요 없습니다.

## 사용법

캐릭터 GameObject에 **Ach Mover** 컴포넌트를 추가하면 끝입니다.

| 모드 | 조작 |
|---|---|
| **Platformer** | `A` / `D` 좌우 이동, `Space` / `W` / `↑` 점프 |
| **TopDown** | `WASD` / 방향키 상하좌우 이동 |

> 지면 감지는 콜라이더 발끝에서 아래로 짧은 `Physics2D.Raycast`를 쏘아 자동 처리됩니다.
> 자기 자신 레이어는 자동으로 제외되므로 별도 레이어 설정이 필요 없습니다.

## Inspector

### Movement

| 필드 | 기본값 | 설명 |
|---|---|---|
| `MoveSpeed` | 5 | 이동 속도 (Units/sec) |
| `JumpForce` | 12 | 점프 힘 (Platformer 전용) |
| `Mode` | Platformer | `Platformer` / `TopDown` |

### Physics (Platformer)

| 필드 | 기본값 | 설명 |
|---|---|---|
| `GravityScale` | 3 | 중력 배율 |
| `FallMultiplier` | 2 | 낙하 시 추가 중력 — 높을수록 묵직한 느낌 |
| `MaxFallSpeed` | 20 | 최대 낙하 속도 |

### Control

| 필드 | 기본값 | 설명 |
|---|---|---|
| `Movable` | true | `false`이면 입력 차단, 코드로만 제어 가능 |
| `FlipSprite` | true | 이동 방향에 따라 `SpriteRenderer` 자동 반전 |

## Movable = false — 코드 제어

`Movable = false`로 설정하면 입력이 차단됩니다. 아래 메서드로 코드에서 직접 조작합니다.

```csharp
var mover = GetComponent<AchMover>();
mover.Movable = false;

mover.Move(Vector2.right);                      // 이동 방향 설정
mover.Jump();                                   // 점프 (Platformer)
mover.Teleport(new Vector2(10f, 0f));           // 순간이동
mover.SetVelocity(new Vector2(-5f, 4f));        // 속도 직접 설정 (넉백 등)
mover.AddForce(Vector2.left * 10f);             // 힘 적용
mover.Stop();                                   // 즉시 정지
```

### 활용 예시

```csharp
// 컷씬: 목표 지점까지 자동 이동
mover.Movable = false;
while (Vector2.Distance(transform.position, target) > 0.1f)
{
    mover.Move((target - (Vector2)transform.position).normalized);
    await Task.Yield();
}
mover.Stop();
mover.Movable = true;

// 피격 넉백
mover.Movable = false;
mover.SetVelocity(new Vector2(-6f, 5f));
await Task.Delay(400);
mover.Movable = true;
```

## 상태 프로퍼티

```csharp
bool    grounded = mover.IsGrounded;  // 지면 접지 여부 (Platformer)
Vector2 vel      = mover.Velocity;    // 현재 속도
```

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
        mover.Move(dir);
        await Task.Yield();
    }
}
mover.Stop();
mover.Movable = true;
```

> 자세한 내용은 [A\* 길찾기](./pathfinding) 문서를 참고하세요.
