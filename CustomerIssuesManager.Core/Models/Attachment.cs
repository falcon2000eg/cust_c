using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomerIssuesManager.Core.Models;

/// <summary>
/// يمثل جدول المرفقات
/// </summary>
public class Attachment
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int CaseId { get; set; }
    [ForeignKey("CaseId")]
    public virtual Case? Case { get; set; }

    [Required]
    public string FileName { get; set; } = string.Empty;

    [Required]
    public string FilePath { get; set; } = string.Empty;

    public string? FileType { get; set; }
    public string? Description { get; set; }
    public DateTime UploadDate { get; set; } = DateTime.Now;

    public long? FileSize { get; set; } // الحجم بالبايت

    [NotMapped]
    public string? UploadedByName => UploadedBy?.Name;

    [Required]
    public int UploadedById { get; set; }
    [ForeignKey("UploadedById")]
    public virtual Employee? UploadedBy { get; set; }
}
