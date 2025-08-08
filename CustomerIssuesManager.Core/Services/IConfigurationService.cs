namespace CustomerIssuesManager.Core.Services
{
    public interface IConfigurationService
    {
        string GetConnectionString();
        string GetBackupPath();
        int GetMaxBackupsToKeep();
        string GetLogPath();
        bool GetEnableLogging();
        void SetConnectionString(string connectionString);
        void SetBackupPath(string backupPath);
        void SetMaxBackupsToKeep(int maxBackups);
        void SetLogPath(string logPath);
        void SetEnableLogging(bool enableLogging);
        void SaveConfiguration();
        void LoadConfiguration();
    }
} 