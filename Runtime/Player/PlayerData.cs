namespace AchEngine.Player
{
    /// <summary>
    /// 플레이어의 기본 데이터 컨테이너입니다.
    /// partial 클래스로 선언되어 있어 게임별로 추가 프로퍼티를 확장할 수 있습니다.
    /// </summary>
    public partial class PlayerData
    {
        /// <summary>플레이어 고유 ID.</summary>
        public int Id { get; set; }
        /// <summary>플레이어 이름.</summary>
        public string Name { get; set; }
        /// <summary>플레이어 레벨.</summary>
        public int Level { get; set; }
    }
}
