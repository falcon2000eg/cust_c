using System.Threading.Tasks;

namespace CustomerIssuesManager.Core.Services;

public interface IBackupService
{
    Task CreateBackupAsync();
    void CleanupOldBackups();
}
