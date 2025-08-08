using System.Threading.Tasks;

namespace CustomerIssuesManager.Core.Services;

public interface IFileManager
{
    string GetCaseFolderPath(int caseId);
    Task<string> CopyFileToCaseFolderAsync(string sourcePath, int caseId);
}
