using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocationVoituresAPI.Models;

[Table("PAIEMENT")]
public class Paiement
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Required]
    [Column("MONTANT", TypeName = "DECIMAL(10,2)")]
    public decimal Montant { get; set; }

    [Required]
    [Column("DATEPAIEMENT")]
    public DateTime DatePaiement { get; set; }

    [Required]
    [Column("MODEPAIEMENT")]
    public ModePaiement ModePaiement { get; set; }

    [Required]
    [Column("STATUT")]
    public StatutPaiement Statut { get; set; } = StatutPaiement.EN_ATTENTE;

    [MaxLength(100)]
    [Column("REFERENCE")]
    public string? Reference { get; set; }

    [Required]
    [Column("LOCATIONID")]
    public int LocationId { get; set; }

    [Column("DATECREATION")]
    public DateTime DateCreation { get; set; } = DateTime.Now;

    // Navigation property
    [ForeignKey("LocationId")]
    public virtual Location Location { get; set; } = null!;
}

