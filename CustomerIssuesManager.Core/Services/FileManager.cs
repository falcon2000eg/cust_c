using System;
using System.IO;
using System.Threading.Tasks;

namespace CustomerIssuesManager.Core.Services;

public class FileManager : IFileManager
{
    private readonly string _baseAttachmentsPath;

    public FileManager()
    {
        // Create a dedicated folder for copied attachments in the application's root directory
        _baseAttachmentsPath = Path.Combine(AppContext.BaseDirectory, "attachments");
        Directory.CreateDirectory(_baseAttachmentsPath);
    }

    public string GetCaseFolderPath(int caseId)
    {
        var caseFolder = Path.Combine(_baseAttachmentsPath, $"case_{caseId}");
        Directory.CreateDirectory(caseFolder);
        return caseFolder;
    }

    public async Task<string> CopyFileToCaseFolderAsync(string sourcePath, int caseId)
    {
        if (!File.Exists(sourcePath))
        {
            throw new FileNotFoundException("Source file not found.", sourcePath);
        }

        var caseFolder = GetCaseFolderPath(caseId);
        var destinationFileName = Path.GetFileName(sourcePath);
        var destinationPath = Path.Combine(caseFolder, destinationFileName);

        // Handle file name conflicts by adding a timestamp
        if (File.Exists(destinationPath))
        {
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(sourcePath);
            var extension = Path.GetExtension(sourcePath);
            destinationFileName = $"{fileNameWithoutExtension}_{timestamp}{extension}";
            destinationPath = Path.Combine(caseFolder, destinationFileName);
        }

        using (var sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true))
        using (var destinationStream = new FileStream(destinationPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, useAsync: true))
        {
            await sourceStream.CopyToAsync(destinationStream);
        }

        return destinationPath;
    }
}
