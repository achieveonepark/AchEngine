namespace AchEngine.Player
{
    /// <summary>플레이어 데이터 컨테이너의 저장 키를 제공하는 기본 인터페이스입니다.</summary>
    public interface IPlayerDataContainerBase
    {
        /// <summary>저장·불러오기에 사용할 고유 키.</summary>
        string DataKey { get; }
    }
}