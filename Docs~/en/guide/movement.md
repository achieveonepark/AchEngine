# AchMover

`AchMover` is a 2D character movement component that works the moment you attach it.
`Rigidbody2D` and `CapsuleCollider2D` are added and configured automatically — no layer masks or foot transforms required.

## Usage

Add the **Ach Mover** component to your character GameObject. That's it.

| Mode | Controls |
|---|---|
| **Platformer** | `A` / `D` to move, `Space` / `W` / `↑` to jump |
| **TopDown** | `WASD` / arrow keys to move in all directions |

> Ground detection runs a short `Physics2D.Raycast` downward from the collider's bottom each `FixedUpdate`.
> The mover's own layer is excluded automatically — no layer setup is needed.

## Inspector

### Movement

| Field | Default | Description |
|---|---|---|
| `MoveSpeed` | 5 | Movement speed in Units/sec |
| `JumpForce` | 12 | Jump impulse force (Platformer only) |
| `Mode` | Platformer | `Platformer` or `TopDown` |

### Physics (Platformer)

| Field | Default | Description |
|---|---|---|
| `GravityScale` | 3 | Gravity multiplier |
| `FallMultiplier` | 2 | Extra gravity while falling — higher = heavier feel |
| `MaxFallSpeed` | 20 | Terminal fall speed cap |

### Control

| Field | Default | Description |
|---|---|---|
| `Movable` | true | `false` disables input — control via code only |
| `FlipSprite` | true | Auto-flips `SpriteRenderer` based on movement direction |

## Movable = false — Code Control

Set `Movable = false` to disable player input. Use these methods to drive the character from code:

```csharp
var mover = GetComponent<AchMover>();
mover.Movable = false;

mover.Move(Vector2.right);                      // Set movement direction
mover.Jump();                                   // Jump (Platformer only)
mover.Teleport(new Vector2(10f, 0f));           // Instant teleport
mover.SetVelocity(new Vector2(-5f, 4f));        // Override velocity (knockback, etc.)
mover.AddForce(Vector2.left * 10f);             // Apply a physics force
mover.Stop();                                   // Stop immediately
```

### Examples

```csharp
// Cutscene: auto-walk to a target
mover.Movable = false;
while (Vector2.Distance(transform.position, target) > 0.1f)
{
    mover.Move((target - (Vector2)transform.position).normalized);
    await Task.Yield();
}
mover.Stop();
mover.Movable = true;

// Hit knockback
mover.Movable = false;
mover.SetVelocity(new Vector2(-6f, 5f));
await Task.Delay(400);
mover.Movable = true;
```

## State Properties

```csharp
bool    grounded = mover.IsGrounded;  // Is the character on the ground? (Platformer)
Vector2 vel      = mover.Velocity;   // Current velocity
```

## Combining with A* Pathfinding

You can drive `AchMover` along a path produced by `AStarPathfinder` — a natural fit for TopDown mode.

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

> See the [A\* Pathfinding](./pathfinding) guide for details.
