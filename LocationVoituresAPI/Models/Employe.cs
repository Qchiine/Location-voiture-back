using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocationVoituresAPI.Models;

[Table("EMPLOYE")]
public class Employe
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("MATRICULE")]
    public string Matricule { get; set; } = string.Empty;

    [Required]
    [Column("DATEEMBAUCHE")]
    public DateTime DateEmbauche { get; set; }

    // Navigation property
    [ForeignKey("Id")]
    public virtual Utilisateur Utilisateur { get; set; } = null!;

    // Relations
    public virtual ICollection<Location> Locations { get; set; } = new List<Location>();
    public virtual ICollection<Entretien> Entretiens { get; set; } = new List<Entretien>();
}

