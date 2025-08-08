using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CustomerIssuesManager.Core.Services
{
    public interface ISettingsService
    {
        // Attachment Settings
        string GetAttachmentManagementMode();
        void SetAttachmentManagementMode(string mode);
        string GetCustomAttachmentsFolder();
        void SetCustomAttachmentsFolder(string folderPath);
        bool GetOrganizeByDate();
        void SetOrganizeByDate(bool organizeByDate);
        
        // UI Settings
        bool GetDarkMode();
        void SetDarkMode(bool darkMode);
        bool GetShowNotifications();
        void SetShowNotifications(bool showNotifications);
        string GetLanguage();
        void SetLanguage(string language);
        
        // Backup Settings
        bool GetAutoBackup();
        void SetAutoBackup(bool autoBackup);
        int GetBackupCount();
        void SetBackupCount(int count);
        int GetBackupRetention();
        void SetBackupRetention(int days);
        
        // Database Settings
        string GetDatabasePath();
        void SetDatabasePath(string path);
        
        // Import/Export
        Task<bool> ExportSettingsAsync(string filePath);
        Task<bool> ImportSettingsAsync(string filePath);
        Task<string> GetSettingsAsJsonAsync();
        Task<bool> LoadSettingsFromJsonAsync(string json);
        
        // Validation
        bool ValidateSettings();
        List<string> GetValidationErrors();
        
        // Events
        event EventHandler<SettingsChangedEventArgs> SettingsChanged;
    }

    public class SettingsChangedEventArgs : EventArgs
    {
        public string SettingName { get; set; } = string.Empty;
        public object? OldValue { get; set; }
        public object? NewValue { get; set; }
        public string Category { get; set; } = string.Empty;
    }
} 