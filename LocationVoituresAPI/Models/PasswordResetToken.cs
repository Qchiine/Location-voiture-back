using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocationVoituresAPI.Models;

[Table("PASSWORDRESETTOKEN")]
public class PasswordResetToken
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Required]
    [Column("UTILISATEURID")]
    public int UtilisateurId { get; set; }

    [Required]
    [MaxLength(6)]
    [Column("CODE")]
    public string Code { get; set; } = string.Empty;

    [Required]
    [Column("DATEEXPIRATION")]
    public DateTime DateExpiration { get; set; }

    [Required]
    [Column("ESTUTILISE")]
    public bool EstUtilise { get; set; } = false;

    [Column("DATECREATION")]
    public DateTime DateCreation { get; set; } = DateTime.Now;

    // Navigation property
    [ForeignKey("UtilisateurId")]
    public virtual Utilisateur Utilisateur { get; set; } = null!;
}
