using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace LocationVoituresAPI.Models;

[Table("VEHICULE")]
public class Vehicule
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("MARQUE")]
    public string Marque { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [Column("MODELE")]
    public string Modele { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    [Column("IMMATRICULATION")]
    public string Immatriculation { get; set; } = string.Empty;

    [Required]
    [Column("ANNEE")]
    public int Annee { get; set; }

    [MaxLength(50)]
    [Column("COULEUR")]
    public string? Couleur { get; set; }

    [Required]
    [Column("PRIXJOURNALIER", TypeName = "DECIMAL(10,2)")]
    public decimal PrixJournalier { get; set; }

    [Column("ESTDISPONIBLE")]
    public bool EstDisponible { get; set; } = true;

    [Column("KILOMETRAGE")]
    public int Kilometrage { get; set; } = 0;

    [Column("IMAGEPRINCIPALEURL", TypeName = "TEXT")]
    public string? ImagePrincipaleUrl { get; set; }

    [Column("IMAGESURLS", TypeName = "JSON")]
    public string? ImagesUrls { get; set; }

    [Required]
    [Column("TYPEVEHICULEID")]
    public int TypeVehiculeId { get; set; }

    [Column("DATEAJOUT")]
    public DateTime DateAjout { get; set; } = DateTime.Now;

    // Navigation properties
    [ForeignKey("TypeVehiculeId")]
    public virtual TypeVehicule? TypeVehicule { get; set; }

    // Relations
    [JsonIgnore]
    public virtual ICollection<Location> Locations { get; set; } = new List<Location>();
    [JsonIgnore]
    public virtual ICollection<Entretien> Entretiens { get; set; } = new List<Entretien>();

    // Méthodes métier
    public bool VerifierDisponibilite(DateTime dateDebut, DateTime dateFin)
    {
        if (!EstDisponible) return false;

        return !Locations.Any(l => 
            l.Statut != StatutLocation.ANNULEE && 
            l.Statut != StatutLocation.TERMINEE &&
            dateDebut < l.DateFin && 
            dateFin > l.DateDebut);
    }

    public decimal CalculerPrixLocation(int dureeJours)
    {
        return PrixJournalier * dureeJours;
    }
}

