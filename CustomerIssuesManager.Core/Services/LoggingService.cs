using System;
using System.IO;
using System.Text;

namespace CustomerIssuesManager.Core.Services
{
    public class LoggingService : ILoggingService
    {
        private readonly string _logFilePath;
        private readonly object _lockObject = new object();

        public LoggingService()
        {
            var logDir = Path.Combine(AppContext.BaseDirectory, "logs");
            Directory.CreateDirectory(logDir);
            _logFilePath = Path.Combine(logDir, $"app_{DateTime.Now:yyyy-MM-dd}.log");
        }

        public void LogDebug(string message) => Log(LogLevel.Debug, message);
        public void LogInformation(string message) => Log(LogLevel.Information, message);
        public void LogWarning(string message) => Log(LogLevel.Warning, message);
        public void LogError(string message, Exception? exception = null) => Log(LogLevel.Error, message, exception);
        public void LogCritical(string message, Exception? exception = null) => Log(LogLevel.Critical, message, exception);

        public void Log(LogLevel level, string message, Exception? exception = null)
        {
            var logEntry = new StringBuilder();
            logEntry.AppendLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}");
            
            if (exception != null)
            {
                logEntry.AppendLine($"Exception: {exception.Message}");
                logEntry.AppendLine($"StackTrace: {exception.StackTrace}");
            }

            var logMessage = logEntry.ToString();

            // Write to debug output
            System.Diagnostics.Debug.WriteLine(logMessage);

            // Write to file
            try
            {
                lock (_lockObject)
                {
                    File.AppendAllText(_logFilePath, logMessage, Encoding.UTF8);
                }
            }
            catch (Exception ex)
            {
                // If we can't write to log file, at least write to debug output
                System.Diagnostics.Debug.WriteLine($"Failed to write to log file: {ex.Message}");
            }
        }
    }
} 