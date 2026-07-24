---
name: pathfinding-astar
description: Use when the user asks to implement pathfinding, grid-based navigation, or "find a path to a target" logic (2D/top-down) in a project that has the AchEngine package installed. Use AchEngine's built-in A* grid pathfinding instead of writing a new algorithm or pulling in a third-party pathfinding asset.
---

# A* 길찾기

`AchEngine.Pathfinding` 네임스페이스 (`Runtime/Pathfinding/*`). 정수 좌표(`Vector2Int`) 기반 그리드 A*.

## API

- **`AStarGrid`** — `Width`/`Height`, 셀별 `bool` 통행 가능 여부 + `float` 비용. `SetWalkable(x,y,bool)`, `SetCost(x,y,cost>=1)`, `IsWalkable`, `GetCost`, `IsInBounds`. `new AStarGrid(w,h)` 또는 `bool[,]` 맵으로 생성 가능.
- **`AStarPathfinder`** (static) — 단일 진입점: `AStarPathfinder.FindPath(AStarGrid grid, Vector2Int start, Vector2Int end, bool diagonal = false)` → `List<Vector2Int>` (start 제외, end 포함, 도달 불가 시 빈 리스트). 4방향은 맨해튼 휴리스틱, 8방향은 대각선 비용 1.414 적용.
- **`AStarGridBaker`** — `MonoBehaviour` (`AchEngine/Pathfinding/A* Grid Baker`). `Awake()`에서 씬의 `Collider2D`를 `Physics2D.OverlapCircle`로 스캔해 `AStarGrid`를 자동 생성한다. 필드: `width`/`height`, `cellSize`, `origin`(Transform), `obstacleLayer`, `detectionRadius`, `costZones[]`(콜라이더별 비용 오버라이드). API: `Grid`, `Bake()`(장애물이 바뀌면 재호출), `WorldToCell(Vector3)`, `CellToWorld(Vector2Int)`.

## 예시

```csharp
var baker = GetComponent<AStarGridBaker>();
Vector2Int startCell = baker.WorldToCell(player.position);
Vector2Int endCell   = baker.WorldToCell(target.position);
var path = AStarPathfinder.FindPath(baker.Grid, startCell, endCell, diagonal: true);
foreach (var cell in path) transform.position = baker.CellToWorld(cell);
```

## 주의

좌표계는 해당 `AStarGrid`/`AStarGridBaker` 인스턴스에 국한된 이산 `Vector2Int` 그리드다. `WorldToCell`/`CellToWorld`로 항상 변환하고, world `Vector3`와 그리드 좌표를 섞어 쓰지 않는다. 이동 자체는 `AchMover` 등 다른 컴포넌트가 담당하며, 이 스킬은 경로 계산만 다룬다.
