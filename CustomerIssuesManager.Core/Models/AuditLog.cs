using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomerIssuesManager.Core.Models;

/// <summary>
/// يمثل جدول سجل التدقيق
/// </summary>
public class AuditLog
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int CaseId { get; set; }
    [ForeignKey("CaseId")]
    public required virtual Case Case { get; set; }

    [Required]
    public required string ActionType { get; set; }
    public string? ActionDescription { get; set; }

    [Required]
    public int PerformedById { get; set; }
    [ForeignKey("PerformedById")]
    public required virtual Employee PerformedBy { get; set; }
    
    public string? PerformedByName { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.Now;

    public string? OldValues { get; set; } // Can be stored as JSON
    public string? NewValues { get; set; } // Can be stored as JSON
}
