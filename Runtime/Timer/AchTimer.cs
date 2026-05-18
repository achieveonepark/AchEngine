using System.Threading;
using System.Threading.Tasks;

namespace AchEngine
{
    /// <summary>
    /// async/await 및 실시간 진행 추적을 지원하는 타이머 유틸리티.
    ///
    /// 기본 사용:
    ///   await AchTimer.Wait(3f);
    ///   await AchTimer.WaitRealtime(3f);
    ///
    /// 진행 상황 추적:
    ///   var timer = AchTimer.Start(5f);
    ///   await timer;
    ///   // 또는
    ///   _ = timer.Task;  // 완료를 다른 곳에서 처리
    ///   Debug.Log(timer.Elapsed);     // 경과 시간(초)
    ///   Debug.Log(timer.Remaining);   // 남은 시간(초)
    ///   Debug.Log(timer.Progress);    // 0~1
    ///   timer.Cancel();               // 중단
    /// </summary>
    public static class AchTimer
    {
        /// <summary>
        /// 지정한 시간(초)이 지나면 완료되는 Task를 반환한다. (게임 시간 기준)
        /// </summary>
        public static Task Wait(float seconds, CancellationToken cancellationToken = default)
        {
            if (seconds <= 0f) return Task.CompletedTask;
            return Start(seconds, useUnscaledTime: false, cancellationToken).Task;
        }

        /// <summary>
        /// 지정한 시간(초)이 지나면 완료되는 Task를 반환한다. (실제 시간 기준, Time.timeScale 영향 없음)
        /// </summary>
        public static Task WaitRealtime(float seconds, CancellationToken cancellationToken = default)
        {
            if (seconds <= 0f) return Task.CompletedTask;
            return Start(seconds, useUnscaledTime: true, cancellationToken).Task;
        }

        /// <summary>
        /// 진행 상황을 추적할 수 있는 AchTimerHandle을 생성해 반환한다.
        /// 반환된 핸들은 await 가능하며, Elapsed / Remaining / Progress 프로퍼티로 상태를 조회할 수 있다.
        /// </summary>
        /// <param name="duration">타이머 총 지속 시간(초)</param>
        /// <param name="useUnscaledTime">true면 실제 시간, false면 게임 시간(기본값)</param>
        /// <param name="cancellationToken">외부에서 취소할 때 사용하는 토큰</param>
        public static AchTimerHandle Start(
            float duration,
            bool useUnscaledTime = false,
            CancellationToken cancellationToken = default)
        {
            var handle = new AchTimerHandle(duration, useUnscaledTime, cancellationToken);
            AchTimerRunner.Instance.Register(handle);
            return handle;
        }
    }
}
