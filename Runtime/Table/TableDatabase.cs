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
            var dict = new Dictionary<int, T>(items.Count);
            foreach (var item in items)
                dict[item.Id] = item;
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
