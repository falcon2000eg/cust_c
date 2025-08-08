namespace CustomerIssuesManager.Core.Models;

/// <summary>
/// معايير البحث الشامل للحالات
/// </summary>
public class SearchCriteria
{
    public string? SearchField { get; set; }
    public string? SearchValue { get; set; }
    public int? Year { get; set; }
    public string? DateField { get; set; } = "CreatedDate";
    public string? CustomerName { get; set; }
    public string? SubscriberNumber { get; set; }
    public string? Address { get; set; }
    public string? Status { get; set; }
    public string? Category { get; set; }
    public string? CorrespondenceText { get; set; }
    public string? AttachmentText { get; set; }
    public string? EmployeeName { get; set; }
    public string? ProblemDescription { get; set; }
    public string? ActionsTaken { get; set; }
}

/// <summary>
/// أنواع البحث المتاحة
/// </summary>
public static class SearchFieldTypes
{
    public const string Comprehensive = "شامل";
    public const string CustomerName = "اسم العميل";
    public const string SubscriberNumber = "رقم المشترك";
    public const string Address = "العنوان";
    public const string Category = "تصنيف المشكلة";
    public const string Status = "حالة المشكلة";
    public const string EmployeeName = "اسم الموظف";
    public const string ProblemDescription = "وصف المشكلة";
    public const string ActionsTaken = "الإجراءات المتخذة";
}

/// <summary>
/// حقول التاريخ المتاحة للفلترة
/// </summary>
public static class DateFieldTypes
{
    public const string CreatedDate = "CreatedDate";
    public const string ReceivedDate = "ReceivedDate";
    public const string ModifiedDate = "ModifiedDate";
    public const string SolvedDate = "SolvedDate";
} 