using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocationVoituresAPI.Models;

[Table("UTILISATEUR")]
public class Utilisateur
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("NOM")]
    public string Nom { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [Column("PRENOM")]
    public string Prenom { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    [EmailAddress]
    [Column("EMAIL")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    [Column("MOTDEPASSEHASH")]
    public string MotDePasseHash { get; set; } = string.Empty;

    [Column("DATECREATION")]
    public DateTime DateCreation { get; set; } = DateTime.Now;

    [Column("ESTACTIF")]
    public bool EstActif { get; set; } = true;

    [Required]
    [Column("TYPEUTILISATEUR")]
    public TypeUtilisateur TypeUtilisateur { get; set; }

    // Navigation properties
    public virtual Employe? Employe { get; set; }
    public virtual Client? Client { get; set; }
}

