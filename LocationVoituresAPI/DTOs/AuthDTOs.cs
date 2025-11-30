using System.ComponentModel.DataAnnotations;
using LocationVoituresAPI.Models;

namespace LocationVoituresAPI.DTOs;

public class LoginDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string MotDePasse { get; set; } = string.Empty;
}

public class RegisterDto
{
    [Required]
    [MaxLength(100)]
    public string Nom { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Prenom { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string MotDePasse { get; set; } = string.Empty;

    [Required]
    public TypeUtilisateur TypeUtilisateur { get; set; }

    // Pour Client
    public string? Telephone { get; set; }
    public string? Adresse { get; set; }
    public string? NumeroPermis { get; set; }

    // Pour Employe
    public string? Matricule { get; set; }
    public DateTime? DateEmbauche { get; set; }
}

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
    public UserInfoDto User { get; set; } = null!;
}

public class UserInfoDto
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public TypeUtilisateur TypeUtilisateur { get; set; }
}

public class RefreshTokenDto
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}

