using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
#if ACHENGINE_MEMORYPACK
using MemoryPack;
#endif

namespace AchEngine.Table
{
    /// <summary>
    /// ITableService 기본 구현입니다.
    /// MemoryPack이 설치된 경우 바이너리 직렬화를, 아닌 경우 Newtonsoft.Json을 사용합니다.
    /// VContainer를 통해 주입하거나 직접 생성하여 사용할 수 있습니다.
    /// </summary>
    public class TableService : ITableService
    {
        private readonly ITableDatabase _database;

        public ITableDatabase Database => _database;

        public TableService(ITableDatabase database)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
        }

        public void Load<T>(byte[] bytes) where T : ITableData
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));
            if (bytes.Length == 0) throw new ArgumentException("테이블 데이터가 비어 있습니다.", nameof(bytes));

#if ACHENGINE_MEMORYPACK
            var items = MemoryPackSerializer.Deserialize<List<T>>(bytes) ?? new List<T>();
#else
            var json = System.Text.Encoding.UTF8.GetString(bytes);
            var items = LoadFromJson<T>(json);
#endif
            _database.Register(items);
        }

        public void Load<T>(TextAsset asset) where T : ITableData
        {
            if (asset == null) throw new ArgumentNullException(nameof(asset));

#if ACHENGINE_MEMORYPACK
            Load<T>(asset.bytes);
#else
            var items = LoadFromJson<T>(asset.text);
            _database.Register(items);
#endif
        }

        public void LoadFromJsonText<T>(string json) where T : ITableData
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new ArgumentException("JSON 테이블 데이터가 비어 있습니다.", nameof(json));

            var items = LoadFromJson<T>(json);
            _database.Register(items);
        }

        private static List<T> LoadFromJson<T>(string json) where T : ITableData
        {
            try
            {
                return JsonConvert.DeserializeObject<List<T>>(json)
                       ?? throw new JsonSerializationException("JSON 루트가 null입니다.");
            }
            catch (JsonException e)
            {
                throw new InvalidOperationException($"'{typeof(T).Name}' 테이블 JSON을 읽을 수 없습니다.", e);
            }
        }
    }
}
