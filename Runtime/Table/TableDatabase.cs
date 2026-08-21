using System;
using System.Collections.Generic;

namespace AchEngine.Table
{
    /// <summary>ITableDatabase의 기본 구현체입니다. 타입별 딕셔너리로 데이터를 관리합니다.</summary>
    public class TableDatabase : ITableDatabase
    {
        private readonly Dictionary<Type, object> _tables = new();

        public void Register<T>(IReadOnlyList<T> items) where T : ITableData
        {
            if (items == null) throw new ArgumentNullException(nameof(items));

            var dict = new Dictionary<int, T>(items.Count);
            foreach (var item in items)
            {
                if (item is null)
                    throw new ArgumentException($"'{typeof(T).Name}' 테이블에 null 행이 있습니다.", nameof(items));
                if (!dict.TryAdd(item.Id, item))
                    throw new ArgumentException(
                        $"'{typeof(T).Name}' 테이블에 중복 Id '{item.Id}'이(가) 있습니다.",
                        nameof(items));
            }
            _tables[typeof(T)] = dict;
        }

        public T Get<T>(int id) where T : ITableData
        {
            if (_tables.TryGetValue(typeof(T), out var table))
            {
                var dict = (Dictionary<int, T>)table;
                if (dict.TryGetValue(id, out var item))
                    return item;
            }
            return default;
        }

        public bool TryGet<T>(int id, out T result) where T : ITableData
        {
            result = default;
            if (!_tables.TryGetValue(typeof(T), out var table))
                return false;
            return ((Dictionary<int, T>)table).TryGetValue(id, out result);
        }

        public IReadOnlyDictionary<int, T> GetTable<T>() where T : ITableData
        {
            if (_tables.TryGetValue(typeof(T), out var table))
                return (Dictionary<int, T>)table;
            return new Dictionary<int, T>();
        }

        public IEnumerable<T> GetAll<T>() where T : ITableData
        {
            if (_tables.TryGetValue(typeof(T), out var table))
                return ((Dictionary<int, T>)table).Values;
            return Array.Empty<T>();
        }

        public int Count<T>() where T : ITableData
        {
            if (_tables.TryGetValue(typeof(T), out var table))
                return ((Dictionary<int, T>)table).Count;
            return 0;
        }

        public bool Contains<T>(int id) where T : ITableData
        {
            if (_tables.TryGetValue(typeof(T), out var table))
                return ((Dictionary<int, T>)table).ContainsKey(id);
            return false;
        }

        public void Clear()
        {
            _tables.Clear();
        }
    }
}
