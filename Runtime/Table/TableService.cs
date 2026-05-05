using System;
using System.Collections.Generic;
using UnityEngine;
#if ACHENGINE_MEMORYPACK
using MemoryPack;
#endif

namespace AchEngine.Table
{
    /// <summary>
    /// ITableService 援ы쁽.
    /// MemoryPack???ㅼ튂??寃쎌슦 諛붿씠?덈━ ??쭅?ы솕, ?꾨땶 寃쎌슦 JsonUtility瑜??ъ슜?⑸땲??
    /// VContainer瑜??듯빐 二쇱엯?섍굅?? 吏곸젒 ?앹꽦?섏뿬 ?ъ슜?????덉뒿?덈떎.
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
#if ACHENGINE_MEMORYPACK
            var items = MemoryPackSerializer.Deserialize<List<T>>(bytes);
#else
            var json = System.Text.Encoding.UTF8.GetString(bytes);
            var items = LoadFromJson<T>(json);
#endif
            _database.Register(items);
        }

        public void Load<T>(TextAsset asset) where T : ITableData
        {
#if ACHENGINE_MEMORYPACK
            Load<T>(asset.bytes);
#else
            var items = LoadFromJson<T>(asset.text);
            _database.Register(items);
#endif
        }

        public void LoadFromJsonText<T>(string json) where T : ITableData
        {
            var items = LoadFromJson<T>(json);
            _database.Register(items);
        }

        private static List<T> LoadFromJson<T>(string json) where T : ITableData
        {
            var wrapped = $"{{\"Items\":{json}}}";
            var container = JsonUtility.FromJson<JsonListWrapper<T>>(wrapped);
            return container?.Items ?? new List<T>();
        }

        [Serializable]
        private class JsonListWrapper<T>
        {
            public List<T> Items;
        }
    }
}
