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
    /// <summary>
    /// 플레이어 데이터 컨테이너를 키-값 형태로 관리하는 매니저.
    /// MemoryPack 직렬화를 지원한다 (USE_QUICK_SAVE 심볼 활성 시).
    /// </summary>
    [MemoryPackable]
    public partial class PlayerManager : IManager
#else
    /// <summary>
    /// 플레이어 데이터 컨테이너를 키-값 형태로 관리하는 매니저.
    /// </summary>
    public class PlayerManager : IManager
#endif
    {
        private readonly Dictionary<string, IPlayerDataContainerBase> _storage = new();

        /// <summary>
        /// 초기화. PlayerManager는 별도 초기화 작업이 없다.
        /// </summary>
        public Task Initialize() => Task.CompletedTask;

        /// <summary>
        /// 플레이어 데이터 컨테이너를 등록한다.
        /// 동일한 키가 이미 존재하면 예외를 발생시킨다.
        /// </summary>
        /// <typeparam name="T">등록할 컨테이너 타입.</typeparam>
        /// <param name="container">등록할 컨테이너 인스턴스.</param>
        public void Add<T>(T container) where T : IPlayerDataContainerBase
        {
            var key = container.DataKey;
            if (_storage.ContainsKey(key))
                throw new InvalidOperationException($"Container '{key}' is already registered.");
            _storage[key] = container;
        }

        /// <summary>
        /// 타입 이름을 키로 사용하여 등록된 컨테이너를 반환한다.
        /// 등록되지 않은 타입이면 예외를 발생시킨다.
        /// </summary>
        /// <typeparam name="T">가져올 컨테이너 타입.</typeparam>
        /// <returns>등록된 컨테이너 인스턴스.</returns>
        public T Get<T>() where T : class, IPlayerDataContainerBase
        {
            var key = typeof(T).Name;
            if (_storage.TryGetValue(key, out var data))
                return data as T;
            throw new KeyNotFoundException($"Container '{key}' is not registered.");
        }

        /// <summary>
        /// 타입 이름을 키로 사용하여 등록된 컨테이너를 제거한다.
        /// 등록되지 않은 타입이면 예외를 발생시킨다.
        /// </summary>
        /// <typeparam name="T">제거할 컨테이너 타입.</typeparam>
        public void Remove<T>() where T : IPlayerDataContainerBase
        {
            var key = typeof(T).Name;
            if (!_storage.Remove(key))
                throw new KeyNotFoundException($"Container '{key}' is not registered.");
        }
    }
}
