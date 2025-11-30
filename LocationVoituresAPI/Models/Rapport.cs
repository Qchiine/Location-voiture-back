using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocationVoituresAPI.Models;

[Table("RAPPORT")]
public class Rapport
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Required]
    [Column("TYPERAPPORT")]
    public TypeRapport TypeRapport { get; set; }

    [Column("DATEGENERATION")]
    public DateTime DateGeneration { get; set; } = DateTime.Now;

    [Required]
    [Column("DONNEES", TypeName = "JSON")]
    public string Donnees { get; set; } = string.Empty;

    [Required]
    [Column("FORMAT")]
    public FormatRapport Format { get; set; } = FormatRapport.PDF;

    [Required]
    [Column("ADMINISTRATEURID")]
    public int AdministrateurId { get; set; }

    [MaxLength(255)]
    [Column("NOMFICHIER")]
    public string? NomFichier { get; set; }

    // Navigation property
    [ForeignKey("AdministrateurId")]
    public virtual Utilisateur Administrateur { get; set; } = null!;
}

