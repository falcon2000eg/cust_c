using CustomerIssuesManager.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace CustomerIssuesManager.Core.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Employee> Employees { get; set; }
    public DbSet<IssueCategory> IssueCategories { get; set; }
    public DbSet<Case> Cases { get; set; }
    public DbSet<Correspondence> Correspondences { get; set; }
    public DbSet<Attachment> Attachments { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    // OnConfiguring is removed

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- تكوين العلاقات ---

        // علاقة Case مع Employee (CreatedBy)
        modelBuilder.Entity<Case>()
            .HasOne(c => c.CreatedBy)
            .WithMany(e => e.CreatedCases)
            .HasForeignKey(c => c.CreatedById)
            .OnDelete(DeleteBehavior.Restrict); // منع حذف الموظف إذا كان قد أنشأ مشاكل

        // علاقة Case مع Employee (ModifiedBy)
        modelBuilder.Entity<Case>()
            .HasOne(c => c.ModifiedBy)
            .WithMany(e => e.ModifiedCases)
            .HasForeignKey(c => c.ModifiedById)
            .OnDelete(DeleteBehavior.SetNull); // عند حذف الموظف، اجعل الحقل فارغًا

        // علاقة Case مع Employee (SolvedBy)
        modelBuilder.Entity<Case>()
            .HasOne(c => c.SolvedBy)
            .WithMany(e => e.SolvedCases)
            .HasForeignKey(c => c.SolvedById)
            .OnDelete(DeleteBehavior.SetNull); // عند حذف الموظف، اجعل الحقل فارغًا

        // علاقة Correspondence مع Case
        modelBuilder.Entity<Correspondence>()
            .HasOne(corr => corr.Case)
            .WithMany(c => c.Correspondences)
            .HasForeignKey(corr => corr.CaseId)
                            .OnDelete(DeleteBehavior.Cascade); // حذف المراسلات عند حذف المشكلة

        // علاقة Attachment مع Case
        modelBuilder.Entity<Attachment>()
            .HasOne(att => att.Case)
            .WithMany(c => c.Attachments)
            .HasForeignKey(att => att.CaseId)
                            .OnDelete(DeleteBehavior.Cascade); // حذف المرفقات عند حذف المشكلة

        // علاقة AuditLog مع Case
        modelBuilder.Entity<AuditLog>()
            .HasOne(log => log.Case)
            .WithMany(c => c.AuditLogs)
            .HasForeignKey(log => log.CaseId)
                            .OnDelete(DeleteBehavior.Cascade); // حذف سجل التدقيق عند حذف المشكلة
            
        // --- القيود الفريدة ---
        modelBuilder.Entity<Employee>()
            .HasIndex(e => e.PerformanceNumber)
            .IsUnique();

        modelBuilder.Entity<IssueCategory>()
            .HasIndex(ic => ic.CategoryName)
            .IsUnique();

        // --- البيانات الأولية (Seed Data) ---

        // إضافة موظف افتراضي
        modelBuilder.Entity<Employee>().HasData(
            new Employee { Id = 1, Name = "مدير النظام", Position = "Admin", PerformanceNumber = "1", CreatedDate = new System.DateTime(2024, 1, 1) }
        );

        // إضافة تصنيفات مشاكل افتراضية (محسنة لتطابق النسخة الأصلية)
        modelBuilder.Entity<IssueCategory>().HasData(
            new IssueCategory { Id = 1, CategoryName = "عبث بالعداد", Description = "التلاعب في قراءات العداد أو كسره", ColorCode = "#e74c3c" },
            new IssueCategory { Id = 2, CategoryName = "توصيلات غير شرعية", Description = "توصيلات غاز غير مرخصة", ColorCode = "#e67e22" },
            new IssueCategory { Id = 3, CategoryName = "خطأ قراءة", Description = "خطأ في قراءة العداد", ColorCode = "#f39c12" },
            new IssueCategory { Id = 4, CategoryName = "مشكلة فواتير", Description = "مشاكل في الفواتير والمدفوعات", ColorCode = "#9b59b6" },
            new IssueCategory { Id = 5, CategoryName = "تغيير نشاط", Description = "طلب تغيير نوع النشاط", ColorCode = "#3498db" },
            new IssueCategory { Id = 6, CategoryName = "تصحيح رقم عداد", Description = "تصحيح أرقام العدادات", ColorCode = "#1abc9c" },
            new IssueCategory { Id = 7, CategoryName = "نقل رقم مشترك", Description = "نقل الاشتراك لموقع آخر", ColorCode = "#2ecc71" },
            new IssueCategory { Id = 8, CategoryName = "كسر بالشاشة", Description = "كسر أو تلف شاشة العداد", ColorCode = "#e74c3c" },
            new IssueCategory { Id = 9, CategoryName = "عطل عداد", Description = "أعطال فنية في العداد", ColorCode = "#c0392b" },
            new IssueCategory { Id = 10, CategoryName = "هدم وازالة", Description = "طلبات هدم أو إزالة التوصيلات", ColorCode = "#7f8c8d" },
            new IssueCategory { Id = 11, CategoryName = "أخرى", Description = "مشاكل أخرى غير مصنفة", ColorCode = "#95a5a6" }
        );
    }
}
