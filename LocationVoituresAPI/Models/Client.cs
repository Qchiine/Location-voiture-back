using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocationVoituresAPI.Models;

[Table("CLIENT")]
public class Client
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Required]
    [MaxLength(20)]
    [Column("TELEPHONE")]
    public string Telephone { get; set; } = string.Empty;

    [Required]
    [Column("ADRESSE", TypeName = "TEXT")]
    public string Adresse { get; set; } = string.Empty;

    [Required]
    [Column("DATEINSCRIPTION")]
    public DateTime DateInscription { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("NUMEROPERMIS")]
    public string NumeroPermis { get; set; } = string.Empty;

    // Navigation property
    [ForeignKey("Id")]
    public virtual Utilisateur Utilisateur { get; set; } = null!;

    // Relations
    public virtual ICollection<Location> Locations { get; set; } = new List<Location>();
}

