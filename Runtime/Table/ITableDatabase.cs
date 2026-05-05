using System;
using System.Collections.Generic;

namespace AchEngine.Table
{
    public interface ITableDatabase
    {
        void Register<T>(IReadOnlyList<T> items) where T : ITableData;
        T Get<T>(int id) where T : ITableData;
        bool TryGet<T>(int id, out T result) where T : ITableData;
        IReadOnlyDictionary<int, T> GetTable<T>() where T : ITableData;
        IEnumerable<T> GetAll<T>() where T : ITableData;
        int Count<T>() where T : ITableData;
        bool Contains<T>(int id) where T : ITableData;
        void Clear();
    }
}
