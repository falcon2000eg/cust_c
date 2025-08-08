using System.Collections.Generic;
using System.Threading.Tasks;
using CustomerIssuesManager.Core.Models;

namespace CustomerIssuesManager.Core.Services
{
    public interface IPrintService
    {
        Task PrintCaseReportAsync(int caseId);
        Task PrintCasesListAsync();
        Task<string> GeneratePrintPreviewAsync(int caseId);
        string GeneratePrintPreview(List<Case> cases);
        Task PrintCases(List<Case> cases);
        Task<string> GenerateCaseReportAsync(int caseId);
        Task<string> GenerateAllCasesReportAsync(IEnumerable<Case> cases);
    }
} 