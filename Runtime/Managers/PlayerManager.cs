using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AchEngine.Player;
#if USE_QUICK_SAVE
using MemoryPack;
using Newtonsoft.Json;
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

#if USE_QUICK_SAVE
        [MemoryPackInclude]
        [MemoryPackOrder(0)]
        private List<PlayerDataContainerRecord> SerializedContainers
        {
            get
            {
                var records = new List<PlayerDataContainerRecord>(_storage.Count);
                foreach (var pair in _storage)
                {
                    var container = pair.Value;
                    var type = container.GetType();
                    try
                    {
                        records.Add(new PlayerDataContainerRecord
                        {
                            DataKey = pair.Key,
                            TypeName = type.AssemblyQualifiedName,
                            Json = JsonConvert.SerializeObject(container, type, Formatting.None, null),
                        });
                    }
                    catch (JsonException exception)
                    {
                        throw new InvalidDataException(
                            $"플레이어 컨테이너 '{pair.Key}'을(를) 직렬화하지 못했습니다.", exception);
                    }
                }
                return records;
            }
            set => RestoreContainers(value);
        }
#endif

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
            AddContainer(container, nameof(container));
        }

        /// <summary>
        /// 타입 이름을 키로 사용하여 등록된 컨테이너를 반환한다.
        /// 등록되지 않은 타입이면 예외를 발생시킨다.
        /// </summary>
        /// <typeparam name="T">가져올 컨테이너 타입.</typeparam>
        /// <returns>등록된 컨테이너 인스턴스.</returns>
        public T Get<T>() where T : class, IPlayerDataContainerBase
        {
            var key = FindKey<T>();
            return (T)_storage[key];
        }

        /// <summary>
        /// 타입 이름을 키로 사용하여 등록된 컨테이너를 제거한다.
        /// 등록되지 않은 타입이면 예외를 발생시킨다.
        /// </summary>
        /// <typeparam name="T">제거할 컨테이너 타입.</typeparam>
        public void Remove<T>() where T : IPlayerDataContainerBase
        {
            _storage.Remove(FindKey<T>());
        }

        private string FindKey<T>() where T : IPlayerDataContainerBase
        {
            var typeName = typeof(T).Name;
            if (_storage.TryGetValue(typeName, out var exact) && exact is T)
                return typeName;

            string foundKey = null;
            foreach (var pair in _storage)
            {
                if (pair.Value is not T) continue;
                if (foundKey != null)
                    throw new InvalidOperationException(
                        $"Container type '{typeName}' is ambiguous. Register only one container per type.");
                foundKey = pair.Key;
            }

            return foundKey ?? throw new KeyNotFoundException($"Container '{typeName}' is not registered.");
        }

        private void AddContainer(IPlayerDataContainerBase container, string parameterName)
        {
            if (container == null) throw new ArgumentNullException(parameterName);

            var key = container.DataKey;
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("DataKey는 비어 있을 수 없습니다.", parameterName);
            if (!_storage.TryAdd(key, container))
                throw new InvalidOperationException($"컨테이너 '{key}'은(는) 이미 등록되어 있습니다.");
        }

#if USE_QUICK_SAVE
        private void RestoreContainers(IReadOnlyList<PlayerDataContainerRecord> records)
        {
            var restored = new Dictionary<string, IPlayerDataContainerBase>();
            if (records == null)
            {
                _storage.Clear();
                return;
            }

            foreach (var record in records)
            {
                if (record == null)
                    throw new InvalidDataException("플레이어 저장 데이터에 null 컨테이너 레코드가 있습니다.");
                if (string.IsNullOrWhiteSpace(record.DataKey))
                    throw new InvalidDataException("플레이어 저장 데이터의 DataKey가 비어 있습니다.");
                if (string.IsNullOrWhiteSpace(record.TypeName))
                    throw new InvalidDataException($"컨테이너 '{record.DataKey}'의 타입 이름이 비어 있습니다.");
                if (record.Json == null)
                    throw new InvalidDataException($"컨테이너 '{record.DataKey}'의 JSON 데이터가 없습니다.");

                var type = Type.GetType(record.TypeName, throwOnError: false);
                if (type == null || type.IsAbstract || !typeof(IPlayerDataContainerBase).IsAssignableFrom(type))
                    throw new InvalidDataException(
                        $"컨테이너 '{record.DataKey}'의 타입 '{record.TypeName}'을(를) 복원할 수 없습니다.");

                IPlayerDataContainerBase container;
                try
                {
                    container = JsonConvert.DeserializeObject(record.Json, type) as IPlayerDataContainerBase;
                }
                catch (JsonException exception)
                {
                    throw new InvalidDataException(
                        $"컨테이너 '{record.DataKey}'의 JSON 데이터를 역직렬화하지 못했습니다.", exception);
                }

                if (container == null)
                    throw new InvalidDataException($"컨테이너 '{record.DataKey}'의 역직렬화 결과가 null입니다.");
                if (!restored.TryAdd(record.DataKey, container))
                    throw new InvalidDataException($"플레이어 저장 데이터에 중복 DataKey '{record.DataKey}'이(가) 있습니다.");
            }

            _storage.Clear();
            foreach (var pair in restored)
                _storage.Add(pair.Key, pair.Value);
        }

        [MemoryPackable]
        private partial class PlayerDataContainerRecord
        {
            [MemoryPackOrder(0)] public string DataKey { get; set; }
            [MemoryPackOrder(1)] public string TypeName { get; set; }
            [MemoryPackOrder(2)] public string Json { get; set; }
        }
#endif
    }
}
