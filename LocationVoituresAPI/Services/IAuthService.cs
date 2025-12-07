using LocationVoituresAPI.DTOs;
using LocationVoituresAPI.Models;

namespace LocationVoituresAPI.Services;

public interface IAuthService
{
    Task<AuthResponseDto?> LoginAsync(LoginDto loginDto);
    Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto);
    Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken);
    string GenerateJwtToken(Utilisateur utilisateur);
    string GenerateRefreshToken();
    
    // Méthodes de réinitialisation de mot de passe
    Task<bool> SendPasswordResetCodeAsync(string email);
    Task<bool> VerifyResetCodeAsync(string email, string code);
    Task<bool> ResetPasswordAsync(string email, string code, string newPassword);
}

