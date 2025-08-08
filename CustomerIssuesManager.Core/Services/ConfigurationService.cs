using System;
using System.IO;
using System.Text.Json;

namespace CustomerIssuesManager.Core.Services
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly string _configFilePath;
        private AppConfiguration _configuration;

        public ConfigurationService()
        {
            _configFilePath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            _configuration = new AppConfiguration();
            LoadConfiguration();
        }

        public string GetConnectionString() => _configuration.ConnectionString;
        public string GetBackupPath() => _configuration.BackupPath;
        public int GetMaxBackupsToKeep() => _configuration.MaxBackupsToKeep;
        public string GetLogPath() => _configuration.LogPath;
        public bool GetEnableLogging() => _configuration.EnableLogging;

        public void SetConnectionString(string connectionString) => _configuration.ConnectionString = connectionString;
        public void SetBackupPath(string backupPath) => _configuration.BackupPath = backupPath;
        public void SetMaxBackupsToKeep(int maxBackups) => _configuration.MaxBackupsToKeep = maxBackups;
        public void SetLogPath(string logPath) => _configuration.LogPath = logPath;
        public void SetEnableLogging(bool enableLogging) => _configuration.EnableLogging = enableLogging;

        public void SaveConfiguration()
        {
            try
            {
                var json = JsonSerializer.Serialize(_configuration, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_configFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save configuration: {ex.Message}");
            }
        }

        public void LoadConfiguration()
        {
            try
            {
                if (File.Exists(_configFilePath))
                {
                    var json = File.ReadAllText(_configFilePath);
                    _configuration = JsonSerializer.Deserialize<AppConfiguration>(json) ?? new AppConfiguration();
                }
                else
                {
                    // Create default configuration
                    _configuration = new AppConfiguration
                    {
                        ConnectionString = "Data Source=CustomerIssues.db",
                        BackupPath = Path.Combine(AppContext.BaseDirectory, "backups"),
                        MaxBackupsToKeep = 10,
                        LogPath = Path.Combine(AppContext.BaseDirectory, "logs"),
                        EnableLogging = true
                    };
                    SaveConfiguration();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load configuration: {ex.Message}");
                // Use default configuration if loading fails
                _configuration = new AppConfiguration
                {
                    ConnectionString = "Data Source=CustomerIssues.db",
                    BackupPath = Path.Combine(AppContext.BaseDirectory, "backups"),
                    MaxBackupsToKeep = 10,
                    LogPath = Path.Combine(AppContext.BaseDirectory, "logs"),
                    EnableLogging = true
                };
            }
        }

        private class AppConfiguration
        {
            public string ConnectionString { get; set; } = "Data Source=CustomerIssues.db";
            public string BackupPath { get; set; } = "backups";
            public int MaxBackupsToKeep { get; set; } = 10;
            public string LogPath { get; set; } = "logs";
            public bool EnableLogging { get; set; } = true;
        }
    }
} 