#if USE_PUBSUB
using System;
using System.Collections.Concurrent;
using System.Threading;
using UniTaskPubSub;

namespace AchEngine.UI
{
    public class UIBindingManager
    {
        private static UIBindingManager _instance;
        private static readonly object _lock = new();
        private ConcurrentDictionary<Type, AsyncMessageBus> _buses;

        [UnityEngine.RuntimeInitializeOnLoadMethod]
        static void Initialize()
        {
            lock (_lock)
            {
                _instance = new UIBindingManager { _buses = new ConcurrentDictionary<Type, AsyncMessageBus>() };
            }
        }

        public static void Subscribe<T>(Action<T> callback) where T : class
            => GetOrCreate<T>().Subscribe(callback);

        public static async AchTask PublishAsync<T>(T msg, CancellationToken cancellation = default) where T : class
            => await GetOrCreate<T>().PublishAsync(msg, cancellation);

        public static void Publish<T>(T msg, CancellationToken cancellation = default) where T : class
            => GetOrCreate<T>().Publish(msg, cancellation);

        public static void ClearAll() => (_instance ?? new UIBindingManager())._buses?.Clear();

        public static bool Contains<T>() where T : class
            => _instance?._buses.ContainsKey(typeof(T)) ?? false;

        private static AsyncMessageBus GetOrCreate<T>() where T : class
        {
            if (_instance == null) Initialize();
            return _instance._buses.GetOrAdd(typeof(T), _ => new AsyncMessageBus());
        }
    }
}
#endif
