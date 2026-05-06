namespace AchEngine
{
    public interface IAchLog
    {
        LogLevel LogLevel { get; set; }
        void Log(LogLevel level, object message);
    }
}
