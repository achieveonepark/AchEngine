using System.Collections.Generic;
using UnityEngine;

namespace AchEngine.Pathfinding
{
    /// <summary>
    /// A* 알고리즘 기반 격자 경로 탐색기입니다.
    ///
    /// <code>
    /// var grid = new AStarGrid(20, 20);
    /// grid.SetWalkable(5, 3, false);           // 장애물
    /// grid.SetCost(4, 4, 3f);                  // 이동 비용 높은 지형
    ///
    /// var path = AStarPathfinder.FindPath(
    ///     grid,
    ///     start:    new Vector2Int(0, 0),
    ///     end:      new Vector2Int(19, 19),
    ///     diagonal: true);
    ///
    /// // path: 시작점 제외 / 도착점 포함 순서의 Vector2Int 목록
    /// // 경로 없으면 빈 리스트 반환
    /// </code>
    /// </summary>
    public static class AStarPathfinder
    {
        // 4방향 / 8방향 이동 방향 벡터
        private static readonly Vector2Int[] Dirs4 =
        {
            new( 0,  1), new( 0, -1),
            new( 1,  0), new(-1,  0),
        };

        private static readonly Vector2Int[] Dirs8 =
        {
            new( 0,  1), new( 0, -1),
            new( 1,  0), new(-1,  0),
            new( 1,  1), new( 1, -1),
            new(-1,  1), new(-1, -1),
        };

        // 대각선 이동 비용 (√2 ≈ 1.414)
        private const float DiagonalCost = 1.414f;

        /// <summary>
        /// A* 알고리즘으로 시작점에서 도착점까지의 최단 경로를 탐색합니다.
        /// </summary>
        /// <param name="grid">탐색할 격자</param>
        /// <param name="start">시작 좌표</param>
        /// <param name="end">목표 좌표</param>
        /// <param name="diagonal">대각선 이동 허용 여부 (기본 false)</param>
        /// <returns>
        /// 시작점을 제외한 경로 좌표 목록 (도착점 포함, 이동 순서).
        /// 경로가 없으면 빈 리스트를 반환합니다.
        /// </returns>
        public static List<Vector2Int> FindPath(
            AStarGrid  grid,
            Vector2Int start,
            Vector2Int end,
            bool       diagonal = false)
        {
            if (!grid.IsWalkable(start.x, start.y) || !grid.IsWalkable(end.x, end.y))
                return new List<Vector2Int>();

            if (start == end)
                return new List<Vector2Int>();

            var dirs     = diagonal ? Dirs8 : Dirs4;
            var openHeap = new MinHeap<Node>(capacity: 64);
            var closed   = new HashSet<Vector2Int>();
            var nodeMap  = new Dictionary<Vector2Int, Node>();

            var startNode = new Node(start, g: 0f, h: Heuristic(start, end, diagonal));
            openHeap.Push(startNode);
            nodeMap[start] = startNode;

            while (openHeap.Count > 0)
            {
                var current = openHeap.Pop();

                if (current.Pos == end)
                    return ReconstructPath(current);

                closed.Add(current.Pos);

                foreach (var dir in dirs)
                {
                    var neighborPos = current.Pos + dir;

                    if (!grid.IsWalkable(neighborPos.x, neighborPos.y)) continue;
                    if (closed.Contains(neighborPos)) continue;

                    bool isDiag    = dir.x != 0 && dir.y != 0;
                    float moveCost = (isDiag ? DiagonalCost : 1f) * grid.GetCost(neighborPos.x, neighborPos.y);
                    float tentativeG = current.G + moveCost;

                    if (nodeMap.TryGetValue(neighborPos, out var existing))
                    {
                        if (tentativeG >= existing.G) continue;

                        // 더 좋은 경로 발견 → 기존 노드 갱신
                        existing.G      = tentativeG;
                        existing.Parent = current;
                        openHeap.UpdatePriority(existing);
                    }
                    else
                    {
                        var neighbor = new Node(
                            neighborPos,
                            g:      tentativeG,
                            h:      Heuristic(neighborPos, end, diagonal),
                            parent: current);
                        openHeap.Push(neighbor);
                        nodeMap[neighborPos] = neighbor;
                    }
                }
            }

            // 경로 없음
            return new List<Vector2Int>();
        }

        // ── 휴리스틱 ─────────────────────────────────────────────────────────
        // 4방향: Manhattan / 8방향: Chebyshev
        private static float Heuristic(Vector2Int a, Vector2Int b, bool diagonal)
        {
            int dx = Mathf.Abs(a.x - b.x);
            int dy = Mathf.Abs(a.y - b.y);

            if (!diagonal)
                return dx + dy;

            int min = dx < dy ? dx : dy;
            int max = dx > dy ? dx : dy;
            return DiagonalCost * min + (max - min);
        }

        // ── 경로 역추적 ───────────────────────────────────────────────────────
        private static List<Vector2Int> ReconstructPath(Node end)
        {
            var path = new List<Vector2Int>();
            var node = end;
            while (node.Parent != null)
            {
                path.Add(node.Pos);
                node = node.Parent;
            }
            path.Reverse();
            return path;
        }

        // ── 내부 노드 ─────────────────────────────────────────────────────────
        /// <summary>A* 탐색 중 각 격자 셀을 표현하는 내부 노드.</summary>
        private class Node : IHeapItem<Node>
        {
            /// <summary>격자 좌표.</summary>
            public Vector2Int Pos    { get; }
            /// <summary>시작점으로부터의 실제 이동 비용.</summary>
            public float      G      { get; set; }
            /// <summary>목표까지의 휴리스틱 추정 비용.</summary>
            public float      H      { get; }
            /// <summary>총 추정 비용 (G + H).</summary>
            public float      F      => G + H;
            /// <summary>이 노드에 도달하기 직전 노드.</summary>
            public Node       Parent { get; set; }

            // MinHeap 인덱스 추적용
            public int HeapIndex { get; set; }

            public Node(Vector2Int pos, float g, float h, Node parent = null)
            {
                Pos    = pos;
                G      = g;
                H      = h;
                Parent = parent;
            }

            public int CompareTo(Node other) => F.CompareTo(other.F);
        }

        // ── 최소 힙 (이진 힙 기반 O(log n) 삽입/삭제) ─────────────────────
        /// <summary>MinHeap에서 관리되는 항목이 구현해야 하는 인터페이스.</summary>
        private interface IHeapItem<T> : System.IComparable<T>
        {
            /// <summary>힙 내 현재 인덱스.</summary>
            int HeapIndex { get; set; }
        }

        /// <summary>이진 힙 기반 최소 우선순위 큐. O(log n) 삽입·삭제를 지원한다.</summary>
        private class MinHeap<T> where T : IHeapItem<T>
        {
            private readonly List<T> _items;

            /// <summary>힙에 있는 항목 수.</summary>
            public int Count => _items.Count;

            public MinHeap(int capacity = 16) => _items = new List<T>(capacity);

            /// <summary>항목을 힙에 삽입한다.</summary>
            public void Push(T item)
            {
                item.HeapIndex = _items.Count;
                _items.Add(item);
                BubbleUp(item.HeapIndex);
            }

            /// <summary>우선순위가 가장 낮은(F 값이 가장 작은) 항목을 꺼낸다.</summary>
            public T Pop()
            {
                var top  = _items[0];
                var last = _items[Count - 1];
                _items.RemoveAt(Count - 1);
                if (_items.Count > 0)
                {
                    _items[0]       = last;
                    last.HeapIndex  = 0;
                    SiftDown(0);
                }
                return top;
            }

            /// <summary>항목의 우선순위가 낮아진 경우(G 값 감소) 힙을 재정렬한다.</summary>
            public void UpdatePriority(T item) => BubbleUp(item.HeapIndex);

            private void BubbleUp(int i)
            {
                while (i > 0)
                {
                    int parent = (i - 1) / 2;
                    if (_items[i].CompareTo(_items[parent]) >= 0) break;
                    Swap(i, parent);
                    i = parent;
                }
            }

            private void SiftDown(int i)
            {
                while (true)
                {
                    int left  = 2 * i + 1;
                    int right = 2 * i + 2;
                    int smallest = i;

                    if (left  < Count && _items[left].CompareTo(_items[smallest])  < 0) smallest = left;
                    if (right < Count && _items[right].CompareTo(_items[smallest]) < 0) smallest = right;

                    if (smallest == i) break;
                    Swap(i, smallest);
                    i = smallest;
                }
            }

            private void Swap(int a, int b)
            {
                (_items[a], _items[b]) = (_items[b], _items[a]);
                _items[a].HeapIndex = a;
                _items[b].HeapIndex = b;
            }
        }
    }
}
