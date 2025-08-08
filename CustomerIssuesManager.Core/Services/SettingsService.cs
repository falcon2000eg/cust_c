using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace CustomerIssuesManager.Core.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly string _settingsFilePath;
        private AppSettings _settings;
        private readonly List<string> _validationErrors;

        public event EventHandler<SettingsChangedEventArgs>? SettingsChanged;

        public SettingsService()
        {
            _settingsFilePath = Path.Combine(AppContext.BaseDirectory, "settings.json");
            _validationErrors = new List<string>();
            _settings = new AppSettings();
            LoadSettings();
        }

        #region Attachment Settings
        public string GetAttachmentManagementMode() => _settings.AttachmentManagementMode;
        
        public void SetAttachmentManagementMode(string mode)
        {
            var oldValue = _settings.AttachmentManagementMode;
            _settings.AttachmentManagementMode = mode;
            SaveSettings();
            OnSettingsChanged("AttachmentManagementMode", oldValue, mode, "Attachment");
        }
        
        public string GetCustomAttachmentsFolder() => _settings.CustomAttachmentsFolder;
        
        public void SetCustomAttachmentsFolder(string folderPath)
        {
            var oldValue = _settings.CustomAttachmentsFolder;
            _settings.CustomAttachmentsFolder = folderPath;
            SaveSettings();
            OnSettingsChanged("CustomAttachmentsFolder", oldValue, folderPath, "Attachment");
        }
        
        public bool GetOrganizeByDate() => _settings.OrganizeByDate;
        
        public void SetOrganizeByDate(bool organizeByDate)
        {
            var oldValue = _settings.OrganizeByDate;
            _settings.OrganizeByDate = organizeByDate;
            SaveSettings();
            OnSettingsChanged("OrganizeByDate", oldValue, organizeByDate, "Attachment");
        }
        #endregion

        #region UI Settings
        public bool GetDarkMode() => _settings.DarkMode;
        
        public void SetDarkMode(bool darkMode)
        {
            var oldValue = _settings.DarkMode;
            _settings.DarkMode = darkMode;
            SaveSettings();
            OnSettingsChanged("DarkMode", oldValue, darkMode, "UI");
        }
        
        public bool GetShowNotifications() => _settings.ShowNotifications;
        
        public void SetShowNotifications(bool showNotifications)
        {
            var oldValue = _settings.ShowNotifications;
            _settings.ShowNotifications = showNotifications;
            SaveSettings();
            OnSettingsChanged("ShowNotifications", oldValue, showNotifications, "UI");
        }
        
        public string GetLanguage() => _settings.Language;
        
        public void SetLanguage(string language)
        {
            var oldValue = _settings.Language;
            _settings.Language = language;
            SaveSettings();
            OnSettingsChanged("Language", oldValue, language, "UI");
        }
        #endregion

        #region Backup Settings
        public bool GetAutoBackup() => _settings.AutoBackup;
        
        public void SetAutoBackup(bool autoBackup)
        {
            var oldValue = _settings.AutoBackup;
            _settings.AutoBackup = autoBackup;
            SaveSettings();
            OnSettingsChanged("AutoBackup", oldValue, autoBackup, "Backup");
        }
        
        public int GetBackupCount() => _settings.BackupCount;
        
        public void SetBackupCount(int count)
        {
            var oldValue = _settings.BackupCount;
            _settings.BackupCount = count;
            SaveSettings();
            OnSettingsChanged("BackupCount", oldValue, count, "Backup");
        }
        
        public int GetBackupRetention() => _settings.BackupRetention;
        
        public void SetBackupRetention(int days)
        {
            var oldValue = _settings.BackupRetention;
            _settings.BackupRetention = days;
            SaveSettings();
            OnSettingsChanged("BackupRetention", oldValue, days, "Backup");
        }
        #endregion

        #region Database Settings
        public string GetDatabasePath() => _settings.DatabasePath;
        
        public void SetDatabasePath(string path)
        {
            var oldValue = _settings.DatabasePath;
            _settings.DatabasePath = path;
            SaveSettings();
            OnSettingsChanged("DatabasePath", oldValue, path, "Database");
        }
        #endregion

        #region Import/Export
        public async Task<bool> ExportSettingsAsync(string filePath)
        {
            try
            {
                var json = await GetSettingsAsJsonAsync();
                await File.WriteAllTextAsync(filePath, json);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to export settings: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ImportSettingsAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return false;

                var json = await File.ReadAllTextAsync(filePath);
                return await LoadSettingsFromJsonAsync(json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to import settings: {ex.Message}");
                return false;
            }
        }

        public async Task<string> GetSettingsAsJsonAsync()
        {
            return await Task.Run(() => JsonSerializer.Serialize(_settings, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            }));
        }

        public async Task<bool> LoadSettingsFromJsonAsync(string json)
        {
            try
            {
                var importedSettings = await Task.Run(() => JsonSerializer.Deserialize<AppSettings>(json));
                if (importedSettings != null)
                {
                    _settings = importedSettings;
                    SaveSettings();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load settings from JSON: {ex.Message}");
                return false;
            }
        }
        #endregion

        #region Validation
        public bool ValidateSettings()
        {
            _validationErrors.Clear();

            // Validate attachment settings
            if (string.IsNullOrEmpty(_settings.AttachmentManagementMode))
                _validationErrors.Add("وضع إدارة المرفقات مطلوب");

            if (_settings.AttachmentManagementMode == "MoveToFolder" && string.IsNullOrEmpty(_settings.CustomAttachmentsFolder))
                _validationErrors.Add("مسار المرفقات المخصص مطلوب عند اختيار وضع النقل إلى مجلد");

            // Validate backup settings
            if (_settings.BackupCount < 1 || _settings.BackupCount > 100)
                _validationErrors.Add("عدد النسخ الاحتياطية يجب أن يكون بين 1 و 100");

            if (_settings.BackupRetention < 1 || _settings.BackupRetention > 365)
                _validationErrors.Add("فترة الاحتفاظ بالنسخ الاحتياطية يجب أن تكون بين 1 و 365 يوم");

            // Validate database path
            if (string.IsNullOrEmpty(_settings.DatabasePath))
                _validationErrors.Add("مسار قاعدة البيانات مطلوب");

            return _validationErrors.Count == 0;
        }

        public List<string> GetValidationErrors() => new List<string>(_validationErrors);
        #endregion

        #region Private Methods
        private void LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    var json = File.ReadAllText(_settingsFilePath);
                    _settings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
                else
                {
                    // Create default settings
                    _settings = new AppSettings
                    {
                        AttachmentManagementMode = "KeepOriginal",
                        CustomAttachmentsFolder = Path.Combine(AppContext.BaseDirectory, "CustomAttachments"),
                        OrganizeByDate = true,
                        DarkMode = false,
                        ShowNotifications = true,
                        Language = "ar",
                        AutoBackup = true,
                        BackupCount = 10,
                        BackupRetention = 30,
                        DatabasePath = Path.Combine(AppContext.BaseDirectory, "CustomerIssues.db")
                    };
                    SaveSettings();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load settings: {ex.Message}");
                // Use default settings if loading fails
                _settings = new AppSettings();
            }
        }

        private void SaveSettings()
        {
            try
            {
                var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_settingsFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save settings: {ex.Message}");
            }
        }

        private void OnSettingsChanged(string settingName, object? oldValue, object? newValue, string category)
        {
            SettingsChanged?.Invoke(this, new SettingsChangedEventArgs
            {
                SettingName = settingName,
                OldValue = oldValue,
                NewValue = newValue,
                Category = category
            });
        }
        #endregion

        #region Settings Model
        private class AppSettings
        {
            // Attachment Settings
            public string AttachmentManagementMode { get; set; } = "KeepOriginal";
            public string CustomAttachmentsFolder { get; set; } = "";
            public bool OrganizeByDate { get; set; } = true;

            // UI Settings
            public bool DarkMode { get; set; } = false;
            public bool ShowNotifications { get; set; } = true;
            public string Language { get; set; } = "ar";

            // Backup Settings
            public bool AutoBackup { get; set; } = true;
            public int BackupCount { get; set; } = 10;
            public int BackupRetention { get; set; } = 30;

            // Database Settings
            public string DatabasePath { get; set; } = "";
        }
        #endregion
    }
} 