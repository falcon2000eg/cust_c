using CustomerIssuesManager.Core.Data;
using CustomerIssuesManager.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using System;
using CsvHelper;
using System.Globalization;

namespace CustomerIssuesManager.Core.Services
{
    public class CaseService : ICaseService
    {
        private readonly AppDbContext _context;

        public CaseService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Case>> GetAllCasesAsync()
        {
            return await _context.Cases
                .Include(c => c.Category)
                .Include(c => c.CreatedBy)
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();
        }

        public async Task<Case> GetCaseByIdAsync(int caseId)
        {
            return await _context.Cases
                .Include(c => c.Category)
                .Include(c => c.CreatedBy)
                .Include(c => c.ModifiedBy)
                .Include(c => c.SolvedBy)
                .Include(c => c.Correspondences)
                .Include(c => c.Attachments)
                .Include(c => c.AuditLogs)
                .FirstOrDefaultAsync(c => c.Id == caseId);
        }

        public async Task<Case> CreateCaseAsync(Case newCase, int creatorId)
        {
            if (newCase == null)
                throw new ArgumentNullException(nameof(newCase));

            newCase.CreatedById = creatorId;
            _context.Cases.Add(newCase);

            var creator = await _context.Employees.FindAsync(creatorId);
            if (creator == null)
                throw new ArgumentException($"Employee with ID {creatorId} not found", nameof(creatorId));

            // Save the case first to get the ID
            await _context.SaveChangesAsync();

            var auditLog = new AuditLog
            {
                Case = newCase,
                CaseId = newCase.Id,
                ActionType = "إنشاء",
                ActionDescription = $"تم إنشاء مشكلة جديدة للعميل: {newCase.CustomerName}",
                PerformedById = creatorId,
                PerformedBy = creator,
                PerformedByName = creator.Name,
                Timestamp = System.DateTime.Now,
                NewValues = JsonSerializer.Serialize(newCase, new JsonSerializerOptions { ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve })
            };
            _context.AuditLogs.Add(auditLog);

            await _context.SaveChangesAsync();
            return newCase;
        }

        public async Task UpdateCaseAsync(Case caseToUpdate, int modifierId)
        {
            if (caseToUpdate == null)
                throw new ArgumentNullException(nameof(caseToUpdate));

            var originalCase = await _context.Cases.AsNoTracking().FirstOrDefaultAsync(c => c.Id == caseToUpdate.Id);
            if (originalCase == null)
                throw new ArgumentException($"Case with ID {caseToUpdate.Id} not found", nameof(caseToUpdate));
            
            caseToUpdate.ModifiedById = modifierId;
            caseToUpdate.ModifiedDate = System.DateTime.Now;

            var modifier = await _context.Employees.FindAsync(modifierId);
            if (modifier == null)
                throw new ArgumentException($"Employee with ID {modifierId} not found", nameof(modifierId));

            var auditLog = new AuditLog
            {
                Case = caseToUpdate,
                CaseId = caseToUpdate.Id,
                ActionType = "تعديل",
                ActionDescription = $"تم تعديل المشكلة الخاصة بالعميل: {caseToUpdate.CustomerName}",
                PerformedById = modifierId,
                PerformedBy = modifier,
                PerformedByName = modifier.Name,
                Timestamp = System.DateTime.Now,
                OldValues = JsonSerializer.Serialize(originalCase, new JsonSerializerOptions { ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve }),
                NewValues = JsonSerializer.Serialize(caseToUpdate, new JsonSerializerOptions { ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve })
            };
            _context.AuditLogs.Add(auditLog);

            _context.Entry(caseToUpdate).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCaseAsync(int caseId, int deleterId)
        {
            var caseToDelete = await _context.Cases.FindAsync(caseId);
            if (caseToDelete == null)
                throw new ArgumentException($"Case with ID {caseId} not found", nameof(caseId));

            var deleter = await _context.Employees.FindAsync(deleterId);
            if (deleter == null)
                throw new ArgumentException($"Employee with ID {deleterId} not found", nameof(deleterId));

            var auditLog = new AuditLog
            {
                Case = caseToDelete,
                CaseId = caseToDelete.Id,
                ActionType = "حذف",
                ActionDescription = $"تم حذف المشكلة الخاصة بالعميل: {caseToDelete.CustomerName}",
                PerformedById = deleterId,
                PerformedBy = deleter,
                PerformedByName = deleter.Name,
                Timestamp = System.DateTime.Now,
                OldValues = JsonSerializer.Serialize(caseToDelete, new JsonSerializerOptions { ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve })
            };
            _context.AuditLogs.Add(auditLog);

            _context.Cases.Remove(caseToDelete);
            await _context.SaveChangesAsync();
        }

        public async Task<Attachment> AddAttachmentAsync(Attachment attachment)
        {
            _context.Attachments.Add(attachment);
            await _context.SaveChangesAsync();
            return attachment;
        }

        public async Task<Correspondence> AddCorrespondenceAsync(Correspondence correspondence)
        {
            // ترقيم تسلسلي للحالة
            var caseSequence = await _context.Correspondences
                .Where(c => c.CaseId == correspondence.CaseId)
                .CountAsync();
            correspondence.CaseSequenceNumber = caseSequence + 1;

            // ترقيم سنوي تلقائي لجميع المراسلات
            var year = correspondence.SentDate.Year;
            var yearlySequence = await _context.Correspondences
                .Where(c => c.SentDate.Year == year)
                .CountAsync();
            
            // تنسيق الرقم السنوي: السنة-الرقم التسلسلي
            correspondence.YearlySequenceNumber = $"{year:D4}-{(yearlySequence + 1):D4}";

            _context.Correspondences.Add(correspondence);
            await _context.SaveChangesAsync();
            return correspondence;
        }

        /// <summary>
        /// الحصول على الرقم السنوي التالي للمراسلات
        /// </summary>
        public async Task<string> GetNextYearlySequenceNumberAsync(int year)
        {
            var yearlySequence = await _context.Correspondences
                .Where(c => c.SentDate.Year == year)
                .CountAsync();
            
            return $"{year:D4}-{(yearlySequence + 1):D4}";
        }

        /// <summary>
        /// البحث في المراسلات بالرقم السنوي
        /// </summary>
        public async Task<IEnumerable<Correspondence>> SearchCorrespondencesByYearlyNumberAsync(string yearlyNumber)
        {
            return await _context.Correspondences
                .Include(c => c.Case)
                .Include(c => c.CreatedBy)
                .Where(c => c.YearlySequenceNumber.Contains(yearlyNumber))
                .OrderByDescending(c => c.SentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<IssueCategory>> GetAllCategoriesAsync()
        {
            return await _context.IssueCategories.ToListAsync();
        }

        public async Task<IEnumerable<Case>> SearchCasesAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetAllCasesAsync();
            }

            var lowerCaseSearchTerm = searchTerm.ToLower();

            return await _context.Cases
                .Include(c => c.Category)
                .Include(c => c.CreatedBy)
                .Where(c => 
                    c.CustomerName.ToLower().Contains(lowerCaseSearchTerm) ||
                    (c.SubscriberNumber != null && c.SubscriberNumber.ToLower().Contains(lowerCaseSearchTerm)) ||
                    (c.Phone != null && c.Phone.Contains(lowerCaseSearchTerm)) ||
                    c.Category.CategoryName.ToLower().Contains(lowerCaseSearchTerm) ||
                    c.Status.ToLower().Contains(lowerCaseSearchTerm)
                )
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Case>> AdvancedSearchCasesAsync(
            string? customerName,
            string? subscriberNumber,
            string? address,
            string? status,
            int? year,
            string? dateField,
            string? category,
            string? correspondenceText,
            string? attachmentText)
        {
            var query = _context.Cases
                .Include(c => c.Category)
                .Include(c => c.CreatedBy)
                .Include(c => c.ModifiedBy)
                .Include(c => c.SolvedBy)
                .Include(c => c.Correspondences)
                .Include(c => c.Attachments)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(customerName))
                query = query.Where(c => c.CustomerName.Contains(customerName));
            if (!string.IsNullOrWhiteSpace(subscriberNumber))
                query = query.Where(c => c.SubscriberNumber.Contains(subscriberNumber));
            if (!string.IsNullOrWhiteSpace(address))
                query = query.Where(c => c.Address.Contains(address));
            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(c => c.Status == status);
            if (!string.IsNullOrWhiteSpace(category))
                query = query.Where(c => c.Category.CategoryName == category);

            // فلترة بالسنة حسب الحقل المختار
            if (year.HasValue && !string.IsNullOrWhiteSpace(dateField))
            {
                if (dateField == "CreatedDate")
                    query = query.Where(c => c.CreatedDate.HasValue && c.CreatedDate.Value.Year == year.Value);
                else if (dateField == "ReceivedDate")
                    query = query.Where(c => c.ReceivedDate.HasValue && c.ReceivedDate.Value.Year == year.Value);
            }

            // البحث في المراسلات
            if (!string.IsNullOrWhiteSpace(correspondenceText))
                query = query.Where(c => c.Correspondences.Any(co => co.MessageContent.Contains(correspondenceText)));

            // البحث في المرفقات
            if (!string.IsNullOrWhiteSpace(attachmentText))
                query = query.Where(c => c.Attachments.Any(a => a.Description.Contains(attachmentText) || a.FileName.Contains(attachmentText)));

            return await query.OrderByDescending(c => c.CreatedDate).ToListAsync();
        }

        public async Task DeleteAttachmentAsync(int attachmentId)
        {
            var att = await _context.Attachments.FindAsync(attachmentId);
            if (att != null)
            {
                _context.Attachments.Remove(att);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteCorrespondenceAsync(int correspondenceId)
        {
            var corr = await _context.Correspondences.FindAsync(correspondenceId);
            if (corr != null)
            {
                _context.Correspondences.Remove(corr);
                await _context.SaveChangesAsync();
            }
        }

        // New comprehensive search methods
        public async Task<IEnumerable<Case>> SearchCasesComprehensiveAsync(SearchCriteria criteria)
        {
            var query = _context.Cases
                .Include(c => c.Category)
                .Include(c => c.CreatedBy)
                .Include(c => c.ModifiedBy)
                .Include(c => c.SolvedBy)
                .Include(c => c.Correspondences)
                .Include(c => c.Attachments)
                .AsQueryable();

            // Comprehensive search (شامل)
            if (criteria.SearchField == SearchFieldTypes.Comprehensive && !string.IsNullOrWhiteSpace(criteria.SearchValue))
            {
                var searchValue = criteria.SearchValue.ToLower();
                query = query.Where(c =>
                    c.CustomerName.ToLower().Contains(searchValue) ||
                    (c.SubscriberNumber != null && c.SubscriberNumber.ToLower().Contains(searchValue)) ||
                    (c.Address != null && c.Address.ToLower().Contains(searchValue)) ||
                    (c.ProblemDescription != null && c.ProblemDescription.ToLower().Contains(searchValue)) ||
                    (c.ActionsTaken != null && c.ActionsTaken.ToLower().Contains(searchValue)) ||
                    c.Correspondences.Any(co => co.MessageContent.ToLower().Contains(searchValue)) ||
                    c.Attachments.Any(a => (a.Description != null && a.Description.ToLower().Contains(searchValue)) ||
                                          (a.FileName != null && a.FileName.ToLower().Contains(searchValue)))
                );
            }
            else
            {
                // Specific field searches
                if (!string.IsNullOrWhiteSpace(criteria.CustomerName))
                    query = query.Where(c => c.CustomerName.Contains(criteria.CustomerName));

                if (!string.IsNullOrWhiteSpace(criteria.SubscriberNumber))
                    query = query.Where(c => c.SubscriberNumber.Contains(criteria.SubscriberNumber));

                if (!string.IsNullOrWhiteSpace(criteria.Address))
                    query = query.Where(c => c.Address.Contains(criteria.Address));

                if (!string.IsNullOrWhiteSpace(criteria.Status))
                    query = query.Where(c => c.Status == criteria.Status);

                if (!string.IsNullOrWhiteSpace(criteria.Category))
                    query = query.Where(c => c.Category.CategoryName == criteria.Category);

                if (!string.IsNullOrWhiteSpace(criteria.EmployeeName))
                    query = query.Where(c => c.CreatedBy.Name == criteria.EmployeeName || 
                                           (c.ModifiedBy != null && c.ModifiedBy.Name == criteria.EmployeeName));

                if (!string.IsNullOrWhiteSpace(criteria.ProblemDescription))
                    query = query.Where(c => c.ProblemDescription.Contains(criteria.ProblemDescription));

                if (!string.IsNullOrWhiteSpace(criteria.ActionsTaken))
                    query = query.Where(c => c.ActionsTaken.Contains(criteria.ActionsTaken));

                if (!string.IsNullOrWhiteSpace(criteria.CorrespondenceText))
                    query = query.Where(c => c.Correspondences.Any(co => co.MessageContent.Contains(criteria.CorrespondenceText)));

                if (!string.IsNullOrWhiteSpace(criteria.AttachmentText))
                    query = query.Where(c => c.Attachments.Any(a => 
                        (a.Description != null && a.Description.Contains(criteria.AttachmentText)) ||
                        (a.FileName != null && a.FileName.Contains(criteria.AttachmentText))));
            }

            // Year filtering
            if (criteria.Year.HasValue)
            {
                switch (criteria.DateField)
                {
                    case DateFieldTypes.CreatedDate:
                        query = query.Where(c => c.CreatedDate.HasValue && c.CreatedDate.Value.Year == criteria.Year.Value);
                        break;
                    case DateFieldTypes.ReceivedDate:
                        query = query.Where(c => c.ReceivedDate.HasValue && c.ReceivedDate.Value.Year == criteria.Year.Value);
                        break;
                    case DateFieldTypes.ModifiedDate:
                        query = query.Where(c => c.ModifiedDate.HasValue && c.ModifiedDate.Value.Year == criteria.Year.Value);
                        break;
                    case DateFieldTypes.SolvedDate:
                        query = query.Where(c => c.SolvedDate.HasValue && c.SolvedDate.Value.Year == criteria.Year.Value);
                        break;
                }
            }

            return await query.OrderByDescending(c => c.ModifiedDate).ThenByDescending(c => c.CreatedDate).ToListAsync();
        }

        public async Task<IEnumerable<Case>> GetCasesByYearAsync(int? year = null)
        {
            var query = _context.Cases
                .Include(c => c.Category)
                .Include(c => c.CreatedBy)
                .Include(c => c.ModifiedBy)
                .AsQueryable();

            if (year.HasValue)
            {
                query = query.Where(c => c.CreatedDate.HasValue && c.CreatedDate.Value.Year == year.Value);
            }

            return await query.OrderByDescending(c => c.ModifiedDate).ThenByDescending(c => c.CreatedDate).ToListAsync();
        }

        public async Task<IEnumerable<string>> GetAvailableYearsAsync()
        {
            return await _context.Cases
                .Where(c => c.CreatedDate.HasValue)
                .Select(c => c.CreatedDate!.Value.Year.ToString())
                .Distinct()
                .OrderByDescending(y => y)
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetAvailableCategoriesAsync()
        {
            return await _context.IssueCategories
                .Select(c => c.CategoryName)
                .OrderBy(c => c)
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetAvailableStatusesAsync()
        {
            return await _context.Cases
                .Select(c => c.Status)
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetAvailableEmployeesAsync()
        {
            return await _context.Employees
                .Where(e => e.IsActive)
                .Select(e => e.Name)
                .Distinct()
                .ToListAsync();
        }

        public async Task<string> GenerateCaseReportAsync(int caseId)
        {
            var caseData = await GetCaseByIdAsync(caseId);
            if (caseData == null)
                throw new ArgumentException("Case not found", nameof(caseId));

            var report = new System.Text.StringBuilder();
            report.AppendLine("========== تقرير حالة عميل ==========");
            report.AppendLine();
            report.AppendLine("--- بيانات المشكلة ---");
            report.AppendLine($"اسم العميل: {caseData.CustomerName}");
            report.AppendLine($"رقم المشترك: {caseData.SubscriberNumber}");
            report.AppendLine($"الهاتف: {caseData.Phone ?? ""}");
            report.AppendLine($"العنوان: {caseData.Address ?? ""}");
            report.AppendLine($"تصنيف المشكلة: {caseData.Category?.CategoryName ?? ""}");
            report.AppendLine($"حالة المشكلة: {caseData.Status}");
            report.AppendLine($"وصف المشكلة: {caseData.ProblemDescription ?? ""}");
            report.AppendLine($"الإجراءات المتخذة: {caseData.ActionsTaken ?? ""}");
            report.AppendLine($"آخر قراءة عداد: {caseData.LastMeterReading}");
            report.AppendLine($"تاريخ آخر قراءة: {caseData.LastReadingDate}");
            report.AppendLine($"مبلغ الدين: {caseData.DebtAmount}");
            report.AppendLine($"تاريخ الورود: {caseData.ReceivedDate}");
            report.AppendLine($"تاريخ الإنشاء: {caseData.CreatedDate}");
            report.AppendLine($"أنشئ بواسطة: {caseData.CreatedBy?.Name ?? ""}");
            report.AppendLine($"تاريخ التعديل: {caseData.ModifiedDate}");
            report.AppendLine($"عدل بواسطة: {caseData.ModifiedBy?.Name ?? ""}");
            report.AppendLine($"تم الحل بواسطة: {caseData.SolvedBy?.Name ?? ""}");
            report.AppendLine($"تاريخ الحل: {caseData.SolvedDate}");

            report.AppendLine();
            report.AppendLine("--- المرفقات ---");
            if (caseData.Attachments?.Any() == true)
            {
                foreach (var attachment in caseData.Attachments)
                {
                    report.AppendLine($"ملف: {attachment.FileName} | الوصف: {attachment.Description ?? ""} | التاريخ: {attachment.UploadDate}");
                }
            }
            else
            {
                report.AppendLine("لا يوجد مرفقات");
            }

            report.AppendLine();
            report.AppendLine("--- المراسلات ---");
            if (caseData.Correspondences?.Any() == true)
            {
                foreach (var correspondence in caseData.Correspondences)
                {
                    report.AppendLine($"مرسل: {correspondence.Sender} | التاريخ: {correspondence.SentDate}");
                    report.AppendLine($"المحتوى: {correspondence.MessageContent}");
                    report.AppendLine("---");
                }
            }
            else
            {
                report.AppendLine("لا يوجد مراسلات");
            }

            report.AppendLine();
            report.AppendLine("--- سجل التعديلات ---");
            if (caseData.AuditLogs?.Any() == true)
            {
                foreach (var log in caseData.AuditLogs)
                {
                    report.AppendLine($"{log.ActionType} | {log.ActionDescription} | {log.PerformedByName} | {log.Timestamp}");
                }
            }
            else
            {
                report.AppendLine("لا يوجد سجل تعديلات");
            }

            return report.ToString();
        }

        public async Task<string> ExportCasesToCsvAsync(IEnumerable<Case> cases, string filePath)
        {
            using var writer = new System.IO.StreamWriter(filePath, false, System.Text.Encoding.UTF8);
            using var csv = new CsvHelper.CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture);

            // Write headers
            csv.WriteField("اسم العميل");
            csv.WriteField("رقم المشترك");
            csv.WriteField("تصنيف المشكلة");
            csv.WriteField("حالة المشكلة");
            csv.WriteField("تاريخ الإضافة");
            csv.WriteField("آخر تعديل");
            csv.WriteField("أنشئ بواسطة");
            csv.WriteField("عدل بواسطة");
            csv.NextRecord();

            // Write data
            foreach (var caseItem in cases)
            {
                csv.WriteField(caseItem.CustomerName);
                csv.WriteField(caseItem.SubscriberNumber);
                csv.WriteField(caseItem.Category?.CategoryName ?? "");
                csv.WriteField(caseItem.Status);
                csv.WriteField(caseItem.CreatedDate);
                csv.WriteField(caseItem.ModifiedDate);
                csv.WriteField(caseItem.CreatedBy?.Name ?? "");
                csv.WriteField(caseItem.ModifiedBy?.Name ?? "");
                csv.NextRecord();
            }

            return filePath;
        }

        public async Task<IEnumerable<Case>> GetCasesForExportAsync()
        {
            return await _context.Cases
                .Include(c => c.Category)
                .Include(c => c.CreatedBy)
                .Include(c => c.ModifiedBy)
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();
        }

        public async Task<CaseStatistics> GetCaseStatisticsAsync()
        {
            var allCases = await _context.Cases.ToListAsync();
            var currentDate = DateTime.Now;
            var currentMonth = currentDate.Month;
            var currentYear = currentDate.Year;

            var statistics = new CaseStatistics
            {
                TotalCases = allCases.Count,
                NewCases = allCases.Count(c => c.Status == "جديدة"),
                InProgressCases = allCases.Count(c => c.Status == "قيد التنفيذ"),
                SolvedCases = allCases.Count(c => c.Status == "تم حلها"),
                ClosedCases = allCases.Count(c => c.Status == "مغلقة"),
                ActiveCases = allCases.Count(c => c.Status != "تم حلها" && c.Status != "مغلقة"),
                CasesThisMonth = allCases.Count(c => c.CreatedDate.HasValue && c.CreatedDate.Value.Month == currentMonth && c.CreatedDate.Value.Year == currentYear),
                CasesThisYear = allCases.Count(c => c.CreatedDate.HasValue && c.CreatedDate.Value.Year == currentYear),
                LastCaseDate = allCases.Any() ? allCases.Max(c => c.CreatedDate ?? DateTime.MinValue) : DateTime.MinValue
            };

            // Calculate most common category
            var categoryStats = await _context.Cases
                .Include(c => c.Category)
                .Where(c => c.Category != null)
                .GroupBy(c => c.Category!.CategoryName)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .FirstOrDefaultAsync();

            if (categoryStats != null)
            {
                statistics.MostCommonCategory = categoryStats.Category;
            }

            // Calculate most active employee
            var employeeStats = await _context.Cases
                .Include(c => c.CreatedBy)
                .Where(c => c.CreatedBy != null)
                .GroupBy(c => c.CreatedBy!.Name)
                .Select(g => new { Employee = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .FirstOrDefaultAsync();

            if (employeeStats != null)
            {
                statistics.MostActiveEmployee = employeeStats.Employee;
            }

            // Calculate total attachments and correspondences
            statistics.TotalAttachments = await _context.Attachments.CountAsync();
            statistics.TotalCorrespondences = await _context.Correspondences.CountAsync();

            // Calculate average resolution time (simplified)
            var solvedCases = allCases.Where(c => c.Status == "تم حلها" && c.SolvedDate.HasValue && c.CreatedDate.HasValue);
            if (solvedCases.Any())
            {
                var totalDays = solvedCases.Sum(c => (c.SolvedDate!.Value - c.CreatedDate!.Value).TotalDays);
                statistics.AverageResolutionTime = totalDays / solvedCases.Count();
            }

            return statistics;
        }
    }
}
