using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ipa.Manager.Models;

[Table("criteria_progress")]
public class CriteriaProgress
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("project_id")]
    public int ProjectId { get; set; }

    [Required]
    [Column("criteria_id")]
    [MaxLength(50)]
    public string CriteriaId { get; set; } = string.Empty;

    // We will configure this to map to JSON in DbContext
    [Column("fulfilled_requirement_ids")]
    public List<int> FulfilledRequirementIds { get; set; } = new List<int>();

    [Column("notes")]
    [MaxLength(1000)]
    public string? Notes { get; set; }

    [Column("last_updated")]
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    [ForeignKey("ProjectId")]
    public Project Project { get; set; } = null!;
}
