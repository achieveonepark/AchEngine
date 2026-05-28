# A* 経路探索

`AStarPathfinder`は格子ベースのA*アルゴリズムで、2点間の最短経路を探索します。
4方向および対角線（8方向）移動をサポートし、バイナリ最小ヒープで効率的な探索を実行します。

## クイックスタート

### 1. AStarGridBaker でシーンを自動スキャン

シーン内のCollider2Dをスキャンして格子を自動生成するコンポーネントです。

1. 空のGameObjectに**A\* Grid Baker**コンポーネントを追加
2. Inspectorで格子サイズと障害物レイヤーを設定
3. Sceneビューで**緑（移動可能） / 赤（障害物）**のGizmoで確認

```csharp
var baker = GetComponent<AStarGridBaker>();

// ワールド座標 → 格子座標へ変換
Vector2Int start = baker.WorldToCell(player.position);
Vector2Int end   = baker.WorldToCell(targetPosition);

// 経路探索（対角線許可）
var path = AStarPathfinder.FindPath(baker.Grid, start, end, diagonal: true);

// 格子座標 → ワールド座標へ変換して移動
foreach (var cell in path)
{
    Vector3 worldPos = baker.CellToWorld(cell);
    // 移動処理... (例: AchMoverと連携する場合は下の [AchMover 連携] セクションを参照)
}
```

> 経路が存在しない場合は空の`List<Vector2Int>`を返します。

### AchFollower 連携

単純な追跡には[`AchFollower`](./movement#achfollower--ai-追跡)を使用してください — `SetTarget()`を呼び出すだけで完了します。

A*経路に沿ってウェイポイントを順番に移動する必要がある場合は、コードで直接処理します。

```csharp
var follower = GetComponent<AchFollower>();

foreach (var cell in path)
{
    Vector2 waypoint = baker.CellToWorld(cell);
    follower.SetTarget(/* ウェイポイント位置を持つTransform */);

    while (Vector2.Distance(transform.position, waypoint) > 0.05f)
        await Task.Yield();
}

follower.ClearTarget();
```

> ウェイポイントのTransformがない場合は、`AchFollower`の代わりに`transform.position`を直接操作してください。

### 2. コードで格子を直接生成

```csharp
var grid = new AStarGrid(20, 20);

// 障害物の設定
grid.SetWalkable(5, 3, false);

// 移動コストの高い地形（沼地、砂地など）
grid.SetCost(4, 4, 3f);

// 経路探索
var path = AStarPathfinder.FindPath(
    grid,
    start:    new Vector2Int(0,  0),
    end:      new Vector2Int(19, 19),
    diagonal: false);
```

## AStarGridBaker の設定

| フィールド | 説明 |
|---|---|
| `Width` / `Height` | 格子セル数 |
| `CellSize` | セル1つのUnity単位サイズ |
| `Origin` | 格子の左下(0,0)の基準Transform（空欄の場合はGameObjectの位置を使用） |
| `ObstacleLayer` | 障害物と判定するレイヤーマスク |
| `DetectionRadius` | セルごとの障害物検出半径（`cellSize * 0.4`推奨） |
| `CostZones` | 移動コスト区域の配列（Collider2D + コスト値） |
| `DrawGizmos` | Sceneビューでの格子可視化の有無 |

### 移動コスト区域 (Cost Zones)

特定エリアの移動コストを上げることで、経路がそのエリアを迂回するように誘導します。

```csharp
// InspectorのCost Zones配列に追加するか、
// 動的に障害物を変更した後に再スキャン
baker.Bake();
```

| 状況 | 方法 |
|---|---|
| 壁が`Collider2D`の2Dゲーム | `AStarGridBaker`のObstacleLayerを設定 |
| Unity Tilemap使用時 | `TilemapCollider2D`を障害物レイヤーに配置 |
| 開閉する扉など動的な障害物 | `baker.Bake()`を再呼び出しして格子を再生成 |

## AStarGrid API

```csharp
var grid = new AStarGrid(width: 20, height: 20);

// bool配列で一括初期化
var grid2 = new AStarGrid(walkableMap: myBoolArray);

grid.SetWalkable(x, y, false);   // 障害物指定
grid.SetCost(x, y, 3f);          // 移動コスト設定（1以上）
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
    diagonal: true   // false = 4方向, true = 8方向
);
```

| パラメータ | 説明 |
|---|---|
| `grid` | 探索対象の`AStarGrid` |
| `start` | 開始格子座標 |
| `end` | 目標格子座標 |
| `diagonal` | 対角線移動の許可（デフォルト`false`） |

**戻り値**: 開始点を除き、到達点を含む順序の`List<Vector2Int>`。経路がなければ空のリスト。

## アルゴリズム詳細

| 項目 | 内容 |
|---|---|
| 探索方式 | A* (Open Set: バイナリ最小ヒープ) |
| ヒューリスティック | 4方向 → Manhattan / 8方向 → Chebyshev |
| 対角線移動コスト | √2 ≈ 1.414 |
| セル重み | `AStarGrid.SetCost()`によるセル単位コスト設定が可能 |
| 計算量 | O((W×H) log(W×H)) |

> ⚠️ **コーナーカット**: 対角線移動時に直交する2つのセルが両方とも閉じていても、対角線のセルが開いていれば通過します。
> 壁の角に引っかかるように見えるのが気になる場合は、対角線移動を無効にするか、隣接セル検査ロジックを自作してください。
