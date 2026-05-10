# AchMover

`AchMover` is a 2D character movement component that works the moment you attach it.
**It does not use Rigidbody2D** — collision resolution (move-and-slide), gravity, slopes, stairs, and ground snapping are all handled internally.
The only required component is `CapsuleCollider2D`, which is added automatically.

## Usage

Add the **Ach Mover** component to your character GameObject. That's it.

| Mode | Controls |
|---|---|
| **Platformer** | `A` / `D` to move, `Space` / `W` / `↑` to jump |
| **TopDown** | `WASD` / arrow keys to move in all directions |

> **New Input System supported** — switching Player Settings to Input System Package automatically routes input through `Keyboard.current` / `Gamepad.current`.

> If a `Rigidbody2D` exists on the same GameObject, AchMover sets `simulated = false` automatically to prevent it from fighting transform-based movement.

## Inspector

### Movement

| Field | Default | Description |
|---|---|---|
| `MoveSpeed` | 5 | Movement speed in Units/sec |
| `JumpForce` | 12 | Jump impulse force (`UseGravity = true` only) |
| `Mode` | Platformer | `Platformer` or `TopDown` |

### Physics

| Field | Default | Description |
|---|---|---|
| `UseGravity` | true | Enables gravity and ground detection |
| `GravityScale` | 3 | Gravity multiplier |
| `FallMultiplier` | 2 | Extra gravity while falling — higher = heavier feel |
| `MaxFallSpeed` | 20 | Terminal fall speed cap |

### Slopes & Stairs

| Field | Default | Description |
|---|---|---|
| `MaxSlopeAngle` | 50° | Maximum walkable slope angle. Anything steeper is treated as a wall |
| `StepHeight` | 0.3 | Maximum step / ledge height the character will auto-climb (Units) |

### Control

| Field | Default | Description |
|---|---|---|
| `Movable` | true | `false` disables input — control via code only |
| `FlipSprite` | true | Mirrors `transform.localScale.x` based on movement direction — children (weapons, effects, hitboxes) flip together |

## Built-in Collision System

Each `FixedUpdate`, AchMover runs the following pipeline:

1. **Velocity** — combines input, gravity, and queued jumps into the per-frame velocity.
2. **Move-and-slide** — `CapsuleCollider2D.Cast` along the motion direction; on hit, slides along the surface. Up to 4 iterations to handle corners and multiple contacts.
3. **Stair stepping** — when blocked by a wall, attempts to lift over a ledge ≤ `StepHeight` (lift up → move forward → drop down).
4. **Ground probe** — capsule cast downward; grounded if surface angle ≤ `MaxSlopeAngle`.
5. **Ground snap** — pulls the character back to the slope surface when descending so it stays glued.
6. **Depenetration** — a final overlap check pushes out of any residual penetration.

## State Properties

```csharp
bool    isGrounded   = mover.IsGrounded;    // On the ground? (UseGravity = true only)
bool    isMoving     = mover.IsMoving;      // Actively moving? (based on input)
Vector2 velocity     = mover.Velocity;      // Current velocity
Vector2 groundNormal = mover.GroundNormal;  // Surface normal under the feet (Vector2.up if airborne)
```

## Joystick / Custom Input

```csharp
// Connect an on-screen joystick
mover.InputProvider = () => joystick.Direction;

// Disconnect (returns to keyboard input)
mover.InputProvider = null;
```

## Code Control API

These methods work regardless of the `Movable` flag.

```csharp
mover.Jump();                                   // Jump (UseGravity = true only)
mover.Teleport(new Vector2(10f, 0f));           // Instant position warp
mover.SetVelocity(new Vector2(-5f, 4f));        // Override velocity (knockback, etc.)
mover.AddForce(Vector2.left * 10f);             // Add to velocity as an impulse
mover.Stop();                                   // Stop immediately
```

> Without a Rigidbody2D, `AddForce` always behaves like an impulse (added directly to velocity). The `forceMode` argument is kept for source compatibility but is ignored.

### Knockback example

```csharp
mover.Movable = false;
mover.SetVelocity(new Vector2(-6f, 5f));
await Task.Delay(400);
mover.Movable = true;
```

## UseGravity combinations

| Mode | UseGravity | Behaviour |
|---|---|---|
| Platformer | true | Gravity + jump + slopes + stairs, horizontal movement |
| Platformer | false | No gravity, horizontal movement only (vertical free) |
| TopDown | false | No gravity, free 4-direction movement |
| TopDown | true | 4-direction movement + free fall (special cases) |

## AchFollower — AI Chasing

`AchFollower` is a standalone component that chases a target.
It has no relation to `AchMover` and can be used on any GameObject independently.

| Field | Default | Description |
|---|---|---|
| `Target` | null | The Transform to follow |
| `MoveSpeed` | 5 | Movement speed (Units/sec) |
| `StopDistance` | 0.5 | Stops moving when closer than this distance |

```csharp
// Set target
follower.SetTarget(player.transform);

// Clear target
follower.ClearTarget();
```

Moves by setting `transform.position` directly.

For advanced movement along an A* path, see the [A\* Pathfinding](./pathfinding) guide.

## AchProjectile — Projectiles

Manages all projectile types (straight, homing, and more) in a single component.
Select the **Type** in the Inspector — only the relevant fields are shown.

### Common Fields

| Field | Default | Description |
|---|---|---|
| `Type` | Straight | Projectile movement mode |
| `MoveSpeed` | 10 | Movement speed (Units/sec) |

### Per-Type Fields

| Type | Field | Default | Description |
|---|---|---|---|
| **Straight** | `Direction` | Vector2.right | Initial movement direction |
| **Homing** | `Target` | null | Transform to track |
| **Homing** | `TurnSpeed` | 180 | Max turn angle per second (degrees) |

```csharp
// Set launch direction (Straight & Homing)
projectile.Launch(Vector2.right);

// Set / clear tracking target (Homing)
projectile.SetTarget(enemy.transform);
projectile.ClearTarget();
```

> A **Homing** projectile that loses its target continues in its last direction.

## Trigger / Collision Events

Because there's no Rigidbody2D, this GameObject does not raise `OnCollisionEnter2D` directly.
`OnTriggerEnter2D` still fires when the **other** trigger has a Rigidbody2D — set up trigger zones as Kinematic Rigidbody2D + Trigger Collider and they'll work as expected.
