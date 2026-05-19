using System;
using System.Threading;

#if ENABLE_UNITASK

using Cysharp.Threading.Tasks;

namespace AchEngine
{
    /// <summary>UniTask/Task 통합 비동기 래퍼 — UniTask 감지 시 UniTask로 동작</summary>
    public readonly struct AchTask
    {
        private readonly UniTask _inner;

        private AchTask(UniTask task) => _inner = task;

        /// <summary>이미 완료된 태스크</summary>
        public static AchTask CompletedTask => new AchTask(UniTask.CompletedTask);

        /// <summary>Unity 스케일 시간 기준 지연 (Time.timeScale 영향 받음)</summary>
        public static AchTask Delay(float seconds, CancellationToken ct = default)
            => new AchTask(UniTask.Delay(TimeSpan.FromSeconds(seconds), DelayType.DeltaTime, PlayerLoopTiming.Update, ct));

        /// <summary>실제 경과 시간 기준 지연 (Time.timeScale 무관)</summary>
        public static AchTask DelayRealtime(float seconds, CancellationToken ct = default)
            => new AchTask(UniTask.Delay(TimeSpan.FromSeconds(seconds), DelayType.UnscaledDeltaTime, PlayerLoopTiming.Update, ct));

        /// <summary>조건이 참이 될 때까지 매 프레임 대기</summary>
        public static AchTask WaitUntil(Func<bool> predicate, CancellationToken ct = default)
            => new AchTask(UniTask.WaitUntil(predicate, PlayerLoopTiming.Update, ct));

        /// <summary>모든 태스크가 완료될 때까지 대기</summary>
        public static AchTask WhenAll(params AchTask[] tasks)
        {
            var inner = new UniTask[tasks.Length];
            for (var i = 0; i < tasks.Length; i++) inner[i] = tasks[i];
            return WhenAllCore(inner);
        }

        /// <summary>가장 먼저 완료되는 태스크 하나가 끝날 때까지 대기</summary>
        public static AchTask WhenAny(params AchTask[] tasks)
        {
            var inner = new UniTask[tasks.Length];
            for (var i = 0; i < tasks.Length; i++) inner[i] = tasks[i];
            return WhenAnyCore(inner);
        }

        private static async UniTask WhenAllCore(UniTask[] tasks) => await UniTask.WhenAll(tasks);
        private static async UniTask WhenAnyCore(UniTask[] tasks) => await UniTask.WhenAny(tasks);

        /// <summary>UniTask 직접 래핑</summary>
        public static AchTask FromUniTask(UniTask task) => new AchTask(task);

        public UniTask.Awaiter GetAwaiter() => _inner.GetAwaiter();

        public static implicit operator AchTask(UniTask task) => new AchTask(task);
        public static implicit operator UniTask(AchTask task) => task._inner;
    }

    /// <summary>반환값이 있는 UniTask/Task 통합 래퍼</summary>
    public readonly struct AchTask<T>
    {
        private readonly UniTask<T> _inner;

        private AchTask(UniTask<T> task) => _inner = task;

        /// <summary>결과값을 즉시 완료 상태로 래핑</summary>
        public static AchTask<T> FromResult(T value) => new AchTask<T>(UniTask.FromResult(value));

        /// <summary>UniTask&lt;T&gt; 직접 래핑</summary>
        public static AchTask<T> FromUniTask(UniTask<T> task) => new AchTask<T>(task);

        public UniTask<T>.Awaiter GetAwaiter() => _inner.GetAwaiter();

        public static implicit operator AchTask<T>(UniTask<T> task) => new AchTask<T>(task);
        public static implicit operator UniTask<T>(AchTask<T> task) => task._inner;
    }
}

#else

using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace AchEngine
{
    /// <summary>UniTask/Task 통합 비동기 래퍼 — UniTask 없을 시 System.Threading.Tasks.Task로 동작</summary>
    public readonly struct AchTask
    {
        private readonly Task _inner;

        private AchTask(Task task) => _inner = task ?? Task.CompletedTask;

        /// <summary>이미 완료된 태스크</summary>
        public static AchTask CompletedTask => new AchTask(Task.CompletedTask);

        /// <summary>
        /// 지연 대기 — Task 폴백에서는 벽시계 기준 (Time.timeScale 무관).
        /// UniTask 환경에서는 Unity 스케일 시간을 사용하므로 동작이 달라짐.
        /// </summary>
        public static AchTask Delay(float seconds, CancellationToken ct = default)
            => new AchTask(Task.Delay(TimeSpan.FromSeconds(seconds), ct));

        /// <summary>실제 경과 시간 기준 지연 (Task 폴백에서는 Delay와 동일)</summary>
        public static AchTask DelayRealtime(float seconds, CancellationToken ct = default)
            => new AchTask(Task.Delay(TimeSpan.FromSeconds(seconds), ct));

        /// <summary>조건이 참이 될 때까지 폴링 대기 (약 16ms 간격)</summary>
        public static AchTask WaitUntil(Func<bool> predicate, CancellationToken ct = default)
            => new AchTask(PollUntil(predicate, ct));

        private static async Task PollUntil(Func<bool> predicate, CancellationToken ct)
        {
            while (!predicate())
            {
                ct.ThrowIfCancellationRequested();
                await Task.Delay(16, ct);
            }
        }

        /// <summary>모든 태스크가 완료될 때까지 대기</summary>
        public static AchTask WhenAll(params AchTask[] tasks)
        {
            var inner = new Task[tasks.Length];
            for (var i = 0; i < tasks.Length; i++) inner[i] = tasks[i].AsTask();
            return Task.WhenAll(inner);
        }

        /// <summary>가장 먼저 완료되는 태스크 하나가 끝날 때까지 대기</summary>
        public static AchTask WhenAny(params AchTask[] tasks)
        {
            var inner = new Task[tasks.Length];
            for (var i = 0; i < tasks.Length; i++) inner[i] = tasks[i].AsTask();
            return Task.WhenAny(inner);
        }

        /// <summary>Task 직접 래핑</summary>
        public static AchTask FromTask(Task task) => new AchTask(task);

        public TaskAwaiter GetAwaiter() => _inner.GetAwaiter();

        public Task AsTask() => _inner;

        public static implicit operator AchTask(Task task) => new AchTask(task);
        public static implicit operator Task(AchTask task) => task._inner;
    }

    /// <summary>반환값이 있는 UniTask/Task 통합 래퍼</summary>
    public readonly struct AchTask<T>
    {
        private readonly Task<T> _inner;

        private AchTask(Task<T> task) => _inner = task;

        /// <summary>결과값을 즉시 완료 상태로 래핑</summary>
        public static AchTask<T> FromResult(T value) => new AchTask<T>(Task.FromResult(value));

        /// <summary>Task&lt;T&gt; 직접 래핑</summary>
        public static AchTask<T> FromTask(Task<T> task) => new AchTask<T>(task);

        public TaskAwaiter<T> GetAwaiter() => _inner.GetAwaiter();

        public Task<T> AsTask() => _inner;

        public static implicit operator AchTask<T>(Task<T> task) => new AchTask<T>(task);
        public static implicit operator Task<T>(AchTask<T> task) => task._inner;
    }
}

#endif
