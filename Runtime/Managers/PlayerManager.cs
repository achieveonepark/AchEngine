using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AchEngine.Player;
#if USE_QUICK_SAVE
using MemoryPack;
#endif

namespace AchEngine.Managers
{
#if USE_QUICK_SAVE
    [MemoryPackable]
    public partial class PlayerManager : IManager
#else
    public class PlayerManager : IManager
#endif
    {
        private readonly Dictionary<string, IPlayerDataContainerBase> _storage = new();

        public Task Initialize() => Task.CompletedTask;

        public void Add<T>(T container) where T : IPlayerDataContainerBase
        {
            var key = container.DataKey;
            if (_storage.ContainsKey(key))
                throw new InvalidOperationException($"Container '{key}' is already registered.");
            _storage[key] = container;
        }

        public T Get<T>() where T : class, IPlayerDataContainerBase
        {
            var key = typeof(T).Name;
            if (_storage.TryGetValue(key, out var data))
                return data as T;
            throw new KeyNotFoundException($"Container '{key}' is not registered.");
        }

        public void Remove<T>() where T : IPlayerDataContainerBase
        {
            var key = typeof(T).Name;
            if (!_storage.Remove(key))
                throw new KeyNotFoundException($"Container '{key}' is not registered.");
        }
    }
}
