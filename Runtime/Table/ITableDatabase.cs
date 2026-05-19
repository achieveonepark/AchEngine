using System;
using System.Collections.Generic;

namespace AchEngine.Table
{
    /// <summary>타입별 테이블 데이터를 등록하고 조회하는 인터페이스입니다.</summary>
    public interface ITableDatabase
    {
        /// <summary>리스트를 ID 기준 딕셔너리로 변환해 등록합니다.</summary>
        void Register<T>(IReadOnlyList<T> items) where T : ITableData;
        /// <summary>ID로 항목을 가져옵니다. 없으면 default를 반환합니다.</summary>
        T Get<T>(int id) where T : ITableData;
        /// <summary>ID로 항목을 시도해 가져옵니다. 없으면 false를 반환합니다.</summary>
        bool TryGet<T>(int id, out T result) where T : ITableData;
        /// <summary>타입에 해당하는 전체 딕셔너리를 반환합니다.</summary>
        IReadOnlyDictionary<int, T> GetTable<T>() where T : ITableData;
        /// <summary>타입에 해당하는 모든 항목을 열거합니다.</summary>
        IEnumerable<T> GetAll<T>() where T : ITableData;
        /// <summary>등록된 항목 수를 반환합니다.</summary>
        int Count<T>() where T : ITableData;
        /// <summary>지정한 ID의 항목이 존재하는지 확인합니다.</summary>
        bool Contains<T>(int id) where T : ITableData;
        /// <summary>모든 테이블 데이터를 초기화합니다.</summary>
        void Clear();
    }
}
