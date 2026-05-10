using UnityEngine;

namespace AchEngine
{
    /// <summary>발사체 이동 방식.</summary>
    public enum ProjectileType
    {
        /// <summary>고정 방향으로 직선 이동.</summary>
        Straight,
        /// <summary>대상을 향해 선회하며 추적 (유도탄).</summary>
        Homing,
    }

    /// <summary>
    /// 직선·유도탄 등 다양한 발사체 이동을 관리하는 컴포넌트.
    /// transform.position을 직접 이동하며 Rigidbody2D를 사용하지 않습니다.
    /// </summary>
    public class AchProjectile : MonoBehaviour
    {
        // ── 공통 ──────────────────────────────────────────────────────────
        [Header("공통")]
        [Tooltip("발사체 이동 방식.")]
        public ProjectileType Type = ProjectileType.Straight;

        [Tooltip("이동 속도 (Units/sec).")]
        public float MoveSpeed = 10f;

        // ── Straight ──────────────────────────────────────────────────────
        [Header("Straight")]
        [Tooltip("초기 이동 방향. Launch(Vector2) 호출 시 덮어씁니다.")]
        public Vector2 Direction = Vector2.right;

        // ── Homing ────────────────────────────────────────────────────────
        [Header("Homing")]
        [Tooltip("추적할 대상 Transform.")]
        public Transform Target;

        [Tooltip("초당 선회 가능한 최대 각도 (도). 낮을수록 완만하게 돕니다.")]
        public float TurnSpeed = 180f;

        // ── 내부 상태 ─────────────────────────────────────────────────────
        private Vector2 _currentDir;

        // ─────────────────────────────────────────────────────────────────

        void Awake()
        {
            // 초기 방향이 zero이면 오른쪽을 기본값으로
            _currentDir = Direction.sqrMagnitude > 0f ? Direction.normalized : Vector2.right;
        }

        void FixedUpdate()
        {
            switch (Type)
            {
                case ProjectileType.Straight: MoveStraight(); break;
                case ProjectileType.Homing:   MoveHoming();   break;
            }
        }

        // ── 이동 처리 ─────────────────────────────────────────────────────

        void MoveStraight()
        {
            transform.position += (Vector3)(_currentDir * MoveSpeed * Time.fixedDeltaTime);
        }

        void MoveHoming()
        {
            // 타겟 없으면 마지막 방향 그대로 직선 이동
            if (Target == null) { MoveStraight(); return; }

            Vector2 toTarget = ((Vector2)Target.position - (Vector2)transform.position).normalized;

            // 현재 진행 각도 → 목표 각도 사이를 TurnSpeed만큼만 회전
            float cur = Mathf.Atan2(_currentDir.y, _currentDir.x) * Mathf.Rad2Deg;
            float dst = Mathf.Atan2(toTarget.y,    toTarget.x)    * Mathf.Rad2Deg;
            float next = Mathf.MoveTowardsAngle(cur, dst, TurnSpeed * Time.fixedDeltaTime);

            _currentDir = new Vector2(
                Mathf.Cos(next * Mathf.Deg2Rad),
                Mathf.Sin(next * Mathf.Deg2Rad));

            transform.position += (Vector3)(_currentDir * MoveSpeed * Time.fixedDeltaTime);
        }

        // ── 공개 API ──────────────────────────────────────────────────────

        /// <summary>
        /// 발사 방향을 설정합니다. Awake 이후 언제든 호출 가능합니다.
        /// </summary>
        public void Launch(Vector2 direction)
        {
            _currentDir = direction.normalized;
            Direction   = _currentDir;
        }

        /// <summary>
        /// 추적 대상을 설정합니다. (Homing 전용)
        /// </summary>
        public void SetTarget(Transform target) => Target = target;

        /// <summary>
        /// 추적 대상을 해제합니다.
        /// </summary>
        public void ClearTarget() => Target = null;
    }
}
