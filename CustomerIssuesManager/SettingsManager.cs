using CustomerIssuesManager.Core.Services;
using CustomerIssuesManager.Services;
using Microsoft.Win32;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace CustomerIssuesManager
{
    public class SettingsManager
    {
        private readonly ISettingsService _settingsService;
        private readonly NotificationService _notificationService;

        public SettingsManager(ISettingsService settingsService, Window parentWindow)
        {
            _settingsService = settingsService;
            _notificationService = new NotificationService(parentWindow);
            
            // Subscribe to settings changes for notifications
            _settingsService.SettingsChanged += SettingsService_SettingsChanged;
        }

        private void SettingsService_SettingsChanged(object? sender, SettingsChangedEventArgs e)
        {
            // Show notifications for attachment settings changes
            if (e.Category == "Attachment" && _settingsService.GetShowNotifications())
            {
                string message = GetSettingsChangeMessage(e.SettingName, e.OldValue, e.NewValue);
                _notificationService.ShowNotification(message, NotificationType.Info);
            }
        }

        private string GetSettingsChangeMessage(string settingName, object? oldValue, object? newValue)
        {
            return settingName switch
            {
                "AttachmentManagementMode" => $"تم تغيير وضع إدارة المرفقات من '{oldValue}' إلى '{newValue}'",
                "CustomAttachmentsFolder" => $"تم تغيير مجلد المرفقات المخصص إلى: {newValue}",
                "OrganizeByDate" => $"تم {((bool)newValue! ? "تفعيل" : "إلغاء")} تنظيم المرفقات حسب التاريخ",
                _ => $"تم تغيير إعداد: {settingName}"
            };
        }

        public async Task<bool> ExportSettingsAsync()
        {
            try
            {
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Title = "تصدير الإعدادات",
                    Filter = "ملفات JSON|*.json|جميع الملفات|*.*",
                    FileName = $"settings_{DateTime.Now:yyyyMMdd_HHmmss}.json"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    bool success = await _settingsService.ExportSettingsAsync(saveFileDialog.FileName);
                    if (success)
                    {
                        _notificationService.ShowNotification("تم تصدير الإعدادات بنجاح", NotificationType.Success);
                        return true;
                    }
                    else
                    {
                        _notificationService.ShowNotification("فشل في تصدير الإعدادات", NotificationType.Error);
                        return false;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification($"خطأ في تصدير الإعدادات: {ex.Message}", NotificationType.Error);
                return false;
            }
        }

        public async Task<bool> ImportSettingsAsync()
        {
            try
            {
                var openFileDialog = new Microsoft.Win32.OpenFileDialog
                {
                    Title = "استيراد الإعدادات",
                    Filter = "ملفات JSON|*.json|جميع الملفات|*.*"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    bool success = await _settingsService.ImportSettingsAsync(openFileDialog.FileName);
                    if (success)
                    {
                        _notificationService.ShowNotification("تم استيراد الإعدادات بنجاح", NotificationType.Success);
                        return true;
                    }
                    else
                    {
                        _notificationService.ShowNotification("فشل في استيراد الإعدادات", NotificationType.Error);
                        return false;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification($"خطأ في استيراد الإعدادات: {ex.Message}", NotificationType.Error);
                return false;
            }
        }

        public bool ValidateAndSaveSettings()
        {
            try
            {
                // Validate settings before saving
                if (!_settingsService.ValidateSettings())
                {
                    var errors = _settingsService.GetValidationErrors();
                    var errorMessage = "أخطاء في الإعدادات:\n\n" + string.Join("\n", errors);
                    _notificationService.ShowNotification(errorMessage, NotificationType.Error);
                    return false;
                }

                _notificationService.ShowNotification("تم حفظ الإعدادات بنجاح", NotificationType.Success);
                return true;
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification($"خطأ في حفظ الإعدادات: {ex.Message}", NotificationType.Error);
                return false;
            }
        }

        public void LoadSettingsToUI(dynamic settingsWindow)
        {
            try
            {
                // Load database settings
                settingsWindow.DatabasePathTextBox.Text = _settingsService.GetDatabasePath();
                
                // Load attachment settings
                string currentMode = _settingsService.GetAttachmentManagementMode();
                if (currentMode == "MoveToFolder")
                {
                    settingsWindow.AttachmentManagementComboBox.SelectedIndex = 1;
                    settingsWindow.CustomFolderPanel.Visibility = Visibility.Visible;
                }
                else
                {
                    settingsWindow.AttachmentManagementComboBox.SelectedIndex = 0;
                }
                
                settingsWindow.CustomAttachmentsFolderTextBox.Text = _settingsService.GetCustomAttachmentsFolder();
                if (string.IsNullOrEmpty(settingsWindow.CustomAttachmentsFolderTextBox.Text))
                {
                    settingsWindow.CustomAttachmentsFolderTextBox.Text = Path.Combine(AppContext.BaseDirectory, "CustomAttachments");
                }
                
                settingsWindow.OrganizeByDateCheckBox.IsChecked = _settingsService.GetOrganizeByDate();
                
                // Load UI settings
                settingsWindow.DarkModeCheckBox.IsChecked = _settingsService.GetDarkMode();
                settingsWindow.ShowNotificationsCheckBox.IsChecked = _settingsService.GetShowNotifications();
                
                // Load backup settings
                settingsWindow.AutoBackupCheckBox.IsChecked = _settingsService.GetAutoBackup();
                settingsWindow.BackupCountTextBox.Text = _settingsService.GetBackupCount().ToString();
                settingsWindow.BackupRetentionTextBox.Text = _settingsService.GetBackupRetention().ToString();
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification($"خطأ في تحميل الإعدادات: {ex.Message}", NotificationType.Error);
            }
        }

        public void SaveSettingsFromUI(dynamic settingsWindow)
        {
            try
            {
                // Save attachment settings
                _settingsService.SetAttachmentManagementMode(
                    settingsWindow.AttachmentManagementComboBox.SelectedIndex == 1 ? "MoveToFolder" : "KeepOriginal");
                _settingsService.SetCustomAttachmentsFolder(settingsWindow.CustomAttachmentsFolderTextBox.Text);
                _settingsService.SetOrganizeByDate(settingsWindow.OrganizeByDateCheckBox.IsChecked ?? false);

                // Save UI settings
                _settingsService.SetDarkMode(settingsWindow.DarkModeCheckBox.IsChecked ?? false);
                _settingsService.SetShowNotifications(settingsWindow.ShowNotificationsCheckBox.IsChecked ?? true);

                // Save backup settings
                _settingsService.SetAutoBackup(settingsWindow.AutoBackupCheckBox.IsChecked ?? true);
                
                if (int.TryParse(settingsWindow.BackupCountTextBox.Text, out int backupCount))
                    _settingsService.SetBackupCount(backupCount);
                
                if (int.TryParse(settingsWindow.BackupRetentionTextBox.Text, out int backupRetention))
                    _settingsService.SetBackupRetention(backupRetention);

                // Save database path
                _settingsService.SetDatabasePath(settingsWindow.DatabasePathTextBox.Text);
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification($"خطأ في حفظ الإعدادات: {ex.Message}", NotificationType.Error);
            }
        }
    }
} 