using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CustomerIssuesManager.Core.Services;

public class BackupService : IBackupService
{
    private const string DbFileName = "CustomerIssues.db";
    private const string BackupFolderName = "backups";
    private const int MaxBackupsToKeep = 10;

    public Task CreateBackupAsync()
    {
        return Task.Run(() =>
        {
            var dbPath = Path.Combine(AppContext.BaseDirectory, DbFileName);
            if (!File.Exists(dbPath))
            {
                return; // No database to backup
            }

            var backupDir = Path.Combine(AppContext.BaseDirectory, BackupFolderName);
            Directory.CreateDirectory(backupDir);

            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            var backupFileName = $"backup_{timestamp}.db";
            var backupPath = Path.Combine(backupDir, backupFileName);

            File.Copy(dbPath, backupPath, true);
        });
    }

    public void CleanupOldBackups()
    {
        var backupDir = Path.Combine(AppContext.BaseDirectory, BackupFolderName);
        if (!Directory.Exists(backupDir))
        {
            return;
        }

        var backupFiles = new DirectoryInfo(backupDir)
            .GetFiles("*.db")
            .OrderByDescending(f => f.CreationTime)
            .Skip(MaxBackupsToKeep);

        foreach (var file in backupFiles)
        {
            try
            {
                file.Delete();
            }
            catch (Exception ex)
            {
                // Log the error but don't throw to avoid breaking the cleanup process
                System.Diagnostics.Debug.WriteLine($"Error deleting old backup file {file.Name}: {ex.Message}");
            }
        }
    }
}
