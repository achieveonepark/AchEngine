using System;
using System.Collections.Generic;
using AchEngine.Player;
#if ACHENGINE_UNITASK
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif
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

#if ACHENGINE_UNITASK
        public UniTask Initialize() => UniTask.CompletedTask;
#else
        public Task Initialize() => Task.CompletedTask;
#endif

        public void AddContainer<T>(T container) where T : IPlayerDataContainerBase
        {
            var key = container.DataKey;
            if (_storage.ContainsKey(key))
                throw new InvalidOperationException($"Container '{key}' is already registered.");
            _storage[key] = container;
        }

        public T GetContainer<T>() where T : class, IPlayerDataContainerBase
        {
            var key = typeof(T).Name;
            if (_storage.TryGetValue(key, out var data))
                return data as T;
            throw new KeyNotFoundException($"Container '{key}' is not registered.");
        }

        public void RemoveContainer<T>() where T : IPlayerDataContainerBase
        {
            var key = typeof(T).Name;
            if (!_storage.Remove(key))
                throw new KeyNotFoundException($"Container '{key}' is not registered.");
        }

#if USE_QUICK_SAVE
        private readonly QuickSave _quickSave = new();

        public void Save() => _quickSave.Save(this);
        public PlayerManager Load() => _quickSave.Load();
#endif
    }
}
