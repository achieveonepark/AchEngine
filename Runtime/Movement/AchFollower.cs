using UnityEngine;

namespace AchEngine
{
    /// <summary>
    /// Makes this GameObject follow a target Transform using AchMover.
    /// Set <see cref="Target"/> in the Inspector (or via code) and adjust
    /// <see cref="AchMover.MoveSpeed"/> on the same GameObject to control follow speed.
    /// </summary>
    [RequireComponent(typeof(AchMover))]
    public class AchFollower : MonoBehaviour
    {
        [Header("Follow")]
        [Tooltip("The Transform to follow. Assign the player (or any target) here.")]
        public Transform Target;

        [Tooltip("Distance at which the follower stops moving toward the target.")]
        public float StopDistance = 0.5f;

        private AchMover _mover;

        void Awake()
        {
            _mover = GetComponent<AchMover>();
            _mover.Movable = false; // hand full control to AchFollower
        }

        void FixedUpdate()
        {
            if (Target == null)
            {
                _mover.Stop();
                return;
            }

            float dist = Vector2.Distance(transform.position, Target.position);
            if (dist > StopDistance)
            {
                Vector2 dir = ((Vector2)Target.position - (Vector2)transform.position).normalized;

                // Platformer: gravity controls Y, so only override X.
                // TopDown: move freely in all directions.
                Vector2 vel = _mover.Mode == MovementMode.Platformer
                    ? new Vector2(dir.x * _mover.MoveSpeed, _mover.Velocity.y)
                    : dir * _mover.MoveSpeed;

                _mover.SetVelocity(vel);
            }
            else
            {
                _mover.Stop();
            }
        }
    }
}
