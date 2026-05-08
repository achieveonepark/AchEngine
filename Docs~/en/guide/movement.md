# AchMover

`AchMover` is a 2D character movement component that works the moment you attach it.
`Rigidbody2D` and `CapsuleCollider2D` are added and fully initialized automatically — gravity, movement, and ground detection are all managed internally.

## Usage

Add the **Ach Mover** component to your character GameObject. That's it.

| Mode | Controls |
|---|---|
| **Platformer** | `A` / `D` to move, `Space` / `W` / `↑` to jump |
| **TopDown** | `WASD` / arrow keys to move in all directions |

> **New Input System supported** — switching Player Settings to Input System Package automatically routes input through `Keyboard.current` / `Gamepad.current`.

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
| `UseGravity` | true | Enables gravity and ground detection — set to `false` for TopDown, flying, or zero-gravity gameplay |
| `GravityScale` | 3 | Gravity multiplier (`UseGravity = true` only) |
| `FallMultiplier` | 2 | Extra gravity while falling — higher = heavier feel |
| `MaxFallSpeed` | 20 | Terminal fall speed cap |

> `Rigidbody2D.gravityScale` is always forced to 0. AchMover applies gravity directly each `FixedUpdate`, so there is no double-gravity conflict with the physics engine.

### Control

| Field | Default | Description |
|---|---|---|
| `Movable` | true | `false` disables input — control via code only |
| `FlipSprite` | true | Auto-flips `SpriteRenderer` based on movement direction |

## State Properties

```csharp
bool    isGrounded = mover.IsGrounded;  // On the ground? (UseGravity = true only)
bool    isMoving   = mover.IsMoving;    // Actively moving? (based on input)
Vector2 velocity   = mover.Velocity;    // Current Rigidbody2D velocity
```

## Joystick / Custom Input

Set `InputProvider` to route input from an external source instead of the built-in keyboard/gamepad.

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
mover.AddForce(Vector2.left * 10f);             // Apply a physics force
mover.Stop();                                   // Stop immediately
```

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
| Platformer | true | Gravity + jump, horizontal movement |
| Platformer | false | No gravity, horizontal movement only |
| TopDown | false | No gravity, free 4-direction movement |
| TopDown | true | 4-direction movement + gravity (special cases) |

## Combining with A* Pathfinding

Drive `AchMover` along a path produced by `AStarPathfinder` — a natural fit for TopDown mode.

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

> See the [A\* Pathfinding](./pathfinding) guide for details.
