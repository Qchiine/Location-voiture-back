using System.ComponentModel.DataAnnotations;

namespace LocationVoituresAPI.DTOs;

public class ForgotPasswordDto
{
    [Required(ErrorMessage = "L'email est requis")]
    [EmailAddress(ErrorMessage = "Format d'email invalide")]
    public string Email { get; set; } = string.Empty;
}

public class VerifyResetCodeDto
{
    [Required(ErrorMessage = "L'email est requis")]
    [EmailAddress(ErrorMessage = "Format d'email invalide")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le code est requis")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Le code doit contenir 6 chiffres")]
    public string Code { get; set; } = string.Empty;
}

public class ResetPasswordDto
{
    [Required(ErrorMessage = "L'email est requis")]
    [EmailAddress(ErrorMessage = "Format d'email invalide")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le code est requis")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Le code doit contenir 6 chiffres")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le nouveau mot de passe est requis")]
    [MinLength(6, ErrorMessage = "Le mot de passe doit contenir au moins 6 caractères")]
    public string NewPassword { get; set; } = string.Empty;
}
