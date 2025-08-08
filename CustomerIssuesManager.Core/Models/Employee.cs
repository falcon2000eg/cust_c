using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomerIssuesManager.Core.Models;

/// <summary>
/// يمثل جدول الموظفين
/// </summary>
public class Employee
{
    [Key]
    public int Id { get; set; }

    [Required]
    public required string Name { get; set; }

    public string? Position { get; set; }

    [Required]
    public required string PerformanceNumber { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.Now;

    public bool IsActive { get; set; } = true;

    // Navigation Properties
    public virtual ICollection<Case> CreatedCases { get; set; } = new List<Case>();
    public virtual ICollection<Case> ModifiedCases { get; set; } = new List<Case>();
    public virtual ICollection<Case> SolvedCases { get; set; } = new List<Case>();
    public virtual ICollection<Correspondence> Correspondences { get; set; } = new List<Correspondence>();
    public virtual ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}
