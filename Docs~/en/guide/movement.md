# AchMover

`AchMover` is a 2D character movement component that works the moment you attach it.
It auto-configures `Rigidbody2D` and supports both Platformer and Top-Down modes.

## Quick Start

Add the **Ach Mover** component to your character GameObject and set `GroundLayer`. That's it.

| Mode | Controls |
|---|---|
| **Platformer** | `A` / `D` to move, `Space` / `W` / `↑` to jump |
| **TopDown** | `WASD` / arrow keys to move in all directions |

## Inspector Reference

### Movement

| Field | Default | Description |
|---|---|---|
| `MoveSpeed` | 5 | Movement speed in Units/sec |
| `JumpForce` | 12 | Jump impulse force (Platformer only) |
| `Mode` | Platformer | `Platformer` or `TopDown` |

### Physics

| Field | Default | Description |
|---|---|---|
| `GravityScale` | 3 | Gravity multiplier (Platformer only) |
| `FallMultiplier` | 2 | Extra gravity while falling — higher = snappier feel |
| `MaxFallSpeed` | 20 | Terminal fall speed cap |

### Ground Check (Platformer)

| Field | Description |
|---|---|
| `GroundLayer` | Layer mask treated as ground |
| `GroundCheckPoint` | Foot position Transform (auto-calculates from Collider bottom if empty) |
| `GroundCheckRadius` | Detection radius (visible as a cyan Gizmo in Scene view) |

### Control

| Field | Default | Description |
|---|---|---|
| `Movable` | true | `false` disables input — control via code only |
| `FlipSprite` | true | Auto-flips SpriteRenderer based on movement direction |

## Movable = false — Code Control

Setting `Movable = false` disables all player input.
Use the following methods to move the character from code:

```csharp
var mover = GetComponent<AchMover>();
mover.Movable = false;

// Set movement direction (applied for one FixedUpdate frame)
mover.Move(Vector2.right);

// Jump (Platformer only)
mover.Jump();

// Teleport instantly
mover.Teleport(new Vector2(10f, 0f));

// Override velocity directly (knockback, etc.)
mover.SetVelocity(new Vector2(8f, 5f));

// Apply a physics force
mover.AddForce(Vector2.left * 10f, ForceMode2D.Impulse);

// Stop immediately
mover.Stop();
```

### Usage Examples

```csharp
// Move character to a target during a cutscene
mover.Movable = false;
while (Vector2.Distance(transform.position, target) > 0.1f)
{
    var dir = (target - (Vector2)transform.position).normalized;
    mover.Move(dir);
    await AchTask.Yield();
}
mover.Stop();
mover.Movable = true;

// Knockback on hit
mover.Movable = false;
mover.SetVelocity(new Vector2(-5f, 4f));
await AchTask.Delay(500);
mover.Movable = true;
```

## State Properties

```csharp
bool    grounded = mover.IsGrounded;  // Whether the character is on the ground (Platformer)
Vector2 vel      = mover.Velocity;   // Current Rigidbody2D velocity
```

## Scene Setup Checklist

- [ ] Add a `Collider2D` to the character (CapsuleCollider2D recommended)
- [ ] Assign your ground objects to a dedicated layer (e.g. "Ground")
- [ ] Set `AchMover.GroundLayer` to that layer
- [ ] In Scene view, confirm the cyan Gizmo circle sits at the character's feet
- [ ] If needed, add an empty child Transform at foot position and assign it to `GroundCheckPoint`

> `Rigidbody2D` is added automatically by `AchMover`. You do not need to add it manually.
