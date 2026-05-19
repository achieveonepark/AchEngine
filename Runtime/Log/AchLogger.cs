using System;
using UnityEngine;

namespace AchEngine
{
    /// <summary>AchEngine 전역 로그 유틸리티. 레벨별 편의 메서드를 제공합니다.</summary>
    public static class AchLogger
    {
        private static IAchLog _log;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetDomain() => _log = null;

        /// <summary>로그 필터 수준을 설정합니다. 해당 수준보다 낮은 레벨의 로그는 출력되지 않습니다.</summary>
        /// <param name="level">적용할 로그 레벨.</param>
        public static void SetLogLevel(LogLevel level) => GetOrCreate().LogLevel = level;

        /// <summary>Debug 수준 메시지를 출력합니다.</summary>
        /// <param name="message">출력할 메시지.</param>
        public static void Debug(string message)   => LogBase(LogLevel.Debug,   message);

        /// <summary>Info 수준 메시지를 출력합니다.</summary>
        /// <param name="message">출력할 메시지.</param>
        public static void Info(string message)    => LogBase(LogLevel.Info,    message);

        /// <summary>Warning 수준 메시지를 출력합니다.</summary>
        /// <param name="message">출력할 메시지.</param>
        public static void Warning(string message) => LogBase(LogLevel.Warning, message);

        /// <summary>Error 수준 메시지를 출력합니다.</summary>
        /// <param name="message">출력할 메시지.</param>
        public static void Error(string message)   => LogBase(LogLevel.Error,   message);

        /// <summary>
        /// Fatal 수준으로 기록하고 예외를 던집니다.
        /// 호출 즉시 실행이 중단됩니다.
        /// </summary>
        /// <param name="exception">발생한 예외.</param>
        /// <returns>이 메서드는 항상 예외를 던지며 반환되지 않습니다.</returns>
        public static Exception Fatal(Exception exception) => throw LogBase(LogLevel.Fatal, exception.Message);

        private static Exception LogBase(LogLevel level, string message)
        {
            var log = GetOrCreate();
            if (log.LogLevel < level)
                return null;

            log.Log(level, message);
            return level == LogLevel.Fatal ? new Exception(message) : null;
        }

        private static IAchLog GetOrCreate() => _log ??= new AchLog();
    }
}
