namespace AchEngine
{
    /// <summary>로그 출력 수준을 나타내는 열거형. 값이 낮을수록 심각도가 높습니다.</summary>
    public enum LogLevel
    {
        /// <summary>복구 불가능한 치명적 오류. 예외를 발생시킵니다.</summary>
        Fatal = 0,
        /// <summary>오류 상황. 실행은 계속되지만 문제가 발생했음을 나타냅니다.</summary>
        Error,
        /// <summary>경고 상황. 잠재적 문제를 나타냅니다.</summary>
        Warning,
        /// <summary>일반 정보 메시지.</summary>
        Info,
        /// <summary>디버그용 상세 메시지.</summary>
        Debug
    }
}
