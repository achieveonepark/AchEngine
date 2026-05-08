#if ACHENGINE_R3
using System;
using System.Collections.Concurrent;
using R3;

namespace AchEngine.UI
{
    public static class UIBindingManager
    {
        private static readonly ConcurrentDictionary<Type, object> _subjects = new();

        public static IDisposable Subscribe<T>(Action<T> callback)
            => GetSubject<T>().Subscribe(callback);

        public static void Publish<T>(T message)
            => GetSubject<T>().OnNext(message);

        public static void ClearAll() => _subjects.Clear();

        public static bool Contains<T>() => _subjects.ContainsKey(typeof(T));

        private static Subject<T> GetSubject<T>()
            => (Subject<T>)_subjects.GetOrAdd(typeof(T), _ => new Subject<T>());
    }
}
#endif
