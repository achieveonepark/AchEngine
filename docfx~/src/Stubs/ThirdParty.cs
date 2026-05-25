#pragma warning disable CS0067, CS0169, CS0414, CS1591
using System;

// ──────────────────────────────────────────────────────────────
// 서드파티 패키지 최소 스텁 — DocFX 메타데이터 생성 전용
// ──────────────────────────────────────────────────────────────

// VContainer
namespace VContainer
{
    public interface IContainerBuilder { IRegistrationBuilder Register<T>(Lifetime lifetime); IRegistrationBuilder RegisterInstance<T>(T instance); }
    public interface IRegistrationBuilder { IRegistrationBuilder As<T>(); IRegistrationBuilder AsSelf(); }
    public interface IObjectResolver { T Resolve<T>(); }
    public enum Lifetime { Singleton, Transient, Scoped }
}
namespace VContainer.Unity
{
    public abstract class LifetimeScope : UnityEngine.MonoBehaviour { protected abstract void Configure(VContainer.IContainerBuilder builder); }
    public interface IInitializable { void Initialize(); }
    public interface IStartable { void Start(); }
    public interface ITickable { void Tick(); }
}

// MemoryPack
namespace MemoryPack
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class MemoryPackableAttribute : Attribute {}
    [AttributeUsage(AttributeTargets.Constructor)]
    public class MemoryPackConstructorAttribute : Attribute {}
    public static class MemoryPackSerializer
    {
        public static byte[] Serialize<T>(T value) => default;
        public static T Deserialize<T>(ReadOnlySpan<byte> buffer) => default;
        public static T Deserialize<T>(byte[] buffer) => default;
    }
}

// Newtonsoft.Json (minimal)
namespace Newtonsoft.Json
{
    public static class JsonConvert
    {
        public static T DeserializeObject<T>(string value) => default;
        public static string SerializeObject(object value) => default;
    }
}

// R3
namespace R3
{
    public abstract class Observable<T> { public IDisposable Subscribe(Action<T> onNext) => default; }
    public class Subject<T> : Observable<T> { public void OnNext(T value) {} public void OnCompleted() {} }
    public class ReactiveProperty<T> : Observable<T> { public T Value; public ReactiveProperty(T v = default) {} }
}

// UniTask
#if ENABLE_UNITASK
namespace Cysharp.Threading.Tasks
{
    public struct UniTask
    {
        public static UniTask CompletedTask => default;
        public static UniTask Delay(TimeSpan t, DelayType d = DelayType.DeltaTime, PlayerLoopTiming timing = PlayerLoopTiming.Update, System.Threading.CancellationToken ct = default) => default;
        public static UniTask WhenAll(params UniTask[] tasks) => default;
        public static UniTask<int> WhenAny(params UniTask[] tasks) => default;
        public static UniTask WaitUntil(Func<bool> predicate, PlayerLoopTiming timing = PlayerLoopTiming.Update, System.Threading.CancellationToken ct = default) => default;
        public static UniTask<T> FromResult<T>(T value) => default;
        public Awaiter GetAwaiter() => default;
        public struct Awaiter : System.Runtime.CompilerServices.INotifyCompletion
        {
            public bool IsCompleted => true;
            public void GetResult() {}
            public void OnCompleted(Action continuation) {}
        }
    }
    public struct UniTask<T>
    {
        public UniTask<T>.Awaiter GetAwaiter() => default;
        public struct Awaiter : System.Runtime.CompilerServices.INotifyCompletion
        {
            public bool IsCompleted => true;
            public T GetResult() => default;
            public void OnCompleted(Action continuation) {}
        }
        public static implicit operator UniTask(UniTask<T> t) => default;
    }
    public enum DelayType { DeltaTime, UnscaledDeltaTime, Realtime }
    public enum PlayerLoopTiming { Update, FixedUpdate, LateUpdate, PreUpdate }
}
#endif
