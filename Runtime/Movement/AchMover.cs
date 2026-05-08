using UnityEngine;

namespace AchEngine.Movement
{
    /// <summary>
    /// 붙이기만 하면 동작하는 2D 캐릭터 이동 컴포넌트.
    /// Rigidbody2D / CapsuleCollider2D를 자동으로 추가·설정합니다.
    /// 레이어, 발 위치, 반경 등 별도 설정이 필요 없습니다.
    ///
    /// ■ Platformer 모드 — 중력 적용, 좌우 이동(A/D) + 점프(Space/W/↑)
    /// ■ TopDown 모드    — 중력 없음, 상하좌우 자유 이동(WASD/방향키)
    ///
    /// Movable = true  : 플레이어 입력으로 이동
    /// Movable = false : 입력 차단 — Move() / Jump() / SetVelocity() 등 코드로만 제어
    ///                   (컷씬, 넉백, AI 이동 등)
    /// </summary>
    [AddComponentMenu("AchEngine/Movement/Ach Mover")]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CapsuleCollider2D))]
    public class AchMover : MonoBehaviour
    {
        // ── 이동 ──────────────────────────────────────────────────────────

        [Header("Movement")]
        [Tooltip("이동 속도 (Units/sec)")]
        public float MoveSpeed = 5f;

        [Tooltip("점프 힘 (Platformer 전용)")]
        public float JumpForce = 12f;

        [Tooltip("Platformer = 중력 + 점프 / TopDown = 중력 없음 + 4방향")]
        public MovementMode Mode = MovementMode.Platformer;

        // ── 물리 ──────────────────────────────────────────────────────────

        [Header("Physics  (Platformer)")]
        [Tooltip("중력 배율")]
        public float GravityScale = 3f;

        [Tooltip("낙하 중 추가 중력 배율 — 높을수록 낙하가 빠르고 묵직한 느낌")]
        public float FallMultiplier = 2f;

        [Tooltip("최대 낙하 속도")]
        public float MaxFallSpeed = 20f;

        // ── 제어 ──────────────────────────────────────────────────────────

        [Header("Control")]
        [Tooltip("true : 플레이어 입력으로 이동\nfalse : 입력 차단, 코드로만 이동 가능")]
        public bool Movable = true;

        [Tooltip("이동 방향에 따라 SpriteRenderer를 자동 좌우 반전")]
        public bool FlipSprite = true;

        // ── 읽기 전용 상태 ────────────────────────────────────────────────

        /// <summary>현재 지면에 닿아 있는지 여부 (Platformer 전용)</summary>
        public bool IsGrounded { get; private set; }

        /// <summary>현재 Rigidbody2D 속도</summary>
        public Vector2 Velocity => _rb.linearVelocity;

        // ── 내부 ──────────────────────────────────────────────────────────

        private Rigidbody2D    _rb;
        private SpriteRenderer _sprite;
        private Vector2        _inputDir;
        private bool           _jumpQueued;
        private bool           _groundedThisFrame;  // OnCollisionStay2D에서 매 물리 프레임 갱신

        // ── Unity 생명주기 ────────────────────────────────────────────────

        private void Awake()
        {
            _rb     = GetComponent<Rigidbody2D>();
            _sprite = GetComponent<SpriteRenderer>();

            // Rigidbody2D 자동 설정
            _rb.freezeRotation = true;
            _rb.gravityScale   = Mode == MovementMode.TopDown ? 0f : GravityScale;
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            // CapsuleCollider2D — RequireComponent가 이미 추가했으나 크기 미설정 시 기본값 유지
        }

        private void Update()
        {
            if (Movable) ReadInput();

            if (FlipSprite && _sprite != null && Mathf.Abs(_inputDir.x) > 0.01f)
                _sprite.flipX = _inputDir.x < 0f;
        }

        private void FixedUpdate()
        {
            // 접촉 감지 결과를 IsGrounded에 반영 후 다음 프레임을 위해 초기화
            IsGrounded          = _groundedThisFrame;
            _groundedThisFrame  = false;

            ApplyMovement();
            ApplyFallGravity();
        }

        // 물리 접촉으로 지면 판단 — 레이어/반경 설정 불필요
        private void OnCollisionStay2D(Collision2D col)
        {
            foreach (var contact in col.contacts)
            {
                // 법선이 위를 향하면(> 0.7) 지면으로 판단
                if (contact.normal.y > 0.7f)
                {
                    _groundedThisFrame = true;
                    return;
                }
            }
        }

        // ── 코드 제어 API ─────────────────────────────────────────────────

        /// <summary>
        /// 이동 방향을 설정합니다. Movable 값에 관계없이 동작합니다.
        /// Platformer는 x축, TopDown은 xy축 모두 사용됩니다.
        /// FixedUpdate 한 프레임 후 자동 초기화됩니다.
        /// </summary>
        public void Move(Vector2 direction) => _inputDir = direction;

        /// <summary>점프합니다 (Platformer 전용). Movable에 관계없이 동작합니다.</summary>
        public void Jump()
        {
            if (Mode == MovementMode.Platformer) _jumpQueued = true;
        }

        /// <summary>지정 월드 좌표로 즉시 텔레포트합니다.</summary>
        public void Teleport(Vector2 worldPosition)
        {
            _rb.position       = worldPosition;
            _rb.linearVelocity = Vector2.zero;
        }

        /// <summary>Rigidbody2D 속도를 직접 설정합니다 (넉백, 밀치기 등).</summary>
        public void SetVelocity(Vector2 velocity) => _rb.linearVelocity = velocity;

        /// <summary>힘을 가합니다 (기본값: Impulse).</summary>
        public void AddForce(Vector2 force, ForceMode2D forceMode = ForceMode2D.Impulse)
            => _rb.AddForce(force, forceMode);

        /// <summary>이동을 즉시 멈춥니다 (Platformer는 수직 속도 유지).</summary>
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

            if (Mode == MovementMode.Platformer && IsGrounded &&
                (Input.GetButtonDown("Jump") ||
                 Input.GetKeyDown(KeyCode.W) ||
                 Input.GetKeyDown(KeyCode.UpArrow)))
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

            // Platformer — 수평만 덮어쓰고 수직은 물리에 맡김
            _rb.linearVelocity = new Vector2(_inputDir.x * MoveSpeed, _rb.linearVelocity.y);

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
            if (Mode != MovementMode.Platformer || _rb.linearVelocity.y >= 0f) return;

            _rb.linearVelocity += Vector2.up * (Physics2D.gravity.y * (FallMultiplier - 1f) * Time.fixedDeltaTime);

            if (_rb.linearVelocity.y < -MaxFallSpeed)
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, -MaxFallSpeed);
        }
    }

    public enum MovementMode
    {
        /// <summary>중력 적용 — 좌우 이동(A/D) + 점프(Space/W/↑)</summary>
        Platformer,

        /// <summary>중력 없음 — 상하좌우 자유 이동(WASD/방향키)</summary>
        TopDown,
    }
}
