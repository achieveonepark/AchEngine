using UnityEngine;

namespace AchEngine.Movement
{
    /// <summary>
    /// 붙이기만 하면 동작하는 2D 캐릭터 이동 컴포넌트.
    ///
    /// ■ Platformer 모드  — 중력 적용, 좌우 이동 + 점프
    /// ■ TopDown 모드     — 중력 없음, 상하좌우 자유 이동
    ///
    /// Movable = true  : 플레이어 입력으로 이동
    /// Movable = false : 입력 비활성화, Move() / Jump() 등 코드로만 제어
    ///                   (컷씬, 넉백, AI 이동 등에 활용)
    /// </summary>
    [AddComponentMenu("AchEngine/Movement/Ach Mover")]
    [RequireComponent(typeof(Rigidbody2D))]
    public class AchMover : MonoBehaviour
    {
        // ── 이동 설정 ──────────────────────────────────────────────────────

        [Header("Movement")]
        [Tooltip("이동 속도 (Units/sec)")]
        public float MoveSpeed = 5f;

        [Tooltip("Platformer 전용 — 점프 힘")]
        public float JumpForce = 12f;

        [Tooltip("이동 모드")]
        public MovementMode Mode = MovementMode.Platformer;

        // ── 물리 설정 ──────────────────────────────────────────────────────

        [Header("Physics")]
        [Tooltip("중력 배율 (Platformer 전용, TopDown이면 자동으로 0 적용)")]
        public float GravityScale = 3f;

        [Tooltip("낙하 시 추가 중력 배율 — 낙하가 더 빠르게 느껴집니다")]
        public float FallMultiplier = 2f;

        [Tooltip("최대 낙하 속도")]
        public float MaxFallSpeed = 20f;

        // ── 지면 감지 ─────────────────────────────────────────────────────

        [Header("Ground Check  (Platformer)")]
        [Tooltip("지면으로 판단할 레이어")]
        public LayerMask GroundLayer;

        [Tooltip("발 위치 Transform. 비워두면 Collider 하단을 자동 계산합니다.")]
        public Transform GroundCheckPoint;

        [Tooltip("지면 감지 반경")]
        public float GroundCheckRadius = 0.1f;

        // ── 제어 ──────────────────────────────────────────────────────────

        [Header("Control")]
        [Tooltip("true  : 플레이어 입력으로 이동\n" +
                 "false : 입력 차단 — Move() / Jump() 등 코드로만 제어 가능")]
        public bool Movable = true;

        [Tooltip("이동 방향에 따라 스프라이트를 자동으로 좌우 반전합니다.")]
        public bool FlipSprite = true;

        // ── 읽기 전용 상태 ────────────────────────────────────────────────

        /// <summary>현재 지면에 닿아 있는지 여부 (Platformer 전용)</summary>
        public bool IsGrounded { get; private set; }

        /// <summary>현재 이동 속도 벡터</summary>
        public Vector2 Velocity => _rb.linearVelocity;

        // ── 내부 ──────────────────────────────────────────────────────────

        private Rigidbody2D _rb;
        private Collider2D  _col;
        private SpriteRenderer _sprite;
        private Vector2 _inputDir;
        private bool    _jumpQueued;

        // ── Unity 생명주기 ────────────────────────────────────────────────

        private void Awake()
        {
            _rb     = GetComponent<Rigidbody2D>();
            _col    = GetComponent<Collider2D>();
            _sprite = GetComponent<SpriteRenderer>();

            ApplyMode();
        }

        private void Update()
        {
            if (Movable)
                ReadInput();

            if (FlipSprite && _sprite != null && Mathf.Abs(_inputDir.x) > 0.01f)
                _sprite.flipX = _inputDir.x < 0f;
        }

        private void FixedUpdate()
        {
            CheckGround();
            ApplyMovement();
            ApplyFallGravity();
        }

        // ── 코드 제어 API ─────────────────────────────────────────────────

        /// <summary>
        /// 코드에서 캐릭터를 이동시킵니다.
        /// Movable 값에 관계없이 동작합니다.
        /// </summary>
        /// <param name="direction">이동 방향 벡터 (크기 1 권장. TopDown은 Vector2, Platformer는 x축만 사용)</param>
        public void Move(Vector2 direction)
        {
            _inputDir = direction;
        }

        /// <summary>
        /// 코드에서 점프를 실행합니다 (Platformer 전용).
        /// Movable 값에 관계없이 동작합니다.
        /// </summary>
        public void Jump()
        {
            if (Mode == MovementMode.Platformer)
                _jumpQueued = true;
        }

        /// <summary>즉시 지정 위치로 텔레포트합니다.</summary>
        public void Teleport(Vector2 worldPosition)
        {
            _rb.position = worldPosition;
            _rb.linearVelocity = Vector2.zero;
        }

        /// <summary>Rigidbody2D 속도를 직접 설정합니다 (넉백, 밀치기 등).</summary>
        public void SetVelocity(Vector2 velocity)
        {
            _rb.linearVelocity = velocity;
        }

        /// <summary>수평 방향으로 힘을 가합니다 (넉백 효과 등).</summary>
        public void AddForce(Vector2 force, ForceMode2D mode = ForceMode2D.Impulse)
        {
            _rb.AddForce(force, mode);
        }

        /// <summary>이동을 즉시 멈춥니다.</summary>
        public void Stop()
        {
            _inputDir = Vector2.zero;
            _rb.linearVelocity = Mode == MovementMode.Platformer
                ? new Vector2(0f, _rb.linearVelocity.y)
                : Vector2.zero;
        }

        // ── 내부 로직 ─────────────────────────────────────────────────────

        private void ReadInput()
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");

            _inputDir = Mode == MovementMode.TopDown
                ? new Vector2(h, v).normalized
                : new Vector2(h, 0f);

            if (Mode == MovementMode.Platformer &&
                IsGrounded &&
                (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)))
            {
                _jumpQueued = true;
            }
        }

        private void ApplyMovement()
        {
            if (Mode == MovementMode.TopDown)
            {
                _rb.linearVelocity = _inputDir * MoveSpeed;
                _inputDir = Vector2.zero;
                return;
            }

            // Platformer — 수평만 덮어씀, 수직은 물리에 맡김
            float targetVx = _inputDir.x * MoveSpeed;
            _rb.linearVelocity = new Vector2(targetVx, _rb.linearVelocity.y);

            if (_jumpQueued)
            {
                if (IsGrounded)
                {
                    _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, 0f);
                    _rb.AddForce(Vector2.up * JumpForce, ForceMode2D.Impulse);
                }
                _jumpQueued = false;
            }
        }

        private void ApplyFallGravity()
        {
            if (Mode != MovementMode.Platformer) return;

            if (_rb.linearVelocity.y < 0f)
            {
                // 낙하 중 — 추가 중력
                _rb.linearVelocity += Vector2.up * (Physics2D.gravity.y * (FallMultiplier - 1f) * Time.fixedDeltaTime);

                // 최대 낙하 속도 제한
                if (_rb.linearVelocity.y < -MaxFallSpeed)
                    _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, -MaxFallSpeed);
            }
        }

        private void CheckGround()
        {
            if (Mode != MovementMode.Platformer)
            {
                IsGrounded = false;
                return;
            }

            Vector2 checkPos = GetGroundCheckPosition();
            IsGrounded = Physics2D.OverlapCircle(checkPos, GroundCheckRadius, GroundLayer);
        }

        private Vector2 GetGroundCheckPosition()
        {
            if (GroundCheckPoint != null)
                return GroundCheckPoint.position;

            // Collider 하단 자동 계산
            if (_col != null)
                return new Vector2(transform.position.x, _col.bounds.min.y);

            return new Vector2(transform.position.x, transform.position.y - 0.5f);
        }

        private void ApplyMode()
        {
            _rb.gravityScale = Mode == MovementMode.TopDown ? 0f : GravityScale;
            _rb.freezeRotation = true;

            if (Mode == MovementMode.TopDown)
                _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        // ── Gizmo ─────────────────────────────────────────────────────────
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (Mode != MovementMode.Platformer) return;

            Collider2D col = GetComponent<Collider2D>();
            Vector2 pos = col != null
                ? new Vector2(transform.position.x, col.bounds.min.y)
                : new Vector2(transform.position.x, transform.position.y - 0.5f);

            if (GroundCheckPoint != null)
                pos = GroundCheckPoint.position;

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(pos, GroundCheckRadius);
        }
#endif
    }

    public enum MovementMode
    {
        /// <summary>중력 적용 — 좌우 이동 + 점프 (A/D + Space)</summary>
        Platformer,

        /// <summary>중력 없음 — 상하좌우 자유 이동 (WASD)</summary>
        TopDown,
    }
}
