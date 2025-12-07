using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocationVoituresAPI.Models;

[Table("ENTRETIEN")]
public class Entretien
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Required]
    [Column("DATEENTRETIEN")]
    public DateTime DateEntretien { get; set; }

    [Required]
    [Column("TYPEENTRETIEN")]
    public TypeEntretien TypeEntretien { get; set; }

    [Required]
    [Column("DESCRIPTION", TypeName = "TEXT")]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Column("COUT", TypeName = "DECIMAL(10,2)")]
    public decimal Cout { get; set; }

    [Column("PROCHAINENTRETIEN")]
    public DateTime? ProchainEntretien { get; set; }

    [Column("ESTURGENT")]
    public bool EstUrgent { get; set; } = false;

    [Required]
    [Column("STATUT")]
    public StatutEntretien Statut { get; set; } = StatutEntretien.PLANIFIE;

    [Required]
    [Column("VEHICULEID")]
    public int VehiculeId { get; set; }

    [Column("EMPLOYEID")]
    public int? EmployeId { get; set; }

    [Column("DATECREATION")]
    public DateTime DateCreation { get; set; } = DateTime.Now;

    // Navigation properties
    [ForeignKey("VehiculeId")]
    public virtual Vehicule Vehicule { get; set; } = null!;

    [ForeignKey("EmployeId")]
    public virtual Employe? Employe { get; set; }

    // Méthodes métier
    public void PlanifierEntretien(int joursAvance)
    {
        ProchainEntretien = DateEntretien.AddDays(joursAvance);
    }

    public void MarquerTermine()
    {
        Statut = StatutEntretien.TERMINE;
    }

    public decimal CalculerCoutTotal()
    {
        return Cout;
    }
}

