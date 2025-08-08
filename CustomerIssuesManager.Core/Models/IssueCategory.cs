using System.ComponentModel.DataAnnotations;

namespace CustomerIssuesManager.Core.Models;

/// <summary>
/// يمثل جدول تصنيفات المشاكل
/// </summary>
public class IssueCategory
{
    [Key]
    public int Id { get; set; }

    [Required]
    public required string CategoryName { get; set; }

    public string? Description { get; set; }

    public string? ColorCode { get; set; }

    // Navigation Property
    public virtual ICollection<Case> Cases { get; set; } = new List<Case>();
}
