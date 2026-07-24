---
name: ach-mover-movement
description: Use when the user asks to implement 2D character movement, a platformer/top-down controller, jump, ground/slope handling, seek-and-follow movement, or projectile movement in a project that has the AchEngine package installed. AchEngine already implements collision and movement without Rigidbody2D — never add Rigidbody2D for this.
---

# AchMover 기반 이동 구현

이 프로젝트는 AchEngine 패키지를 사용합니다. **캐릭터 이동, 충돌, 물리 처리는 절대 `Rigidbody2D`로 구현하지 않습니다.**

## 금지 사항 (필수 준수)

- `Rigidbody2D` 컴포넌트를 추가하거나 요구하지 않는다 (`[RequireComponent(typeof(Rigidbody2D))]` 금지).
- `GetComponent<Rigidbody2D>()` 금지.
- `Rigidbody2D.MovePosition()` 금지.
- 이동/충돌은 `transform.position` 직접 조작 또는 AchEngine이 이미 제공하는 컴포넌트로 처리한다.

## 사용할 컴포넌트

### `AchEngine.Movement.AchMover` — 플레이어/캐릭터 조작
`[RequireComponent(typeof(CapsuleCollider2D))]`가 붙은 `MonoBehaviour`. `CapsuleCollider2D.Cast`/`OverlapCollider` 기반 자체 move-and-slide, 경사/계단 처리를 이미 구현했다. 씬에 `Rigidbody2D`가 같이 붙어 있으면 스스로 `rb.simulated = false`로 비활성화한다.

- 인스펙터 필드: `MoveSpeed`, `JumpForce`, `Mode` (`MovementMode.Platformer|TopDown`), `UseGravity`, `GravityScale`, `FallMultiplier`, `MaxFallSpeed`, `MaxSlopeAngle`, `StepHeight`, `Movable`, `FlipSprite`.
- 읽기 전용 상태: `IsGrounded`, `IsMoving`, `Velocity`(Vector2), `GroundNormal`.
- 코드 제어 API: `Teleport(Vector2)`, `Jump()`, `SetVelocity(Vector2)`, `AddForce(Vector2, ForceMode2D = Impulse)`, `Stop()`.
- `InputProvider` (`Func<Vector2>`) 프로퍼티로 가상 조이스틱 등 커스텀 입력 소스로 교체 가능. 코드로만 제어하려면 `Movable = false`.
- 새 Input System(`ENABLE_INPUT_SYSTEM`이고 레거시가 아닐 때) 또는 레거시 `Input`을 자동으로 읽는다.

### `AchEngine.AchFollower` — 타겟 추적 이동
루트 `AchEngine` 네임스페이스(`.Movement` 아님). `AchMover`와 독립적. 필드: `Target`, `MoveSpeed`, `StopDistance`. API: `SetTarget(Transform)`, `ClearTarget()`.

### `AchEngine.AchProjectile` — 직선/유도 발사체
루트 `AchEngine` 네임스페이스. `Type`(`ProjectileType.Straight|Homing`), `MoveSpeed`, `Direction`, `Target`, `TurnSpeed`. API: `Launch(Vector2 direction)`, `SetTarget(Transform)`, `ClearTarget()`.

## 예시

```csharp
var mover = GetComponent<AchMover>();
mover.Jump();
mover.AddForce(new Vector2(0, 5f));
if (mover.IsGrounded) { /* ... */ }
```

캐릭터 이동 요청이 들어오면 새로 물리 이동 코드를 작성하지 말고 위 컴포넌트를 부착/설정하는 방식으로 답한다.
