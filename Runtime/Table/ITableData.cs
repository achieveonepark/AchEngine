namespace AchEngine.Table
{
    /// <summary>테이블 데이터 행의 기본 인터페이스입니다. 모든 테이블 데이터 클래스에 구현합니다.</summary>
    public interface ITableData
    {
        /// <summary>행을 고유하게 식별하는 ID입니다.</summary>
        int Id { get; }
    }
}
