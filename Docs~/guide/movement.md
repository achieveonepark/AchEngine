# AchMover

`AchMover`는 붙이기만 하면 바로 동작하는 2D 캐릭터 이동 컴포넌트입니다.
`Rigidbody2D`를 자동으로 설정하며, 플랫포머와 탑다운 두 가지 모드를 지원합니다.

## 빠른 시작

캐릭터 GameObject에 **Ach Mover** 컴포넌트를 추가하고 `GroundLayer`를 설정하면 끝입니다.

| 모드 | 조작 |
|---|---|
| **Platformer** | `A` / `D` 좌우 이동, `Space` / `W` / `↑` 점프 |
| **TopDown** | `WASD` / 방향키 상하좌우 이동 |

## Inspector 설정

### Movement

| 필드 | 기본값 | 설명 |
|---|---|---|
| `MoveSpeed` | 5 | 이동 속도 (Units/sec) |
| `JumpForce` | 12 | 점프 힘 (Platformer 전용) |
| `Mode` | Platformer | `Platformer` / `TopDown` |

### Physics

| 필드 | 기본값 | 설명 |
|---|---|---|
| `GravityScale` | 3 | 중력 배율 (Platformer 전용) |
| `FallMultiplier` | 2 | 낙하 시 추가 중력 배율 — 높을수록 낙하가 빠름 |
| `MaxFallSpeed` | 20 | 최대 낙하 속도 제한 |

### Ground Check (Platformer)

| 필드 | 설명 |
|---|---|
| `GroundLayer` | 지면으로 판단할 레이어 마스크 |
| `GroundCheckPoint` | 발 위치 Transform (비우면 Collider 하단 자동 계산) |
| `GroundCheckRadius` | 지면 감지 반경 (Scene 뷰 Gizmo로 확인 가능) |

### Control

| 필드 | 기본값 | 설명 |
|---|---|---|
| `Movable` | true | `false`이면 입력 차단 — 코드로만 제어 가능 |
| `FlipSprite` | true | 이동 방향에 따라 스프라이트 자동 반전 |

## Movable = false — 코드 제어

`Movable`을 `false`로 설정하면 플레이어 입력이 차단됩니다.
아래 메서드로 코드에서 직접 움직일 수 있습니다.

```csharp
var mover = GetComponent<AchMover>();
mover.Movable = false;

// 이동 방향 설정 (FixedUpdate 1프레임 적용)
mover.Move(Vector2.right);

// 점프 (Platformer 전용)
mover.Jump();

// 즉시 위치 이동
mover.Teleport(new Vector2(10f, 0f));

// 속도 직접 설정 (넉백 등)
mover.SetVelocity(new Vector2(8f, 5f));

// 힘 적용
mover.AddForce(Vector2.left * 10f, ForceMode2D.Impulse);

// 즉시 정지
mover.Stop();
```

### 활용 예시

```csharp
// 컷씬 중 캐릭터를 목표 지점으로 이동
mover.Movable = false;
while (Vector2.Distance(transform.position, target) > 0.1f)
{
    var dir = (target - (Vector2)transform.position).normalized;
    mover.Move(dir);
    await AchTask.Yield();
}
mover.Stop();
mover.Movable = true;

// 피격 시 넉백
mover.Movable = false;
mover.SetVelocity(new Vector2(-5f, 4f));
await AchTask.Delay(500);
mover.Movable = true;
```

## 상태 프로퍼티

```csharp
bool  grounded  = mover.IsGrounded;   // 지면 접지 여부 (Platformer)
Vector2 vel     = mover.Velocity;     // 현재 Rigidbody2D 속도
```

## 씬 설정 체크리스트

- [ ] 캐릭터 GameObject에 `Collider2D` 추가 (CapsuleCollider2D 권장)
- [ ] 지면 오브젝트를 별도 레이어(예: "Ground")로 설정
- [ ] `AchMover.GroundLayer`에 해당 레이어 선택
- [ ] Scene 뷰 선택 시 하늘색 Gizmo 원이 발에 맞게 위치하는지 확인
- [ ] `GroundCheckPoint`가 필요하면 발 위치에 빈 Transform 자식 추가 후 할당

> `Rigidbody2D`는 `AchMover`가 자동으로 추가합니다. 별도로 추가할 필요가 없습니다.
