#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif
using UnityEngine;

namespace AchEngine.Movement
{
    /// <summary>
    /// 붙이기만 하면 동작하는 2D 캐릭터 이동 컴포넌트.
    /// Rigidbody2D / CapsuleCollider2D를 자동으로 추가·설정합니다.
    ///
    /// ■ Platformer 모드 — 좌우 이동(A/D) + 점프(Space/W/↑)
    /// ■ TopDown 모드    — 상하좌우 자유 이동(WASD/방향키)
    ///
    /// UseGravity = true  : 중력 + 지면 감지 활성
    /// UseGravity = false : 중력 없음 — TopDown, 횡스크롤 비행, 우주 등
    ///
    /// Movable = true  : 플레이어 입력으로 이동
    /// Movable = false : 입력 차단 — SetVelocity() / AddForce() 등 코드로만 제어
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

        [Tooltip("점프 힘 (UseGravity=true 전용)")]
        public float JumpForce = 12f;

        [Tooltip("Platformer = 좌우 이동 + 점프 / TopDown = 4방향 이동")]
        public MovementMode Mode = MovementMode.Platformer;

        // ── 물리 ──────────────────────────────────────────────────────────

        [Header("Physics")]
        [Tooltip("중력 및 지면 감지 활성 여부 — false로 끄면 어떤 모드든 중력 없이 동작")]
        public bool UseGravity = true;

        [Tooltip("중력 배율 (UseGravity=true 전용)")]
        public float GravityScale = 3f;

        [Tooltip("낙하 중 추가 중력 배율 — 높을수록 낙하가 빠르고 묵직한 느낌 (UseGravity=true 전용)")]
        public float FallMultiplier = 2f;

        [Tooltip("최대 낙하 속도 (UseGravity=true 전용)")]
        public float MaxFallSpeed = 20f;

        // ── 제어 ──────────────────────────────────────────────────────────

        [Header("Control")]
        [Tooltip("true : 플레이어 입력으로 이동\nfalse : 입력 차단, 코드로만 이동 가능")]
        public bool Movable = true;

        [Tooltip("이동 방향에 따라 SpriteRenderer를 자동 좌우 반전")]
        public bool FlipSprite = true;

        // ── 읽기 전용 상태 ────────────────────────────────────────────────

        /// <summary>현재 지면에 닿아 있는지 여부 (UseGravity=true 전용)</summary>
        public bool IsGrounded { get; private set; }

        /// <summary>플레이어 입력 또는 코드에 의해 현재 수평 방향으로 이동 중인지 여부</summary>
        public bool IsMoving { get; private set; }

        /// <summary>현재 Rigidbody2D 속도</summary>
        public Vector2 Velocity => _rb.linearVelocity;

        /// <summary>
        /// 외부 입력 소스 연결용 델리게이트 (온스크린 조이스틱, 커스텀 입력 등).
        /// 설정하면 키보드/게임패드 대신 이 값을 사용합니다.
        /// null로 초기화하면 기본 Input 시스템으로 돌아옵니다.
        /// </summary>
        public System.Func<Vector2> InputProvider { get; set; }

        // ── 내부 ──────────────────────────────────────────────────────────

        private Rigidbody2D       _rb;
        private CapsuleCollider2D _col;
        private SpriteRenderer    _sprite;
        private Vector2           _inputDir;
        private bool              _jumpQueued;
        private int               _groundMask;

        // 콜라이더 바닥보다 살짝 위에서 시작해 페네트레이션으로 인한 미감지 방지
        private const float GroundCheckOriginOffset = 0.05f;
        private const float GroundCheckDistance     = 0.15f;

        // ── Unity 생명주기 ────────────────────────────────────────────────

        private void Awake()
        {
            _rb     = GetComponent<Rigidbody2D>();
            _col    = GetComponent<CapsuleCollider2D>();
            _sprite = GetComponent<SpriteRenderer>();

            // 기존 Rigidbody2D 설정을 완전히 초기화해 외부 설정 충돌 방지
            _rb.bodyType               = RigidbodyType2D.Dynamic;
            _rb.constraints            = RigidbodyConstraints2D.FreezeRotation;
            _rb.gravityScale           = 0f;   // 중력은 AchMover가 직접 적용
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            // Extrapolate 상태이면 velocity 변화 시 렌더 위치가 튀었다 돌아오는 현상 발생
            _rb.interpolation          = RigidbodyInterpolation2D.None;

            // 자기 레이어만 제외 — 지면이 같은 레이어여도 동작
            _groundMask = ~(1 << gameObject.layer);
        }

        private void Update()
        {
            if (Movable) ReadInput();

            if (FlipSprite && _sprite != null && Mathf.Abs(_inputDir.x) > 0.01f)
                _sprite.flipX = _inputDir.x < 0f;
        }

        private void FixedUpdate()
        {
            IsGrounded = CheckGrounded();
            ApplyMovement();
            ApplyGravity();
        }

        private bool CheckGrounded()
        {
            if (!UseGravity) return false;

            var origin = new Vector2(_col.bounds.center.x,
                                     _col.bounds.min.y + GroundCheckOriginOffset);
            return Physics2D.Raycast(origin, Vector2.down, GroundCheckDistance, _groundMask);
        }

        // ── 코드 제어 API ─────────────────────────────────────────────────

        /// <summary>지정 월드 좌표로 즉시 텔레포트합니다.</summary>
        public void Teleport(Vector2 worldPosition)
        {
            _rb.position       = worldPosition;
            _rb.linearVelocity = Vector2.zero;
        }

        /// <summary>점프합니다 (UseGravity=true 전용). Movable에 관계없이 동작합니다.</summary>
        public void Jump()
        {
            if (UseGravity) _jumpQueued = true;
        }

        /// <summary>Rigidbody2D 속도를 직접 설정합니다 (넉백, 밀치기 등).</summary>
        public void SetVelocity(Vector2 velocity) => _rb.linearVelocity = velocity;

        /// <summary>힘을 가합니다 (기본값: Impulse).</summary>
        public void AddForce(Vector2 force, ForceMode2D forceMode = ForceMode2D.Impulse)
            => _rb.AddForce(force, forceMode);

        /// <summary>이동을 즉시 멈춥니다 (Platformer+UseGravity일 때 수직 속도 유지).</summary>
        public void Stop()
        {
            _inputDir = Vector2.zero;
            _rb.linearVelocity = (Mode == MovementMode.Platformer && UseGravity)
                ? new Vector2(0f, _rb.linearVelocity.y)
                : Vector2.zero;
        }

        // ── 내부 로직 ─────────────────────────────────────────────────────

        private void ReadInput()
        {
            float h, v;
            bool jumpPressed;

            if (InputProvider != null)
            {
                var raw  = InputProvider();
                h          = raw.x;
                v          = raw.y;
                jumpPressed = false;
            }
            else
            {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
                var kb = Keyboard.current;
                var gp = Gamepad.current;

                h = 0f;
                v = 0f;
                if (kb != null)
                {
                    if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) h += 1f;
                    if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)  h -= 1f;
                    if (kb.wKey.isPressed || kb.upArrowKey.isPressed)    v += 1f;
                    if (kb.sKey.isPressed || kb.downArrowKey.isPressed)  v -= 1f;
                }
                if (gp != null)
                {
                    h = Mathf.Clamp(h + gp.leftStick.x.ReadValue(), -1f, 1f);
                    v = Mathf.Clamp(v + gp.leftStick.y.ReadValue(), -1f, 1f);
                }

                jumpPressed = (kb != null && (kb.spaceKey.wasPressedThisFrame ||
                                              kb.wKey.wasPressedThisFrame      ||
                                              kb.upArrowKey.wasPressedThisFrame))
                           || (gp != null && gp.buttonSouth.wasPressedThisFrame);
#else
                h          = Input.GetAxisRaw("Horizontal");
                v          = Input.GetAxisRaw("Vertical");
                jumpPressed = Input.GetButtonDown("Jump")       ||
                              Input.GetKeyDown(KeyCode.W)       ||
                              Input.GetKeyDown(KeyCode.UpArrow);
#endif
            }

            _inputDir = Mode == MovementMode.TopDown
                ? new Vector2(h, v).normalized
                : new Vector2(h, 0f);

            if (UseGravity && Mode == MovementMode.Platformer && IsGrounded && jumpPressed)
                _jumpQueued = true;
        }

        private void ApplyMovement()
        {
            if (Mode == MovementMode.TopDown)
            {
                IsMoving           = _inputDir.sqrMagnitude > 0.01f;
                _rb.linearVelocity = _inputDir * MoveSpeed;
                _inputDir          = Vector2.zero;
                return;
            }

            // Platformer — 수평만 덮어쓰고 수직은 물리에 맡김
            IsMoving           = Mathf.Abs(_inputDir.x) > 0.01f;
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

            _inputDir = Vector2.zero;
        }

        private void ApplyGravity()
        {
            if (!UseGravity) return;

            // 기본 중력(GravityScale) + 낙하 중 추가 중력(FallMultiplier) 직접 적용
            // Rigidbody2D.gravityScale = 0 이므로 AchMover가 단독으로 중력을 제어
            float mult = GravityScale + (_rb.linearVelocity.y < 0f ? FallMultiplier - 1f : 0f);
            _rb.linearVelocity += Vector2.up * (Physics2D.gravity.y * mult * Time.fixedDeltaTime);

            if (_rb.linearVelocity.y < -MaxFallSpeed)
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, -MaxFallSpeed);
        }
    }

    public enum MovementMode
    {
        /// <summary>좌우 이동(A/D) + 점프(Space/W/↑)</summary>
        Platformer,

        /// <summary>상하좌우 자유 이동(WASD/방향키)</summary>
        TopDown,
    }
}
