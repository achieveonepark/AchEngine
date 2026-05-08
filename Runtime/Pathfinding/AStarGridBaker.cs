using System.Collections.Generic;
using UnityEngine;

namespace AchEngine.Pathfinding
{
    /// <summary>
    /// 씬의 Collider를 스캔해서 AStarGrid를 자동 생성합니다.
    ///
    /// 사용 방법:
    ///   1. 씬의 빈 GameObject에 이 컴포넌트를 추가합니다.
    ///   2. Inspector에서 격자 크기(Width, Height), 셀 크기(CellSize),
    ///      원점(Origin), 장애물 레이어(ObstacleLayer)를 설정합니다.
    ///   3. 런타임에 Grid 프로퍼티로 AStarGrid를 가져와 FindPath에 전달합니다.
    ///
    /// <code>
    /// var baker = GetComponent&lt;AStarGridBaker&gt;();
    ///
    /// // 월드 좌표 → 격자 좌표 변환
    /// Vector2Int startCell = baker.WorldToCell(player.position);
    /// Vector2Int endCell   = baker.WorldToCell(target.position);
    ///
    /// // 경로 탐색
    /// var path = AStarPathfinder.FindPath(baker.Grid, startCell, endCell, diagonal: true);
    ///
    /// // 격자 좌표 → 월드 좌표로 이동
    /// foreach (var cell in path)
    /// {
    ///     Vector3 worldPos = baker.CellToWorld(cell);
    ///     transform.position = worldPos;
    /// }
    /// </code>
    /// </summary>
    [AddComponentMenu("AchEngine/Pathfinding/A* Grid Baker")]
    public class AStarGridBaker : MonoBehaviour
    {
        [Header("Grid Size")]
        [Tooltip("격자 가로 셀 수")]
        [SerializeField] private int width  = 20;

        [Tooltip("격자 세로 셀 수")]
        [SerializeField] private int height = 20;

        [Tooltip("셀 하나의 Unity 단위 크기")]
        [SerializeField] private float cellSize = 1f;

        [Header("World Position")]
        [Tooltip("격자 좌하단(0,0)의 월드 좌표. 비워두면 이 GameObject의 위치를 사용합니다.")]
        [SerializeField] private Transform origin;

        [Header("Obstacle Detection")]
        [Tooltip("장애물로 판단할 레이어")]
        [SerializeField] private LayerMask obstacleLayer;

        [Tooltip("장애물 감지 반경 (cellSize * 0.4 정도가 적당)")]
        [SerializeField] private float detectionRadius = 0.4f;

        [Header("Cost Zones (선택)")]
        [Tooltip("이동 비용을 높일 영역 설정 (늪지대, 모래 등)")]
        [SerializeField] private CostZone[] costZones;

        [Header("Debug")]
        [SerializeField] private bool drawGizmos = true;

        /// <summary>Bake()로 생성된 격자. Awake에서 자동 생성됩니다.</summary>
        public AStarGrid Grid { get; private set; }

        private Vector3 OriginPos => origin != null ? origin.position : transform.position;

        private void Awake() => Bake();

        /// <summary>
        /// 씬의 Collider를 스캔해서 격자를 (재)생성합니다.
        /// 동적으로 장애물이 변하는 경우 호출하세요.
        /// </summary>
        public void Bake()
        {
            Grid = new AStarGrid(width, height);

            float half = cellSize * detectionRadius;

            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                Vector2 center = CellToWorld2D(x, y);

                bool blocked = Physics2D.OverlapCircle(center, half, obstacleLayer) != null;
                Grid.SetWalkable(x, y, !blocked);
            }

            // 비용 구역 적용
            if (costZones != null)
                foreach (var zone in costZones)
                    ApplyCostZone(zone);

            Debug.Log($"[AStarGridBaker] 격자 생성 완료 ({width}×{height}, cellSize={cellSize})");
        }

        // ── 좌표 변환 ────────────────────────────────────────────────────────

        /// <summary>월드 좌표를 격자 좌표로 변환합니다.</summary>
        public Vector2Int WorldToCell(Vector3 worldPos)
        {
            Vector3 local = worldPos - OriginPos;
            int x = Mathf.FloorToInt(local.x / cellSize);
            int y = Mathf.FloorToInt(local.y / cellSize);
            return new Vector2Int(
                Mathf.Clamp(x, 0, width  - 1),
                Mathf.Clamp(y, 0, height - 1));
        }

        /// <summary>격자 좌표를 월드 좌표(셀 중심)로 변환합니다.</summary>
        public Vector3 CellToWorld(Vector2Int cell)
        {
            float half = cellSize * 0.5f;
            return OriginPos + new Vector3(
                cell.x * cellSize + half,
                cell.y * cellSize + half,
                0f);
        }

        // ── 내부 유틸 ────────────────────────────────────────────────────────

        private Vector2 CellToWorld2D(int x, int y)
        {
            float half = cellSize * 0.5f;
            Vector3 o  = OriginPos;
            return new Vector2(o.x + x * cellSize + half, o.y + y * cellSize + half);
        }

        private void ApplyCostZone(CostZone zone)
        {
            if (zone.ZoneCollider == null) return;
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                Vector2 center = CellToWorld2D(x, y);
                if (zone.ZoneCollider.OverlapPoint(center))
                    Grid.SetCost(x, y, zone.MovementCost);
            }
        }

        // ── Gizmo ────────────────────────────────────────────────────────────
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!drawGizmos) return;

            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                Vector3 center = OriginPos + new Vector3(
                    x * cellSize + cellSize * 0.5f,
                    y * cellSize + cellSize * 0.5f, 0f);

                if (Grid != null)
                    Gizmos.color = Grid.IsWalkable(x, y) ? new Color(0, 1, 0, 0.15f) : new Color(1, 0, 0, 0.35f);
                else
                    Gizmos.color = new Color(1, 1, 1, 0.1f);

                Gizmos.DrawCube(center, Vector3.one * (cellSize * 0.95f));
            }
        }
#endif

        // ── 중첩 타입 ────────────────────────────────────────────────────────

        [System.Serializable]
        public class CostZone
        {
            [Tooltip("비용 적용 영역의 Collider2D")]
            public Collider2D ZoneCollider;

            [Tooltip("이동 비용 (1 이상, 높을수록 우회)")]
            [Min(1f)] public float MovementCost = 3f;
        }
    }
}
