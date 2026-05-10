using UnityEngine;

namespace AchEngine
{
    /// <summary>
    /// Moves this GameObject toward <see cref="Target"/> every FixedUpdate.
    /// Completely independent — works on any GameObject with or without AchMover.
    ///
    /// <list type="bullet">
    ///   <item><b>With AchMover:</b> feeds direction through <c>AchMover.InputProvider</c>.
    ///         AchMover handles collision, gravity, and slopes; movement speed comes from
    ///         <c>AchMover.MoveSpeed</c>.</item>
    ///   <item><b>Without AchMover:</b> moves via <c>Rigidbody2D.MovePosition</c> if a
    ///         Rigidbody2D is present, otherwise directly sets <c>transform.position</c>.
    ///         Movement speed comes from <see cref="MoveSpeed"/>.</item>
    /// </list>
    /// </summary>
    public class AchFollower : MonoBehaviour
    {
        [Header("Follow")]
        [Tooltip("The Transform to follow. Assign the player (or any other target) here.")]
        public Transform Target;

        [Tooltip("Movement speed (Units/sec). Only used when AchMover is absent.")]
        public float MoveSpeed = 5f;

        [Tooltip("Stops moving when distance to target is at or below this value.")]
        public float StopDistance = 0.5f;

        // ── optional integrations ─────────────────────────────────────────────
        private AchMover    _mover;
        private Rigidbody2D _rb;

        // ─────────────────────────────────────────────────────────────────────

        void Awake()
        {
            _mover = GetComponent<AchMover>();
            _rb    = GetComponent<Rigidbody2D>();

            if (_mover != null)
                _mover.InputProvider = GetDirection;
        }

        void OnDestroy()
        {
            // Clean up InputProvider so AchMover returns to keyboard input
            if (_mover != null && _mover.InputProvider == (System.Func<Vector2>)GetDirection)
                _mover.InputProvider = null;
        }

        void FixedUpdate()
        {
            // When AchMover is present it reads GetDirection() via InputProvider — nothing to do here.
            if (_mover != null) return;

            // ── standalone movement ──────────────────────────────────────────
            Vector2 dir = GetDirection();
            if (dir == Vector2.zero) return;

            Vector2 next = (Vector2)transform.position + dir * MoveSpeed * Time.fixedDeltaTime;

            if (_rb != null)
                _rb.MovePosition(next);
            else
                transform.position = next;
        }

        // Returns normalised direction to target, or zero when close enough / no target.
        Vector2 GetDirection()
        {
            if (Target == null) return Vector2.zero;

            Vector2 delta = (Vector2)Target.position - (Vector2)transform.position;
            if (delta.magnitude <= StopDistance) return Vector2.zero;

            return delta.normalized;
        }
    }
}
