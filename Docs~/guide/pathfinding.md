# A* 길찾기

`AStarPathfinder`는 격자 기반 A* 알고리즘으로 두 지점 사이의 최단 경로를 탐색합니다.
4방향 및 대각선(8방향) 이동을 지원하며, 이진 최소 힙으로 효율적인 탐색을 수행합니다.

## 빠른 시작

### 1. AStarGridBaker로 씬 자동 스캔

씬의 Collider2D를 스캔해 격자를 자동 생성하는 컴포넌트입니다.

1. 빈 GameObject에 **A\* Grid Baker** 컴포넌트 추가
2. Inspector에서 격자 크기와 장애물 레이어 설정
3. Scene 뷰에서 **초록(이동 가능) / 빨강(장애물)** Gizmo로 확인

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
    // 이동 처리...
}
```

> 경로가 없으면 빈 `List<Vector2Int>`를 반환합니다.

### 2. 코드로 격자 직접 생성

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

## AStarGridBaker 설정

| 필드 | 설명 |
|---|---|
| `Width` / `Height` | 격자 셀 수 |
| `CellSize` | 셀 하나의 Unity 단위 크기 |
| `Origin` | 격자 좌하단(0,0)의 기준 Transform (비우면 GameObject 위치 사용) |
| `ObstacleLayer` | 장애물로 판단할 레이어 마스크 |
| `DetectionRadius` | 셀 당 장애물 감지 반경 (`cellSize * 0.4` 권장) |
| `CostZones` | 이동 비용 구역 배열 (Collider2D + 비용 값) |
| `DrawGizmos` | Scene 뷰 격자 시각화 표시 여부 |

### 이동 비용 구역 (Cost Zones)

특정 영역의 이동 비용을 높여 경로가 해당 구역을 우회하도록 유도합니다.

```csharp
// Inspector의 Cost Zones 배열에 추가하거나,
// 동적 장애물 변경 후 재스캔
baker.Bake();
```

| 상황 | 방법 |
|---|---|
| 벽이 `Collider2D`인 2D 게임 | `AStarGridBaker` ObstacleLayer 설정 |
| Unity Tilemap 사용 | `TilemapCollider2D`를 장애물 레이어에 배치 |
| 문이 열리고 닫히는 등 동적 장애물 | `baker.Bake()` 재호출로 격자 재생성 |

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

| 파라미터 | 설명 |
|---|---|
| `grid` | 탐색할 `AStarGrid` |
| `start` | 시작 격자 좌표 |
| `end` | 목표 격자 좌표 |
| `diagonal` | 대각선 이동 허용 여부 (기본 `false`) |

**반환값**: 시작점 제외, 도착점 포함 순서의 `List<Vector2Int>`. 경로가 없으면 빈 리스트.

## 알고리즘 상세

| 항목 | 내용 |
|---|---|
| 탐색 방식 | A* (Open Set: 이진 최소 힙) |
| 휴리스틱 | 4방향 → Manhattan / 8방향 → Chebyshev |
| 대각선 이동 비용 | √2 ≈ 1.414 |
| 셀 가중치 | `AStarGrid.SetCost()` 로 per-cell 비용 설정 가능 |
| 복잡도 | O((W×H) log(W×H)) |
