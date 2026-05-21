# A* 寻路

`AStarPathfinder` 使用基于网格的 A* 算法搜索两点之间的最短路径。
支持 4 方向及对角线(8 方向)移动,并通过二叉最小堆实现高效搜索。

## 快速开始

### 1. 使用 AStarGridBaker 自动扫描场景

这是一个扫描场景中 Collider2D 并自动生成网格的组件。

1. 在空 GameObject 上添加 **A\* Grid Baker** 组件
2. 在 Inspector 中设置网格大小和障碍物图层
3. 在 Scene 视图中通过 **绿色(可通行)/ 红色(障碍物)** Gizmo 进行确认

```csharp
var baker = GetComponent<AStarGridBaker>();

// 월드 좌표 → 격자 좌표 변환
Vector2Int start = baker.WorldToCell(player.position);
Vector2Int end   = baker.WorldToCell(targetPosition);

// 경로 탐색 (대각선 허용)
var path = AStarPathfinder.FindPath(baker.Grid, start, end, diagonal: true);

// 격자 좌표 → 월드 좌표로 이동
foreach (var cell in path)
{
    Vector3 worldPos = baker.CellToWorld(cell);
    // 이동 처리... (예: AchMover와 연계 시 아래 [AchMover 연계] 섹션 참고)
}
```

> 如果没有路径,则返回空的 `List<Vector2Int>`。

### 与 AchFollower 联动

简单追踪请使用 [`AchFollower`](./movement#achfollower--ai-추적) —— 只需调用 `SetTarget()` 即可。

当需要沿 A* 路径按顺序经过各路径点时,需用代码自行处理。

```csharp
var follower = GetComponent<AchFollower>();

foreach (var cell in path)
{
    Vector2 waypoint = baker.CellToWorld(cell);
    follower.SetTarget(/* 웨이포인트 위치를 가진 Transform */);

    while (Vector2.Distance(transform.position, waypoint) > 0.05f)
        await Task.Yield();
}

follower.ClearTarget();
```

> 如果没有路径点 Transform,请不要使用 `AchFollower`,而是直接操作 `transform.position`。

### 2. 用代码直接创建网格

```csharp
var grid = new AStarGrid(20, 20);

// 장애물 설정
grid.SetWalkable(5, 3, false);

// 이동 비용이 높은 지형 (늪지, 모래 등)
grid.SetCost(4, 4, 3f);

// 경로 탐색
var path = AStarPathfinder.FindPath(
    grid,
    start:    new Vector2Int(0,  0),
    end:      new Vector2Int(19, 19),
    diagonal: false);
```

## AStarGridBaker 设置

| 字段 | 说明 |
|---|---|
| `Width` / `Height` | 网格单元数 |
| `CellSize` | 单个单元的 Unity 单位大小 |
| `Origin` | 网格左下角 (0,0) 的基准 Transform(留空则使用 GameObject 位置) |
| `ObstacleLayer` | 判定为障碍物的图层掩码 |
| `DetectionRadius` | 每个单元的障碍物检测半径(建议为 `cellSize * 0.4`) |
| `CostZones` | 移动成本区域数组(Collider2D + 成本值) |
| `DrawGizmos` | 是否在 Scene 视图中可视化网格 |

### 移动成本区域 (Cost Zones)

通过提高特定区域的移动成本,引导路径绕开该区域。

```csharp
// Inspector의 Cost Zones 배열에 추가하거나,
// 동적 장애물 변경 후 재스캔
baker.Bake();
```

| 场景 | 方法 |
|---|---|
| 墙壁为 `Collider2D` 的 2D 游戏 | 设置 `AStarGridBaker` 的 ObstacleLayer |
| 使用 Unity Tilemap | 将 `TilemapCollider2D` 放入障碍物图层 |
| 门的开关等动态障碍物 | 重新调用 `baker.Bake()` 以重建网格 |

## AStarGrid API

```csharp
var grid = new AStarGrid(width: 20, height: 20);

// bool 배열로 일괄 초기화
var grid2 = new AStarGrid(walkableMap: myBoolArray);

grid.SetWalkable(x, y, false);   // 장애물 지정
grid.SetCost(x, y, 3f);          // 이동 비용 설정 (1 이상)
bool ok   = grid.IsWalkable(x, y);
float cost = grid.GetCost(x, y);
bool inRange = grid.IsInBounds(x, y);
```

## AStarPathfinder API

```csharp
List<Vector2Int> path = AStarPathfinder.FindPath(
    grid:     grid,
    start:    new Vector2Int(0, 0),
    end:      new Vector2Int(10, 10),
    diagonal: true   // false = 4방향, true = 8방향
);
```

| 参数 | 说明 |
|---|---|
| `grid` | 要搜索的 `AStarGrid` |
| `start` | 起点网格坐标 |
| `end` | 目标网格坐标 |
| `diagonal` | 是否允许对角线移动(默认 `false`) |

**返回值**:不含起点、包含终点、按顺序排列的 `List<Vector2Int>`。若没有路径则返回空列表。

## 算法详解

| 项目 | 内容 |
|---|---|
| 搜索方式 | A*(Open Set:二叉最小堆) |
| 启发函数 | 4 方向 → Manhattan / 8 方向 → Chebyshev |
| 对角线移动成本 | √2 ≈ 1.414 |
| 单元权重 | 可通过 `AStarGrid.SetCost()` 设置 per-cell 成本 |
| 复杂度 | O((W×H) log(W×H)) |

> ⚠️ **拐角穿越**:进行对角线移动时,即使两个正交单元都被堵住,只要对角线单元处于开放状态就会通过。
> 如果介意角色看起来卡在墙角,可关闭对角线移动,或自行添加相邻单元检查逻辑。
