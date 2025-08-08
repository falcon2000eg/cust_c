using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Collections.Generic;

namespace CustomerIssuesManager.Core.Services
{
    public class FileManagerService : IFileManagerService
    {
        private readonly string _attachmentsBasePath;
        private string _attachmentManagementMode = "KeepOriginal";
        private string _customAttachmentsFolder = "";
        private bool _organizeByDate = true;

        public FileManagerService(string attachmentsBasePath = "Attachments")
        {
            _attachmentsBasePath = Path.Combine(AppContext.BaseDirectory, attachmentsBasePath);
            EnsureBaseDirectory();
        }

        public void EnsureBaseDirectory()
        {
            if (!Directory.Exists(_attachmentsBasePath))
                Directory.CreateDirectory(_attachmentsBasePath);
        }

        public string CreateCaseFolder(int caseId)
        {
            string caseFolder = Path.Combine(_attachmentsBasePath, $"Case_{caseId}");
            if (!Directory.Exists(caseFolder))
                Directory.CreateDirectory(caseFolder);
            return caseFolder;
        }

        public string CopyFileToCaseFolder(string sourcePath, int caseId, string description = "")
        {
            if (!File.Exists(sourcePath))
                return string.Empty;

            string caseFolder = CreateCaseFolder(caseId);
            string fileName = Path.GetFileName(sourcePath);
            string destPath = Path.Combine(caseFolder, fileName);

            // Check if file already exists
            if (File.Exists(destPath))
            {
                // Add timestamp to avoid conflicts
                string nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                string extension = Path.GetExtension(fileName);
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                fileName = $"{nameWithoutExt}_{timestamp}{extension}";
                destPath = Path.Combine(caseFolder, fileName);
            }

            try
            {
                File.Copy(sourcePath, destPath, overwrite: true);
                return destPath;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error copying file from {sourcePath} to {destPath}: {ex.Message}");
                throw new InvalidOperationException($"فشل في نسخ الملف: {ex.Message}", ex);
            }
        }

        public void DeleteFile(string filePath)
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        public void OpenFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                try
                {
                    Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    // Provide user feedback instead of just debug output
                    System.Windows.MessageBox.Show(
                        $"فشل في فتح الملف:\n{ex.Message}",
                        "خطأ في فتح الملف",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Error);
                }
            }
            else
            {
                System.Windows.MessageBox.Show(
                    "الملف غير موجود",
                    "خطأ",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning);
            }
        }

        public void OpenCaseFolder(int caseId)
        {
            string caseFolder = Path.Combine(_attachmentsBasePath, $"Case_{caseId}");
            if (Directory.Exists(caseFolder))
            {
                try
                {
                    Process.Start(new ProcessStartInfo(caseFolder) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    // Provide user feedback instead of just debug output
                    System.Windows.MessageBox.Show(
                        $"فشل في فتح مجلد المشكلة:\n{ex.Message}",
                        "خطأ في فتح المجلد",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Error);
                }
            }
            else
            {
                System.Windows.MessageBox.Show(
                    "مجلد المشكلة غير موجود",
                    "خطأ",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning);
            }
        }

        public FileInfo? GetFileInfo(string filePath)
        {
            if (File.Exists(filePath))
                return new FileInfo(filePath);
            return null;
        }

        public string GetFileType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" => "صورة",
                ".pdf" => "PDF",
                ".doc" or ".docx" => "Word",
                ".xls" or ".xlsx" => "Excel",
                ".txt" => "نص",
                ".zip" or ".rar" => "مضغوط",
                _ => "آخر"
            };
        }

        public string FormatFileSize(long bytes)
        {
            return bytes switch
            {
                < 1024 => $"{bytes} بايت",
                < 1024 * 1024 => $"{bytes / 1024.0:F1} كيلوبايت",
                < 1024 * 1024 * 1024 => $"{bytes / (1024.0 * 1024.0):F1} ميجابايت",
                _ => $"{bytes / (1024.0 * 1024.0 * 1024.0):F1} جيجابايت"
            };
        }

        public async Task<string> CopyAttachmentAsync(string sourcePath, int caseId)
        {
            return await Task.FromResult(CopyFileToCaseFolder(sourcePath, caseId));
        }

        public string? SelectAndCopyFile(int caseId, string description = "")
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "اختر الملف",
                Filter = "ملفات الصور|*.jpg;*.jpeg;*.png;*.gif;*.bmp|ملفات PDF|*.pdf|ملفات Word|*.doc;*.docx|ملفات Excel|*.xls;*.xlsx|جميع الملفات|*.*",
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                return CopyFileToCaseFolder(openFileDialog.FileName, caseId, description);
            }

            return null;
        }

        public FileAttachmentInfo? GetAttachmentInfo(string filePath, string description = "")
        {
            if (!File.Exists(filePath))
                return null;

            var fileInfo = new FileInfo(filePath);
            return new FileAttachmentInfo
            {
                FileName = fileInfo.Name,
                FilePath = fileInfo.FullName,
                FileType = GetFileType(fileInfo.Name),
                Description = description,
                Size = FormatFileSize(fileInfo.Length),
                SizeBytes = fileInfo.Length,
                ModifiedDate = fileInfo.LastWriteTime
            };
        }

        public bool MoveFileToCase(string sourcePath, int targetCaseId, string description = "")
        {
            if (!File.Exists(sourcePath))
                return false;

            try
            {
                string targetFolder = CreateCaseFolder(targetCaseId);
                string fileName = Path.GetFileName(sourcePath);
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string newFileName = $"{timestamp}_{fileName}";
                string newFilePath = Path.Combine(targetFolder, newFileName);

                File.Move(sourcePath, newFilePath);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error moving file: {ex.Message}");
                return false;
            }
        }

        public IEnumerable<FileAttachmentInfo> GetCaseFilesInfo(int caseId)
        {
            var filesInfo = new List<FileAttachmentInfo>();
            string caseFolder = Path.Combine(_attachmentsBasePath, $"Case_{caseId}");

            if (Directory.Exists(caseFolder))
            {
                foreach (var filePath in Directory.GetFiles(caseFolder))
                {
                    var fileInfo = new FileInfo(filePath);
                    filesInfo.Add(new FileAttachmentInfo
                    {
                        FileName = fileInfo.Name,
                        FilePath = fileInfo.FullName,
                        FileType = GetFileType(fileInfo.Name),
                        Size = FormatFileSize(fileInfo.Length),
                        SizeBytes = fileInfo.Length,
                        ModifiedDate = fileInfo.LastWriteTime
                    });
                }
            }

            return filesInfo;
        }

        public string? CreateBackup(int caseId, string? backupPath = null)
        {
            string caseFolder = Path.Combine(_attachmentsBasePath, $"Case_{caseId}");

            if (!Directory.Exists(caseFolder))
                return null;

            if (string.IsNullOrEmpty(backupPath))
            {
                backupPath = Path.Combine(_attachmentsBasePath, "Backups");
                if (!Directory.Exists(backupPath))
                    Directory.CreateDirectory(backupPath);
            }

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string backupName = $"Case_{caseId}_Backup_{timestamp}";
            string backupFullPath = Path.Combine(backupPath, backupName);

            try
            {
                CopyDirectory(caseFolder, backupFullPath);
                return backupFullPath;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating backup: {ex.Message}");
                return null;
            }
        }

        public void CleanupOldBackups(int daysToKeep = 30)
        {
            string backupPath = Path.Combine(_attachmentsBasePath, "Backups");
            if (!Directory.Exists(backupPath))
                return;

            var cutoffTime = DateTime.Now.AddDays(-daysToKeep);
            var backupDirectories = Directory.GetDirectories(backupPath);

            foreach (var directory in backupDirectories)
            {
                var dirInfo = new DirectoryInfo(directory);
                if (dirInfo.CreationTime < cutoffTime)
                {
                    try
                    {
                        Directory.Delete(directory, true);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error deleting old backup: {ex.Message}");
                    }
                }
            }
        }

        public StorageInfo GetStorageInfo()
        {
            long totalSize = 0;
            int totalFiles = 0;

            if (Directory.Exists(_attachmentsBasePath))
            {
                foreach (var file in Directory.GetFiles(_attachmentsBasePath, "*", SearchOption.AllDirectories))
                {
                    try
                    {
                        var fileInfo = new FileInfo(file);
                        totalSize += fileInfo.Length;
                        totalFiles++;
                    }
                    catch
                    {
                        // Skip files that can't be accessed
                    }
                }
            }

            return new StorageInfo
            {
                TotalFiles = totalFiles,
                TotalSize = FormatFileSize(totalSize),
                TotalSizeBytes = totalSize,
                BasePath = _attachmentsBasePath
            };
        }

        private void CopyDirectory(string sourceDir, string destDir)
        {
            var dir = new DirectoryInfo(sourceDir);
            Directory.CreateDirectory(destDir);

            foreach (var file in dir.GetFiles())
            {
                string tempPath = Path.Combine(destDir, file.Name);
                file.CopyTo(tempPath, false);
            }

            foreach (var subDir in dir.GetDirectories())
            {
                string tempPath = Path.Combine(destDir, subDir.Name);
                CopyDirectory(subDir.FullName, tempPath);
            }
        }

        // Attachment Management Settings Implementation
        public void SetAttachmentManagementMode(string mode)
        {
            _attachmentManagementMode = mode;
        }

        public void SetCustomAttachmentsFolder(string folderPath)
        {
            _customAttachmentsFolder = folderPath;
            if (!string.IsNullOrEmpty(folderPath) && !Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
        }

        public void SetOrganizeByDate(bool organizeByDate)
        {
            _organizeByDate = organizeByDate;
        }

        public string GetAttachmentManagementMode()
        {
            return _attachmentManagementMode;
        }

        public string GetCustomAttachmentsFolder()
        {
            return _customAttachmentsFolder;
        }

        public bool GetOrganizeByDate()
        {
            return _organizeByDate;
        }

        public string GetOrganizedFolderPath(int caseId, DateTime? date = null)
        {
            string baseFolder = _attachmentManagementMode == "MoveToFolder" && !string.IsNullOrEmpty(_customAttachmentsFolder)
                ? _customAttachmentsFolder
                : _attachmentsBasePath;

            string caseFolder = Path.Combine(baseFolder, $"Case_{caseId}");

            if (_organizeByDate && date.HasValue)
            {
                string dateFolder = date.Value.ToString("yyyy-MM");
                caseFolder = Path.Combine(caseFolder, dateFolder);
            }

            if (!Directory.Exists(caseFolder))
            {
                Directory.CreateDirectory(caseFolder);
            }

            return caseFolder;
        }
    }

    public class FileAttachmentInfo
    {
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public long SizeBytes { get; set; }
        public DateTime ModifiedDate { get; set; }
    }

    public class StorageInfo
    {
        public int TotalFiles { get; set; }
        public string TotalSize { get; set; } = string.Empty;
        public long TotalSizeBytes { get; set; }
        public string BasePath { get; set; } = string.Empty;
    }
}
