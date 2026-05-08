#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif
using UnityEngine;

namespace AchEngine.Movement
{
    /// <summary>
    /// 붙이기만 하면 동작하는 2D 캐릭터 이동 컴포넌트.
    /// Rigidbody2D 없이 자체 충돌 처리(Move-and-Slide, 경사면, 계단, 지면 스냅)를 수행합니다.
    /// CapsuleCollider2D만 자동으로 추가되며, 별도 설정이 필요 없습니다.
    ///
    /// ■ Platformer 모드 — 좌우 이동(A/D) + 점프(Space/W/↑)
    /// ■ TopDown 모드    — 상하좌우 자유 이동(WASD/방향키)
    ///
    /// UseGravity = true  : 중력 + 지면 감지 + 경사면 + 계단
    /// UseGravity = false : 중력 없음 — TopDown, 비행, 우주 등
    ///
    /// Movable = true  : 플레이어 입력으로 이동
    /// Movable = false : 입력 차단 — SetVelocity() / AddForce() 등 코드로만 제어
    /// </summary>
    [AddComponentMenu("AchEngine/Movement/Ach Mover")]
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

        [Tooltip("낙하 중 추가 중력 배율 — 높을수록 낙하가 빠르고 묵직한 느낌")]
        public float FallMultiplier = 2f;

        [Tooltip("최대 낙하 속도")]
        public float MaxFallSpeed = 20f;

        // ── 경사면 / 계단 ────────────────────────────────────────────────

        [Header("Slopes & Stairs")]
        [Tooltip("등반 가능한 최대 경사 각도(°). 이를 넘으면 벽으로 처리")]
        [Range(0f, 89f)]
        public float MaxSlopeAngle = 50f;

        [Tooltip("자동으로 올라갈 수 있는 최대 계단/턱 높이 (Units)")]
        public float StepHeight = 0.3f;

        // ── 제어 ──────────────────────────────────────────────────────────

        [Header("Control")]
        [Tooltip("true : 플레이어 입력으로 이동\nfalse : 입력 차단, 코드로만 이동 가능")]
        public bool Movable = true;

        [Tooltip("이동 방향에 따라 SpriteRenderer를 자동 좌우 반전")]
        public bool FlipSprite = true;

        // ── 읽기 전용 상태 ────────────────────────────────────────────────

        /// <summary>현재 지면에 닿아 있는지 여부 (UseGravity=true 전용)</summary>
        public bool IsGrounded { get; private set; }

        /// <summary>플레이어 입력 또는 코드에 의해 현재 이동 중인지 여부</summary>
        public bool IsMoving { get; private set; }

        /// <summary>현재 속도</summary>
        public Vector2 Velocity => _velocity;

        /// <summary>지면에 서 있을 때 그 표면의 법선 (없으면 Vector2.up)</summary>
        public Vector2 GroundNormal => _groundNormal;

        /// <summary>
        /// 외부 입력 소스 연결용 델리게이트 (온스크린 조이스틱, 커스텀 입력 등).
        /// 설정하면 키보드/게임패드 대신 이 값을 사용합니다.
        /// null로 초기화하면 기본 Input 시스템으로 돌아옵니다.
        /// </summary>
        public System.Func<Vector2> InputProvider { get; set; }

        // ── 내부 ──────────────────────────────────────────────────────────

        private CapsuleCollider2D _col;
        private SpriteRenderer    _sprite;
        private Vector2           _inputDir;
        private Vector2           _velocity;
        private bool              _jumpQueued;
        private Vector2           _groundNormal = Vector2.up;
        private ContactFilter2D   _filter;

        private readonly RaycastHit2D[] _hitBuf     = new RaycastHit2D[8];
        private readonly Collider2D[]   _overlapBuf = new Collider2D[8];

        // 콜라이더가 표면에 딱 붙어 끼는 현상 방지용 여유
        private const float SkinWidth          = 0.015f;
        // 매 프레임 지면 감지를 위해 아래로 쏘는 거리
        private const float GroundProbeDistance = 0.1f;
        // Move-and-Slide 시 한 프레임에 시도하는 최대 슬라이드 횟수
        private const int   MaxSlideIterations  = 4;
        // 보정 후에도 남은 미세 페네트레이션을 밀어내는 최대 시도 횟수
        private const int   MaxDepenetrationIterations = 3;

        // ── Unity 생명주기 ────────────────────────────────────────────────

        private void Awake()
        {
            _col    = GetComponent<CapsuleCollider2D>();
            _sprite = GetComponent<SpriteRenderer>();

            // 자기 레이어만 제외해 자기 콜라이더와 충돌하지 않도록
            _filter = new ContactFilter2D
            {
                useLayerMask = true,
                layerMask    = ~(1 << gameObject.layer),
                useTriggers  = false,
            };

            // 같은 GameObject에 Rigidbody2D가 붙어 있다면 transform 이동과 충돌하므로 비활성
            // (사용자가 의도적으로 두었을 수 있으므로 파괴하지 않고 Simulated만 끔)
            var rb = GetComponent<Rigidbody2D>();
            if (rb != null) rb.simulated = false;
        }

        private void Update()
        {
            if (Movable) ReadInput();

            if (FlipSprite && _sprite != null && Mathf.Abs(_inputDir.x) > 0.01f)
                _sprite.flipX = _inputDir.x < 0f;
        }

        private void FixedUpdate()
        {
            // 1. 입력·중력 → 이번 프레임 속도 결정
            ComputeVelocity();

            // 2. 이동 + 슬라이드 + 계단 처리
            Vector2 motion = _velocity * Time.fixedDeltaTime;
            MoveAndSlide(ref motion);

            // 3. 지면 검사 (이동 후)
            IsGrounded = ProbeGround();

            // 4. 경사면 따라 내려갈 때 지면 스냅
            if (UseGravity && _velocity.y <= 0.01f && IsGrounded)
                SnapToGround();

            // 5. 잔여 페네트레이션 보정
            Depenetrate();

            _inputDir = Vector2.zero;
        }

        // ── 코드 제어 API ─────────────────────────────────────────────────

        /// <summary>지정 월드 좌표로 즉시 텔레포트합니다.</summary>
        public void Teleport(Vector2 worldPosition)
        {
            transform.position = worldPosition;
            _velocity          = Vector2.zero;
        }

        /// <summary>점프합니다 (UseGravity=true 전용). Movable에 관계없이 동작합니다.</summary>
        public void Jump()
        {
            if (UseGravity) _jumpQueued = true;
        }

        /// <summary>속도를 직접 설정합니다 (넉백, 밀치기 등).</summary>
        public void SetVelocity(Vector2 velocity) => _velocity = velocity;

        /// <summary>
        /// 힘을 즉시 속도에 반영합니다. Rigidbody2D를 쓰지 않으므로 Impulse만 의미가 있으며,
        /// 매 프레임 누적해 적용하려면 직접 호출해야 합니다.
        /// </summary>
        public void AddForce(Vector2 force, ForceMode2D forceMode = ForceMode2D.Impulse)
        {
            _velocity += force;
        }

        /// <summary>이동을 즉시 멈춥니다 (Platformer+UseGravity일 때 수직 속도 유지).</summary>
        public void Stop()
        {
            _inputDir = Vector2.zero;
            _velocity = (Mode == MovementMode.Platformer && UseGravity)
                ? new Vector2(0f, _velocity.y)
                : Vector2.zero;
        }

        // ── 내부 로직 ─────────────────────────────────────────────────────

        private void ReadInput()
        {
            float h, v;
            bool  jumpPressed;

            if (InputProvider != null)
            {
                var raw     = InputProvider();
                h           = raw.x;
                v           = raw.y;
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
                h           = Input.GetAxisRaw("Horizontal");
                v           = Input.GetAxisRaw("Vertical");
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

        private void ComputeVelocity()
        {
            if (Mode == MovementMode.TopDown)
            {
                _velocity = _inputDir * MoveSpeed;
                IsMoving  = _inputDir.sqrMagnitude > 0.01f;

                // TopDown에 UseGravity = true인 특수 케이스: 자유 낙하만 추가
                if (UseGravity)
                {
                    float g = Physics2D.gravity.y * GravityScale * Time.fixedDeltaTime;
                    _velocity.y = Mathf.Max(_velocity.y + g, -MaxFallSpeed);
                }
                return;
            }

            // Platformer
            _velocity.x = _inputDir.x * MoveSpeed;
            IsMoving    = Mathf.Abs(_inputDir.x) > 0.01f;

            if (UseGravity)
            {
                if (IsGrounded && _velocity.y < 0f)
                    _velocity.y = 0f;   // 지면 위 미세 진동 방지

                float mult = GravityScale + (_velocity.y < 0f ? FallMultiplier - 1f : 0f);
                _velocity.y += Physics2D.gravity.y * mult * Time.fixedDeltaTime;

                if (_velocity.y < -MaxFallSpeed) _velocity.y = -MaxFallSpeed;
            }

            if (_jumpQueued)
            {
                if (IsGrounded) _velocity.y = JumpForce;
                _jumpQueued = false;
            }
        }

        // 캐릭터를 motion만큼 이동. 충돌 시 표면을 따라 슬라이드, 막히면 계단 시도.
        private void MoveAndSlide(ref Vector2 motion)
        {
            for (int i = 0; i < MaxSlideIterations && motion.sqrMagnitude > 1e-8f; i++)
            {
                Vector2 dir  = motion.normalized;
                float   dist = motion.magnitude;

                if (!CastClosest(dir, dist + SkinWidth, out var hit))
                {
                    // 충돌 없음 — 끝까지 이동
                    transform.position = (Vector2)transform.position + motion;
                    return;
                }

                float allowed = Mathf.Max(0f, hit.distance - SkinWidth);
                transform.position = (Vector2)transform.position + dir * allowed;

                Vector2 remaining = motion - dir * allowed;

                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                bool  isWall     = slopeAngle > MaxSlopeAngle;

                // 벽에 부딪혔고 지면 위라면 계단 오르기 시도
                if (isWall &&
                    Mode == MovementMode.Platformer &&
                    IsGrounded &&
                    Mathf.Abs(hit.normal.y) < 0.5f &&
                    Mathf.Abs(remaining.x) > 1e-4f)
                {
                    if (TryStepUp(remaining))
                        return;
                }

                // 표면을 따라 슬라이드 — 속도와 잔여 모션 모두 투영
                _velocity = SlideAlong(_velocity, hit.normal);
                motion    = SlideAlong(remaining, hit.normal);
            }
        }

        // 작은 턱·계단을 위로 한 번에 오르는 처리.
        // 1) 위로 StepHeight만큼 시도 → 2) 가로 모션 시도 → 3) 다시 아래로 내려와 평지 확인
        private bool TryStepUp(Vector2 remaining)
        {
            Vector3 origin = transform.position;

            // (1) 위로 올라갈 수 있는 만큼 올라간다
            float upDist = StepHeight;
            if (CastClosest(Vector2.up, StepHeight + SkinWidth, out var upHit))
                upDist = Mathf.Max(0f, upHit.distance - SkinWidth);

            if (upDist < SkinWidth) return false;

            transform.position = origin + (Vector3)(Vector2.up * upDist);

            // (2) 가로로 남은 만큼 이동 시도
            Vector2 horizontal = new Vector2(remaining.x, 0f);
            float   hMag       = horizontal.magnitude;
            Vector2 hDir       = horizontal / hMag;

            float hAllowed = hMag;
            if (CastClosest(hDir, hMag + SkinWidth, out var fwdHit))
                hAllowed = Mathf.Max(0f, fwdHit.distance - SkinWidth);

            if (hAllowed < 0.01f)
            {
                transform.position = origin;
                return false;
            }

            transform.position = (Vector2)transform.position + hDir * hAllowed;

            // (3) 발 밑에 디딜 곳이 있는지 — StepHeight 이내에 walkable 표면 필요
            if (!CastClosest(Vector2.down, StepHeight + SkinWidth * 2f, out var downHit))
            {
                transform.position = origin;   // 허공이면 계단 아님
                return false;
            }

            float surfaceAngle = Vector2.Angle(downHit.normal, Vector2.up);
            if (surfaceAngle > MaxSlopeAngle)
            {
                transform.position = origin;
                return false;
            }

            float dnDist = Mathf.Max(0f, downHit.distance - SkinWidth);
            transform.position = (Vector2)transform.position + Vector2.down * dnDist;
            return true;
        }

        private bool ProbeGround()
        {
            if (!UseGravity) return false;

            if (!CastClosest(Vector2.down, GroundProbeDistance + SkinWidth, out var hit))
            {
                _groundNormal = Vector2.up;
                return false;
            }

            float angle = Vector2.Angle(hit.normal, Vector2.up);
            if (angle > MaxSlopeAngle)
            {
                _groundNormal = Vector2.up;
                return false;
            }

            _groundNormal = hit.normal;
            return true;
        }

        // 경사면을 따라 내려갈 때 표면에 붙어 떨어지지 않도록 끌어내림
        private void SnapToGround()
        {
            if (!CastClosest(Vector2.down, GroundProbeDistance, out var hit)) return;

            float angle = Vector2.Angle(hit.normal, Vector2.up);
            if (angle > MaxSlopeAngle) return;

            float dist = Mathf.Max(0f, hit.distance - SkinWidth);
            if (dist > 0.001f)
                transform.position = (Vector2)transform.position + Vector2.down * dist;
        }

        // 다른 콜라이더와 겹친 상태로 끝났을 경우 강제로 밀어냄
        private void Depenetrate()
        {
            for (int iter = 0; iter < MaxDepenetrationIterations; iter++)
            {
                int count = Physics2D.OverlapCollider(_col, _filter, _overlapBuf);
                if (count == 0) return;

                bool moved = false;
                for (int i = 0; i < count; i++)
                {
                    var d = _col.Distance(_overlapBuf[i]);
                    if (!d.isValid || !d.isOverlapped) continue;

                    // d.distance는 음수, d.normal은 B→A 방향
                    float push = -d.distance + SkinWidth;
                    transform.position = (Vector2)transform.position + d.normal * push;
                    moved = true;
                }
                if (!moved) return;
            }
        }

        // ── 유틸 ──────────────────────────────────────────────────────────

        private bool CastClosest(Vector2 direction, float distance, out RaycastHit2D closest)
        {
            int count = _col.Cast(direction, _filter, _hitBuf, distance);
            if (count == 0)
            {
                closest = default;
                return false;
            }

            int best = 0;
            for (int i = 1; i < count; i++)
                if (_hitBuf[i].distance < _hitBuf[best].distance) best = i;

            closest = _hitBuf[best];
            return true;
        }

        private static Vector2 SlideAlong(Vector2 v, Vector2 normal)
            => v - Vector2.Dot(v, normal) * normal;
    }

    public enum MovementMode
    {
        /// <summary>좌우 이동(A/D) + 점프(Space/W/↑)</summary>
        Platformer,

        /// <summary>상하좌우 자유 이동(WASD/방향키)</summary>
        TopDown,
    }
}
