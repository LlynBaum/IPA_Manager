using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ipa.Manager.Models;

[Table("projects")]
public class Project
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Required]
    [Column("name")]
    [MaxLength(255)]
    public required string Name { get; set; }

    [Required]
    [Column("topic")]
    [MaxLength(255)]
    public required string Topic { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("UserId")]
    public User User { get; set; } = null!;

    public ICollection<CriteriaProgress> CriteriaProgress { get; set; } = [];
}
