using System;
using UnityEngine;

namespace AchEngine
{
    public static class AchLogger
    {
        private static IAchLog _log;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetDomain() => _log = null;

        public static void SetLogLevel(LogLevel level) => GetOrCreate().LogLevel = level;

        public static void Debug(string message)   => LogBase(LogLevel.Debug,   message);
        public static void Info(string message)    => LogBase(LogLevel.Info,    message);
        public static void Warning(string message) => LogBase(LogLevel.Warning, message);
        public static void Error(string message)   => LogBase(LogLevel.Error,   message);

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
