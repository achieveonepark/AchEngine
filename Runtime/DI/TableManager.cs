using System.Collections.Generic;
using AchEngine.Table;

namespace AchEngine.DI
{
    /// <summary>
    /// Static accessor for ITableService.
    /// VContainer ?ъ슜 ??AchEngineScope媛 ?먮룞?쇰줈 ?ㅼ젙?⑸땲??
    /// VContainer ?놁씠 ?ъ슜??寃쎌슦 ?섎룞?쇰줈 SetService瑜??몄텧?섏꽭??
    /// </summary>
    public static class TableManager
    {
        private static ITableService _service;
        private static TableService _fallback;

        public static ITableService Current => _service ?? FallbackService;

        private static ITableService FallbackService
        {
            get
            {
                if (_fallback == null)
                {
                    _fallback = new TableService(new TableDatabase());
                }
                return _fallback;
            }
        }

        public static void SetService(ITableService service) => _service = service;

        public static void Load<T>(byte[] bytes) where T : ITableData
            => Current.Load<T>(bytes);

        public static void Load<T>(UnityEngine.TextAsset asset) where T : ITableData
            => Current.Load<T>(asset);

        public static void LoadFromJsonText<T>(string json) where T : ITableData
            => Current.LoadFromJsonText<T>(json);

        public static T Get<T>(int id) where T : ITableData
            => Current.Database.Get<T>(id);

        public static bool TryGet<T>(int id, out T result) where T : ITableData
            => Current.Database.TryGet(id, out result);

        public static IEnumerable<T> GetAll<T>() where T : ITableData
            => Current.Database.GetAll<T>();

        public static void Reset()
        {
            _service = null;
            _fallback = null;
        }
    }
}
