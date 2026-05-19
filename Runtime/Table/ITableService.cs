using UnityEngine;

namespace AchEngine.Table
{
    /// <summary>바이너리·TextAsset·JSON 문자열로부터 테이블 데이터를 로드하는 서비스 인터페이스입니다.</summary>
    public interface ITableService
    {
        /// <summary>로드된 데이터를 보관하는 데이터베이스입니다.</summary>
        ITableDatabase Database { get; }
        /// <summary>바이트 배열(MemoryPack 또는 JSON)에서 데이터를 로드해 등록합니다.</summary>
        void Load<T>(byte[] bytes) where T : ITableData;
        /// <summary>TextAsset에서 데이터를 로드해 등록합니다.</summary>
        void Load<T>(TextAsset asset) where T : ITableData;
        /// <summary>JSON 문자열에서 직접 데이터를 로드해 등록합니다.</summary>
        void LoadFromJsonText<T>(string json) where T : ITableData;
    }
}
