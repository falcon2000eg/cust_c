using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace CustomerIssuesManager.Core.Services
{
    public interface IFileManagerService
    {
        void EnsureBaseDirectory();
        string CreateCaseFolder(int caseId);
        string CopyFileToCaseFolder(string sourcePath, int caseId, string description = "");
        void DeleteFile(string filePath);
        void OpenFile(string filePath);
        void OpenCaseFolder(int caseId);
        FileInfo? GetFileInfo(string filePath);
        Task<string> CopyAttachmentAsync(string sourcePath, int caseId);
        
        // New methods from Python version
        string GetFileType(string fileName);
        string FormatFileSize(long bytes);
        string? SelectAndCopyFile(int caseId, string description = "");
        FileAttachmentInfo? GetAttachmentInfo(string filePath, string description = "");
        bool MoveFileToCase(string sourcePath, int targetCaseId, string description = "");
        IEnumerable<FileAttachmentInfo> GetCaseFilesInfo(int caseId);
        string? CreateBackup(int caseId, string? backupPath = null);
        void CleanupOldBackups(int daysToKeep = 30);
        StorageInfo GetStorageInfo();
        
        // Attachment Management Settings
        void SetAttachmentManagementMode(string mode);
        void SetCustomAttachmentsFolder(string folderPath);
        void SetOrganizeByDate(bool organizeByDate);
        string GetAttachmentManagementMode();
        string GetCustomAttachmentsFolder();
        bool GetOrganizeByDate();
        string GetOrganizedFolderPath(int caseId, DateTime? date = null);
    }
}
