namespace AchEngine
{
    /// <summary>AchEngine 로그 출력을 정의하는 인터페이스.</summary>
    public interface IAchLog
    {
        /// <summary>현재 로그 필터 수준. 이 수준보다 낮은 레벨의 로그는 출력되지 않습니다.</summary>
        LogLevel LogLevel { get; set; }

        /// <summary>
        /// 지정한 레벨로 메시지를 출력합니다.
        /// </summary>
        /// <param name="level">로그 레벨.</param>
        /// <param name="message">출력할 메시지 객체.</param>
        void Log(LogLevel level, object message);
    }
}
