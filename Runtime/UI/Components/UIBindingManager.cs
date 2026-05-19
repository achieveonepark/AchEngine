#if ACHENGINE_R3
using System;
using System.Collections.Concurrent;
using R3;

namespace AchEngine.UI
{
    /// <summary>
    /// R3 기반의 타입-키 메시지 버스입니다. ACHENGINE_R3 심볼이 정의된 경우에만 활성화됩니다.
    /// </summary>
    public static class UIBindingManager
    {
        private static readonly ConcurrentDictionary<Type, object> _subjects = new();

        /// <summary>지정한 타입 <typeparamref name="T"/>의 메시지를 구독합니다.</summary>
        public static IDisposable Subscribe<T>(Action<T> callback)
            => GetSubject<T>().Subscribe(callback);

        /// <summary>지정한 타입 <typeparamref name="T"/>의 메시지를 모든 구독자에게 발행합니다.</summary>
        public static void Publish<T>(T message)
            => GetSubject<T>().OnNext(message);

        /// <summary>모든 Subject를 제거합니다.</summary>
        public static void ClearAll() => _subjects.Clear();

        /// <summary>지정한 타입 <typeparamref name="T"/>에 대한 Subject가 존재하는지 확인합니다.</summary>
        public static bool Contains<T>() => _subjects.ContainsKey(typeof(T));

        private static Subject<T> GetSubject<T>()
            => (Subject<T>)_subjects.GetOrAdd(typeof(T), _ => new Subject<T>());
    }
}
#endif
