using CustomerIssuesManager.Core.Models;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CustomerIssuesManager.Core.Services
{
    public class PrintService : IPrintService
    {
        private readonly ICaseService _caseService;

        public PrintService(ICaseService caseService)
        {
            _caseService = caseService;
        }

        public async Task PrintCaseReportAsync(int caseId)
        {
            try
            {
                var reportContent = await _caseService.GenerateCaseReportAsync(caseId);
                var tempFilePath = Path.GetTempFileName();
                
                // Write report to temporary file
                await File.WriteAllTextAsync(tempFilePath, reportContent, System.Text.Encoding.UTF8);
                
                // Print the file using default system printer
                var processInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = tempFilePath,
                    Verb = "print",
                    UseShellExecute = true
                };
                
                System.Diagnostics.Process.Start(processInfo);
                
                // Clean up temp file after a delay
                await Task.Delay(5000);
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"فشل في طباعة التقرير: {ex.Message}", ex);
            }
        }

        public async Task PrintCasesListAsync()
        {
            try
            {
                var cases = await _caseService.GetCasesForExportAsync();
                var reportContent = GenerateCasesListReport(cases);
                var tempFilePath = Path.GetTempFileName();
                
                await File.WriteAllTextAsync(tempFilePath, reportContent, System.Text.Encoding.UTF8);
                
                var processInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = tempFilePath,
                    Verb = "print",
                    UseShellExecute = true
                };
                
                System.Diagnostics.Process.Start(processInfo);
                
                await Task.Delay(5000);
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"فشل في طباعة قائمة المشاكل: {ex.Message}", ex);
            }
        }

        public async Task<string> GeneratePrintPreviewAsync(int caseId)
        {
            return await _caseService.GenerateCaseReportAsync(caseId);
        }

        public string GeneratePrintPreview(List<Case> cases)
        {
            return GenerateCasesListReport(cases);
        }

        public async Task PrintCases(List<Case> cases)
        {
            try
            {
                var reportContent = GenerateCasesListReport(cases);
                var tempFilePath = Path.GetTempFileName();
                
                await File.WriteAllTextAsync(tempFilePath, reportContent, System.Text.Encoding.UTF8);
                
                var processInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = tempFilePath,
                    Verb = "print",
                    UseShellExecute = true
                };
                
                System.Diagnostics.Process.Start(processInfo);
                
                await Task.Delay(5000);
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"فشل في طباعة المشاكل: {ex.Message}", ex);
            }
        }

        public async Task<string> GenerateCaseReportAsync(int caseId)
        {
            return await _caseService.GenerateCaseReportAsync(caseId);
        }

        public async Task<string> GenerateAllCasesReportAsync(IEnumerable<Case> cases)
        {
            return GenerateCasesListReport(cases);
        }

        private string GenerateCasesListReport(IEnumerable<Case> cases)
        {
            var report = new System.Text.StringBuilder();
            report.AppendLine("========== تقرير قائمة المشاكل ==========");
            report.AppendLine();
            report.AppendLine($"تاريخ التقرير: {DateTime.Now:yyyy/MM/dd HH:mm:ss}");
            report.AppendLine($"إجمالي المشاكل: {cases.Count()}");
            report.AppendLine();
            report.AppendLine("--- تفاصيل المشاكل ---");
            
            foreach (var caseItem in cases)
            {
                report.AppendLine($"اسم العميل: {caseItem.CustomerName}");
                report.AppendLine($"رقم المشترك: {caseItem.SubscriberNumber}");
                report.AppendLine($"تصنيف المشكلة: {caseItem.Category?.CategoryName ?? ""}");
                report.AppendLine($"حالة المشكلة: {caseItem.Status}");
                report.AppendLine($"تاريخ الإنشاء: {caseItem.CreatedDate}");
                report.AppendLine($"أنشئ بواسطة: {caseItem.CreatedBy?.Name ?? ""}");
                report.AppendLine("---");
            }
            
            return report.ToString();
        }
    }
} 