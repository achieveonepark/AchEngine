# A* Pathfinding

`AStarPathfinder` uses the A* algorithm to find the shortest path between two points on a grid.
It supports 4-directional and diagonal (8-directional) movement with a binary min-heap for efficient traversal.

## Quick Start

### 1. Auto-scan the scene with AStarGridBaker

A component that automatically builds a grid by scanning `Collider2D` objects in the scene.

1. Add the **A\* Grid Baker** component to an empty GameObject
2. Set the grid size and obstacle layer in the Inspector
3. Verify the result in the Scene view — **green (walkable) / red (blocked)**

```csharp
var baker = GetComponent<AStarGridBaker>();

// World position → grid coordinate
Vector2Int start = baker.WorldToCell(player.position);
Vector2Int end   = baker.WorldToCell(targetPosition);

// Find path (diagonal movement enabled)
var path = AStarPathfinder.FindPath(baker.Grid, start, end, diagonal: true);

// Grid coordinate → world position
foreach (var cell in path)
{
    Vector3 worldPos = baker.CellToWorld(cell);
    // move logic... (see the [AchMover integration] section below)
}
```

> Returns an empty `List<Vector2Int>` when no path exists.

### AchMover integration

You can drive [`AchMover`](./movement) along the path with `Move()`:

```csharp
mover.Movable = false;
foreach (var cell in path)
{
    Vector2 target = baker.CellToWorld(cell);
    while (Vector2.Distance(transform.position, target) > 0.05f)
    {
        mover.Move(((Vector2)target - (Vector2)transform.position).normalized);
        await Task.Yield();
    }
}
mover.Stop();
mover.Movable = true;
```

### 2. Build a grid manually

```csharp
var grid = new AStarGrid(20, 20);

// Set obstacle
grid.SetWalkable(5, 3, false);

// High-cost terrain (swamp, sand, etc.)
grid.SetCost(4, 4, 3f);

// Find path
var path = AStarPathfinder.FindPath(
    grid,
    start:    new Vector2Int(0,  0),
    end:      new Vector2Int(19, 19),
    diagonal: false);
```

## AStarGridBaker Inspector

| Field | Description |
|---|---|
| `Width` / `Height` | Number of grid cells |
| `CellSize` | Size of one cell in Unity units |
| `Origin` | Transform for the grid's bottom-left origin (defaults to the GameObject's position) |
| `ObstacleLayer` | Layer mask used to detect obstacles |
| `DetectionRadius` | Obstacle detection radius per cell (`cellSize * 0.4` recommended) |
| `CostZones` | Array of Collider2D + cost value pairs for terrain weighting |
| `DrawGizmos` | Toggle Scene view grid visualization |

### Cost Zones

Raise the movement cost in specific areas to guide paths around them.

```csharp
// Set via Inspector, or rescan after dynamic changes
baker.Bake();
```

| Scenario | Approach |
|---|---|
| Walls with `Collider2D` | Set ObstacleLayer on `AStarGridBaker` |
| Unity Tilemap | Attach `TilemapCollider2D` on the obstacle layer |
| Dynamic obstacles (doors, etc.) | Call `baker.Bake()` after the change |

## AStarGrid API

```csharp
var grid = new AStarGrid(width: 20, height: 20);

// Initialize from a bool array
var grid2 = new AStarGrid(walkableMap: myBoolArray);

grid.SetWalkable(x, y, false);    // mark as obstacle
grid.SetCost(x, y, 3f);           // movement cost (minimum 1)
bool  ok      = grid.IsWalkable(x, y);
float cost    = grid.GetCost(x, y);
bool  inRange = grid.IsInBounds(x, y);
```

## AStarPathfinder API

```csharp
List<Vector2Int> path = AStarPathfinder.FindPath(
    grid:     grid,
    start:    new Vector2Int(0, 0),
    end:      new Vector2Int(10, 10),
    diagonal: true   // false = 4-way, true = 8-way
);
```

| Parameter | Description |
|---|---|
| `grid` | The `AStarGrid` to search |
| `start` | Start grid coordinate |
| `end` | Target grid coordinate |
| `diagonal` | Allow diagonal movement (default `false`) |

**Returns**: `List<Vector2Int>` from the first step after start to end (inclusive), in movement order. Empty list if no path exists.

## Algorithm Details

| Item | Detail |
|---|---|
| Search | A* with binary min-heap open set |
| Heuristic | Manhattan (4-way) / Chebyshev (8-way) |
| Diagonal cost | √2 ≈ 1.414 |
| Cell weighting | Per-cell cost via `AStarGrid.SetCost()` |
| Complexity | O((W×H) log(W×H)) |

> ⚠️ **Corner cutting**: With diagonals enabled, the path may slip through a diagonally open cell even when both adjacent orthogonal cells are blocked.
> Disable diagonal movement or add a custom neighbor check if this matters for your game.
