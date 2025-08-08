using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomerIssuesManager.Core.Models;

/// <summary>
    /// يمثل جدول المشاكل الرئيسي
/// </summary>
public class Case
{
    [Key]
    public int Id { get; set; }

    [Required]
    public required string CustomerName { get; set; }
    public string? SubscriberNumber { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }

    [Required]
    public int CategoryId { get; set; }
    [ForeignKey("CategoryId")]
    public required virtual IssueCategory Category { get; set; }

    [Required]
    public required string Status { get; set; }

    public string? ProblemDescription { get; set; }
    public string? ActionsTaken { get; set; }
    public decimal? LastMeterReading { get; set; }
    public DateTime? LastReadingDate { get; set; }
    public decimal? DebtAmount { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public DateTime? CreatedDate { get; set; } = DateTime.Now;

    [Required]
    public int CreatedById { get; set; }
    [ForeignKey("CreatedById")]
    public required virtual Employee CreatedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }
    public int? ModifiedById { get; set; }
    [ForeignKey("ModifiedById")]
    public virtual Employee? ModifiedBy { get; set; }

    public DateTime? SolvedDate { get; set; }
    public int? SolvedById { get; set; }
    [ForeignKey("SolvedById")]
    public virtual Employee? SolvedBy { get; set; }

    // Navigation Properties
    public virtual ICollection<Correspondence> Correspondences { get; set; } = new List<Correspondence>();
    public virtual ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}
