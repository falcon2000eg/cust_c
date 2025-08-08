using CustomerIssuesManager.Core.Services;
using CustomerIssuesManager.Services;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using WpfMessageBox = System.Windows.MessageBox;
using WpfMessageBoxButton = System.Windows.MessageBoxButton;
using WpfMessageBoxImage = System.Windows.MessageBoxImage;
using WpfMessageBoxResult = System.Windows.MessageBoxResult;

namespace CustomerIssuesManager
{
    public partial class SettingsWindow : Window
    {
        private readonly IBackupService _backupService;
        private readonly IFileManagerService _fileManagerService;
        private readonly ISettingsService _settingsService;
        private readonly NotificationService _notificationService;

        public SettingsWindow(IBackupService backupService, IFileManagerService fileManagerService, ISettingsService settingsService)
        {
            InitializeComponent();
            _backupService = backupService;
            _fileManagerService = fileManagerService;
            _settingsService = settingsService;
            _notificationService = new NotificationService(this);
            
            // Subscribe to settings changes for notifications
            _settingsService.SettingsChanged += SettingsService_SettingsChanged;
            
            this.Loaded += SettingsWindow_Loaded;
        }

        private void SettingsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCurrentSettings();
        }

        private void LoadCurrentSettings()
        {
            try
            {
                // Load database settings
                DatabasePathTextBox.Text = _settingsService.GetDatabasePath();
                
                // Load attachment settings
                string currentMode = _settingsService.GetAttachmentManagementMode();
                if (currentMode == "MoveToFolder")
                {
                    AttachmentManagementComboBox.SelectedIndex = 1;
                    CustomFolderPanel.Visibility = Visibility.Visible;
                }
                else
                {
                    AttachmentManagementComboBox.SelectedIndex = 0;
                }
                
                CustomAttachmentsFolderTextBox.Text = _settingsService.GetCustomAttachmentsFolder();
                if (string.IsNullOrEmpty(CustomAttachmentsFolderTextBox.Text))
                {
                    CustomAttachmentsFolderTextBox.Text = Path.Combine(AppContext.BaseDirectory, "CustomAttachments");
                }
                
                OrganizeByDateCheckBox.IsChecked = _settingsService.GetOrganizeByDate();
                
                // Load UI settings
                DarkModeCheckBox.IsChecked = _settingsService.GetDarkMode();
                ShowNotificationsCheckBox.IsChecked = _settingsService.GetShowNotifications();
                
                // Load backup settings
                AutoBackupCheckBox.IsChecked = _settingsService.GetAutoBackup();
                BackupCountTextBox.Text = _settingsService.GetBackupCount().ToString();
                BackupRetentionTextBox.Text = _settingsService.GetBackupRetention().ToString();
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification($"خطأ في تحميل الإعدادات: {ex.Message}", NotificationType.Error);
            }
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

        #region Import/Export Settings
        private async void ExportSettings_Click(object sender, RoutedEventArgs e)
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
                    }
                    else
                    {
                        _notificationService.ShowNotification("فشل في تصدير الإعدادات", NotificationType.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification($"خطأ في تصدير الإعدادات: {ex.Message}", NotificationType.Error);
            }
        }

        private async void ImportSettings_Click(object sender, RoutedEventArgs e)
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
                        LoadCurrentSettings(); // Reload settings after import
                    }
                    else
                    {
                        _notificationService.ShowNotification("فشل في استيراد الإعدادات", NotificationType.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification($"خطأ في استيراد الإعدادات: {ex.Message}", NotificationType.Error);
            }
        }
        #endregion

        private void ChangeDatabasePath_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var openFileDialog = new Microsoft.Win32.OpenFileDialog
                {
                    Title = "اختر قاعدة البيانات",
                    Filter = "ملفات قاعدة البيانات|*.db|جميع الملفات|*.*",
                    FileName = "CustomerIssues.db"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    DatabasePathTextBox.Text = openFileDialog.FileName;
                    _settingsService.SetDatabasePath(openFileDialog.FileName);
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification($"خطأ في تغيير مسار قاعدة البيانات: {ex.Message}", NotificationType.Error);
            }
        }

        private void SelectCustomFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var folderDialog = new System.Windows.Forms.FolderBrowserDialog
                {
                    Description = "اختر مجلد المرفقات المخصص"
                };

                if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    CustomAttachmentsFolderTextBox.Text = folderDialog.SelectedPath;
                    _settingsService.SetCustomAttachmentsFolder(folderDialog.SelectedPath);
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification($"خطأ في اختيار مجلد المرفقات: {ex.Message}", NotificationType.Error);
            }
        }

        private async void CreateBackup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _backupService.CreateBackupAsync();
                _notificationService.ShowNotification("تم إنشاء النسخة الاحتياطية بنجاح", NotificationType.Success);
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification($"فشل في إنشاء النسخة الاحتياطية: {ex.Message}", NotificationType.Error);
            }
        }

        private async void RestoreBackup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var openFileDialog = new Microsoft.Win32.OpenFileDialog
                {
                    Title = "اختر ملف النسخة الاحتياطية",
                    Filter = "ملفات قاعدة البيانات|*.db|جميع الملفات|*.*"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    // Note: RestoreBackupAsync method doesn't exist in IBackupService
                    // This would need to be implemented in the BackupService
                    _notificationService.ShowNotification("وظيفة استعادة النسخة الاحتياطية غير متوفرة حالياً", NotificationType.Warning);
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification($"خطأ في استعادة النسخة الاحتياطية: {ex.Message}", NotificationType.Error);
            }
        }

        private void CleanupFiles_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = WpfMessageBox.Show(
                    "هل أنت متأكد من تنظيف الملفات القديمة؟",
                    "تأكيد التنظيف",
                    WpfMessageBoxButton.YesNo,
                    WpfMessageBoxImage.Question);

                if (result == WpfMessageBoxResult.Yes)
                {
                    _fileManagerService.CleanupOldBackups();
                    _notificationService.ShowNotification("تم تنظيف الملفات القديمة بنجاح", NotificationType.Success);
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification($"خطأ في تنظيف الملفات: {ex.Message}", NotificationType.Error);
            }
        }

        private void ShowStorageInfo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var storageInfo = _fileManagerService.GetStorageInfo();
                var message = $"معلومات التخزين:\n\n" +
                             $"عدد الملفات: {storageInfo.TotalFiles}\n" +
                             $"الحجم الإجمالي: {storageInfo.TotalSize}\n" +
                             $"المسار الأساسي: {storageInfo.BasePath}";

                WpfMessageBox.Show(message, "معلومات التخزين", WpfMessageBoxButton.OK, WpfMessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification($"خطأ في عرض معلومات التخزين: {ex.Message}", NotificationType.Error);
            }
        }

        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate settings before saving
                if (!_settingsService.ValidateSettings())
                {
                    var errors = _settingsService.GetValidationErrors();
                    var errorMessage = "أخطاء في الإعدادات:\n\n" + string.Join("\n", errors);
                    _notificationService.ShowNotification(errorMessage, NotificationType.Error);
                    return;
                }

                // Save all settings
                SaveAllSettings();
                
                _notificationService.ShowNotification("تم حفظ الإعدادات بنجاح", NotificationType.Success);
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification($"خطأ في حفظ الإعدادات: {ex.Message}", NotificationType.Error);
            }
        }

        private void SaveAllSettings()
        {
            // Save attachment settings
            _settingsService.SetAttachmentManagementMode(
                AttachmentManagementComboBox.SelectedIndex == 1 ? "MoveToFolder" : "KeepOriginal");
            _settingsService.SetCustomAttachmentsFolder(CustomAttachmentsFolderTextBox.Text);
            _settingsService.SetOrganizeByDate(OrganizeByDateCheckBox.IsChecked ?? false);

            // Save UI settings
            _settingsService.SetDarkMode(DarkModeCheckBox.IsChecked ?? false);
            _settingsService.SetShowNotifications(ShowNotificationsCheckBox.IsChecked ?? true);

            // Save backup settings
            _settingsService.SetAutoBackup(AutoBackupCheckBox.IsChecked ?? true);
            
            if (int.TryParse(BackupCountTextBox.Text, out int backupCount))
                _settingsService.SetBackupCount(backupCount);
            
            if (int.TryParse(BackupRetentionTextBox.Text, out int backupRetention))
                _settingsService.SetBackupRetention(backupRetention);

            // Save database path
            _settingsService.SetDatabasePath(DatabasePathTextBox.Text);
        }

        private void ResetSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = WpfMessageBox.Show(
                    "هل أنت متأكد من إعادة تعيين جميع الإعدادات؟",
                    "تأكيد إعادة التعيين",
                    WpfMessageBoxButton.YesNo,
                    WpfMessageBoxImage.Question);

                if (result == WpfMessageBoxResult.Yes)
                {
                    LoadCurrentSettings(); // Reload original settings
                    _notificationService.ShowNotification("تم إعادة تعيين الإعدادات", NotificationType.Info);
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification($"خطأ في إعادة تعيين الإعدادات: {ex.Message}", NotificationType.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void AttachmentManagementComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (AttachmentManagementComboBox.SelectedIndex == 1) // MoveToFolder
            {
                CustomFolderPanel.Visibility = Visibility.Visible;
            }
            else
            {
                CustomFolderPanel.Visibility = Visibility.Collapsed;
            }
        }
    }
}
