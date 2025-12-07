using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace LocationVoituresAPI.Models;

[Table("TYPEVEHICULE")]
public class TypeVehicule
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("NOM")]
    public string Nom { get; set; } = string.Empty;

    [Column("DESCRIPTION", TypeName = "TEXT")]
    public string? Description { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("CATEGORIE")]
    public string Categorie { get; set; } = string.Empty;

    [Required]
    [Column("PRIXBASEJOURNALIER", TypeName = "DECIMAL(10,2)")]
    public decimal PrixBaseJournalier { get; set; }

    [Column("DATECREATION")]
    public DateTime DateCreation { get; set; } = DateTime.Now;

    // Relations
    [JsonIgnore]
    public virtual ICollection<Vehicule> Vehicules { get; set; } = new List<Vehicule>();
}

