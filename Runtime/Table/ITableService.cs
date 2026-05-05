using UnityEngine;

namespace AchEngine.Table
{
    public interface ITableService
    {
        ITableDatabase Database { get; }
        void Load<T>(byte[] bytes) where T : ITableData;
        void Load<T>(TextAsset asset) where T : ITableData;
        void LoadFromJsonText<T>(string json) where T : ITableData;
    }
}
