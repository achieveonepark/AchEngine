using System;
using UnityEngine;

namespace AchEngine
{
    /// <summary>Unity Debug API를 사용하는 기본 로그 구현체.</summary>
    internal class AchLog : IAchLog
    {
        /// <summary>현재 로그 필터 수준. 기본값은 Debug(전체 출력)입니다.</summary>
        public LogLevel LogLevel { get; set; } = LogLevel.Debug;

        /// <summary>
        /// 레벨에 맞는 Unity Debug 메서드를 호출하여 메시지를 출력합니다.
        /// Fatal 레벨은 예외를 던집니다.
        /// </summary>
        /// <param name="level">로그 레벨.</param>
        /// <param name="message">출력할 메시지 객체.</param>
        public void Log(LogLevel level, object message)
        {
            string color = level switch
            {
                LogLevel.Info    => "green",
                LogLevel.Debug   => "white",
                LogLevel.Warning => "yellow",
                LogLevel.Error   => "red",
                LogLevel.Fatal   => "red",
                _ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
            };

            switch (level)
            {
                case LogLevel.Debug:
                case LogLevel.Info:
                    Debug.Log($"<color={color}>[{level}] {message}</color>");
                    break;
                case LogLevel.Warning:
                    Debug.LogWarning($"<color={color}>[{level}] {message}</color>");
                    break;
                case LogLevel.Error:
                    Debug.LogError($"<color={color}>[{level}] {message}</color>");
                    break;
                case LogLevel.Fatal:
                    throw new Exception($"[{level}] {message}");
            }
        }
    }
}
