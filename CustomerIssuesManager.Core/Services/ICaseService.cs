using CustomerIssuesManager.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CustomerIssuesManager.Core.Services
{
    public interface ICaseService
    {
        Task<IEnumerable<Case>> GetAllCasesAsync();
        Task<Case> GetCaseByIdAsync(int caseId);
        Task<Case> CreateCaseAsync(Case newCase, int creatorId);
        Task UpdateCaseAsync(Case caseToUpdate, int modifierId);
        Task DeleteCaseAsync(int caseId, int deleterId);
        Task<Attachment> AddAttachmentAsync(Attachment attachment);
        Task<Correspondence> AddCorrespondenceAsync(Correspondence correspondence);
        Task<IEnumerable<IssueCategory>> GetAllCategoriesAsync();
        Task<IEnumerable<Case>> SearchCasesAsync(string searchTerm);
        Task<IEnumerable<Case>> AdvancedSearchCasesAsync(
            string? customerName,
            string? subscriberNumber,
            string? address,
            string? status,
            int? year,
            string? dateField,
            string? category,
            string? correspondenceText,
            string? attachmentText);
        Task DeleteAttachmentAsync(int attachmentId);
        Task DeleteCorrespondenceAsync(int correspondenceId);
        Task<IEnumerable<Case>> SearchCasesComprehensiveAsync(SearchCriteria criteria);
        Task<IEnumerable<Case>> GetCasesByYearAsync(int? year = null);
        Task<IEnumerable<string>> GetAvailableYearsAsync();
        Task<IEnumerable<string>> GetAvailableCategoriesAsync();
        Task<IEnumerable<string>> GetAvailableStatusesAsync();
        Task<IEnumerable<string>> GetAvailableEmployeesAsync();
        
        // New methods for print and export functionality
        Task<string> GenerateCaseReportAsync(int caseId);
        Task<string> ExportCasesToCsvAsync(IEnumerable<Case> cases, string filePath);
        Task<IEnumerable<Case>> GetCasesForExportAsync();
        Task<CaseStatistics> GetCaseStatisticsAsync();
        
        // New methods for yearly numbering and search
        Task<string> GetNextYearlySequenceNumberAsync(int year);
        Task<IEnumerable<Correspondence>> SearchCorrespondencesByYearlyNumberAsync(string yearlyNumber);
    }
}
