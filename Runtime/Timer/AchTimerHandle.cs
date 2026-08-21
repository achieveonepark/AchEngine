using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace AchEngine
{
    /// <summary>
    /// AchTimer.Start()가 반환하는 타이머 핸들.
    /// await로 완료를 기다리거나, 다른 코드에서 경과/진행률을 실시간으로 조회할 수 있다.
    /// </summary>
    public sealed class AchTimerHandle
    {
        private readonly TaskCompletionSource<bool> _tcs = new();
        private readonly CancellationToken _ct;
        private CancellationTokenRegistration _cancellationRegistration;

        /// <summary>타이머의 총 지속 시간(초).</summary>
        public float Duration { get; }

        /// <summary>현재까지 경과한 시간(초).</summary>
        public float Elapsed { get; internal set; }

        /// <summary>남은 시간(초). 0 미만이 되지 않는다.</summary>
        public float Remaining => Mathf.Max(0f, Duration - Elapsed);

        /// <summary>완료 비율 (0 = 시작, 1 = 완료).</summary>
        public float Progress => Duration > 0f ? Mathf.Clamp01(Elapsed / Duration) : 1f;

        /// <summary>타이머가 완료됐는지 여부 (완료·취소 모두 true).</summary>
        public bool IsDone { get; private set; }

        /// <summary>취소 여부.</summary>
        public bool IsCancelled { get; private set; }

        /// <summary>UnscaledTime 사용 여부 (내부 전용).</summary>
        internal bool UseUnscaledTime { get; }

        /// <summary>
        /// await handle.Task; 형태로 사용하거나 Task를 직접 참조할 때 사용한다.
        /// </summary>
        public Task Task => _tcs.Task;

        internal AchTimerHandle(float duration, bool useUnscaledTime, CancellationToken ct)
        {
            if (float.IsNaN(duration) || float.IsInfinity(duration) || duration < 0f)
                throw new ArgumentOutOfRangeException(nameof(duration), "타이머 지속 시간은 0 이상의 유한한 값이어야 합니다.");

            Duration = duration;
            UseUnscaledTime = useUnscaledTime;
            _ct = ct;

            if (ct.IsCancellationRequested)
            {
                Cancel();
                return;
            }

            if (duration <= 0f)
            {
                Complete();
                return;
            }

            _cancellationRegistration = ct.Register(Cancel);
            if (IsDone)
                _cancellationRegistration.Dispose();
        }

        /// <summary>
        /// await handle; 를 직접 쓸 수 있도록 GetAwaiter를 노출한다.
        /// </summary>
        public TaskAwaiter GetAwaiter() => Task.GetAwaiter();

        /// <summary>타이머를 수동으로 취소한다.</summary>
        public void Cancel()
        {
            if (IsDone) return;
            IsCancelled = true;
            IsDone = true;
            if (!_ct.IsCancellationRequested)
                _cancellationRegistration.Dispose();
            _tcs.TrySetCanceled();
        }

        /// <summary>프레임마다 Runner가 호출하는 내부 업데이트. true를 반환하면 타이머 완료.</summary>
        internal bool Tick(float deltaTime)
        {
            if (_ct.IsCancellationRequested)
            {
                Cancel();
                return true;
            }

            if (IsDone) return true;

            if (float.IsNaN(deltaTime) || float.IsInfinity(deltaTime) || deltaTime < 0f)
                throw new ArgumentOutOfRangeException(nameof(deltaTime), "경과 시간은 0 이상의 유한한 값이어야 합니다.");

            Elapsed += deltaTime;

            if (Elapsed >= Duration)
            {
                Complete();
                return true;
            }

            return false;
        }

        private void Complete()
        {
            if (IsDone) return;
            Elapsed = Duration;
            IsDone = true;
            _cancellationRegistration.Dispose();
            _tcs.TrySetResult(true);
        }
    }
}
