using UnityEngine;

namespace AchEngine
{
    /// <summary>
    /// 지정한 대상을 향해 이동하는 추적 컴포넌트.
    /// AchMover와 무관하게 독립적으로 동작하며, transform.position을 직접 이동합니다.
    /// </summary>
    public class AchFollower : MonoBehaviour
    {
        [Header("Follow")]
        [Tooltip("따라갈 대상 Transform.")]
        public Transform Target;

        [Tooltip("이동 속도 (Units/sec).")]
        public float MoveSpeed = 5f;

        [Tooltip("이 거리 이하이면 정지.")]
        public float StopDistance = 0.5f;

        void FixedUpdate()
        {
            if (Target == null) return;

            Vector2 delta = (Vector2)Target.position - (Vector2)transform.position;
            if (delta.magnitude <= StopDistance) return;

            transform.position = (Vector2)transform.position + delta.normalized * MoveSpeed * Time.fixedDeltaTime;
        }

        /// <summary>
        /// 추적 대상을 설정합니다.
        /// </summary>
        public void SetTarget(Transform target)
        {
            Target = target;
        }

        /// <summary>
        /// 추적 대상을 해제합니다.
        /// </summary>
        public void ClearTarget()
        {
            Target = null;
        }
    }
}
