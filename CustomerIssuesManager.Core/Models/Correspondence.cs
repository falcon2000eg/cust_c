using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomerIssuesManager.Core.Models;

/// <summary>
/// يمثل جدول المراسلات
/// </summary>
public class Correspondence
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int CaseId { get; set; }
    [ForeignKey("CaseId")]
    public virtual Case? Case { get; set; }

    [Required]
    public int CaseSequenceNumber { get; set; }

    [Required]
    public string YearlySequenceNumber { get; set; } = string.Empty;

    public string? Sender { get; set; }

    [Required]
    public string MessageContent { get; set; } = string.Empty;

    public DateTime SentDate { get; set; } = DateTime.Now;

    [Required]
    public int CreatedById { get; set; }
    [ForeignKey("CreatedById")]
    public virtual Employee? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.Now;
}
