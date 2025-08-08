using System;

namespace CustomerIssuesManager.Core.Services
{
    public enum LogLevel
    {
        Debug,
        Information,
        Warning,
        Error,
        Critical
    }

    public interface ILoggingService
    {
        void LogDebug(string message);
        void LogInformation(string message);
        void LogWarning(string message);
        void LogError(string message, Exception? exception = null);
        void LogCritical(string message, Exception? exception = null);
        void Log(LogLevel level, string message, Exception? exception = null);
    }
} 