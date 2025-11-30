using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocationVoituresAPI.Models;

[Table("LOCATION")]
public class Location
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Required]
    [Column("DATEDEBUT")]
    public DateTime DateDebut { get; set; }

    [Required]
    [Column("DATEFIN")]
    public DateTime DateFin { get; set; }

    [Required]
    [Column("DUREEJOURS")]
    public int DureeJours { get; set; }

    [Required]
    [Column("MONTANTTOTAL", TypeName = "DECIMAL(10,2)")]
    public decimal MontantTotal { get; set; }

    [Required]
    [Column("STATUT")]
    public StatutLocation Statut { get; set; } = StatutLocation.EN_ATTENTE;

    [Column("QRCODE", TypeName = "TEXT")]
    public string? QRCode { get; set; }

    [Column("FACTUREURL", TypeName = "TEXT")]
    public string? FactureUrl { get; set; }

    [Required]
    [Column("CLIENTID")]
    public int ClientId { get; set; }

    [Required]
    [Column("VEHICULEID")]
    public int VehiculeId { get; set; }

    [Column("EMPLOYEID")]
    public int? EmployeId { get; set; }

    [Column("DATECREATION")]
    public DateTime DateCreation { get; set; } = DateTime.Now;

    // Navigation properties
    [ForeignKey("ClientId")]
    public virtual Client Client { get; set; } = null!;

    [ForeignKey("VehiculeId")]
    public virtual Vehicule Vehicule { get; set; } = null!;

    [ForeignKey("EmployeId")]
    public virtual Employe? Employe { get; set; }

    // Relations
    public virtual Paiement? Paiement { get; set; }

    // Méthodes métier
    public void CalculerMontant(Vehicule vehicule)
    {
        DureeJours = (DateFin - DateDebut).Days;
        MontantTotal = vehicule.CalculerPrixLocation(DureeJours);
    }

    public bool VerifierDisponibilite(Vehicule vehicule)
    {
        return vehicule.VerifierDisponibilite(DateDebut, DateFin);
    }
}

