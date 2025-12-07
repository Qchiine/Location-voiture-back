using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using LocationVoituresAPI.Data;
using LocationVoituresAPI.DTOs;
using LocationVoituresAPI.Models;

namespace LocationVoituresAPI.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;

    public AuthService(ApplicationDbContext context, IConfiguration configuration, IEmailService emailService)
    {
        _context = context;
        _configuration = configuration;
        _emailService = emailService;
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
    {
        var utilisateur = await _context.Utilisateurs
            .FirstOrDefaultAsync(u => u.Email == loginDto.Email && u.EstActif);

        if (utilisateur == null)
            return null;

        if (!BCrypt.Net.BCrypt.Verify(loginDto.MotDePasse, utilisateur.MotDePasseHash))
            return null;

        var token = GenerateJwtToken(utilisateur);
        var refreshToken = GenerateRefreshToken();

        return new AuthResponseDto
        {
            Token = token,
            RefreshToken = refreshToken,
            Expiration = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:ExpirationInMinutes"] ?? "1440")),
            User = new UserInfoDto
            {
                Id = utilisateur.Id,
                Nom = utilisateur.Nom,
                Prenom = utilisateur.Prenom,
                Email = utilisateur.Email,
                TypeUtilisateur = utilisateur.TypeUtilisateur
            }
        };
    }

    public async Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto)
    {
        if (await _context.Utilisateurs.AnyAsync(u => u.Email == registerDto.Email))
            return null;

        // Vérification explicite du type - Seuls CLIENT et EMPLOYE sont autorisés
        if (registerDto.TypeUtilisateur != TypeUtilisateur.CLIENT && 
            registerDto.TypeUtilisateur != TypeUtilisateur.EMPLOYE)
        {
            return null; // Type non autorisé (ADMINISTRATEUR ne peut pas être créé via cette méthode)
        }

        var utilisateur = new Utilisateur
        {
            Nom = registerDto.Nom,
            Prenom = registerDto.Prenom,
            Email = registerDto.Email,
            MotDePasseHash = BCrypt.Net.BCrypt.HashPassword(registerDto.MotDePasse),
            TypeUtilisateur = registerDto.TypeUtilisateur, // Le type est forcé dans le contrôleur
            DateCreation = DateTime.Now,
            EstActif = true
        };

        _context.Utilisateurs.Add(utilisateur);
        await _context.SaveChangesAsync(); // Sauvegarder d'abord pour obtenir l'ID

        // Créer le profil spécifique selon le type
        if (registerDto.TypeUtilisateur == TypeUtilisateur.CLIENT)
        {
            if (string.IsNullOrEmpty(registerDto.Telephone) || 
                string.IsNullOrEmpty(registerDto.Adresse) || 
                string.IsNullOrEmpty(registerDto.NumeroPermis))
            {
                // Supprimer l'utilisateur créé si les données client sont invalides
                _context.Utilisateurs.Remove(utilisateur);
                await _context.SaveChangesAsync();
                return null;
            }

            var client = new Client
            {
                Id = utilisateur.Id,
                Telephone = registerDto.Telephone,
                Adresse = registerDto.Adresse,
                NumeroPermis = registerDto.NumeroPermis,
                DateInscription = DateTime.Now
            };
            _context.Clients.Add(client);
        }
        else if (registerDto.TypeUtilisateur == TypeUtilisateur.EMPLOYE)
        {
            if (string.IsNullOrEmpty(registerDto.Matricule) || !registerDto.DateEmbauche.HasValue)
            {
                // Supprimer l'utilisateur créé si les données employé sont invalides
                _context.Utilisateurs.Remove(utilisateur);
                await _context.SaveChangesAsync();
                return null;
            }

            var employe = new Employe
            {
                Id = utilisateur.Id,
                Matricule = registerDto.Matricule,
                DateEmbauche = registerDto.DateEmbauche.Value
            };
            _context.Employes.Add(employe);
        }

        await _context.SaveChangesAsync();

        // Recharger l'utilisateur depuis la base pour s'assurer du type
        utilisateur = await _context.Utilisateurs.FindAsync(utilisateur.Id);
        if (utilisateur == null)
            return null;

        var token = GenerateJwtToken(utilisateur);
        var refreshToken = GenerateRefreshToken();

        return new AuthResponseDto
        {
            Token = token,
            RefreshToken = refreshToken,
            Expiration = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:ExpirationInMinutes"] ?? "1440")),
            User = new UserInfoDto
            {
                Id = utilisateur.Id,
                Nom = utilisateur.Nom,
                Prenom = utilisateur.Prenom,
                Email = utilisateur.Email,
                TypeUtilisateur = utilisateur.TypeUtilisateur
            }
        };
    }

    public async Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken)
    {
        // Dans une implémentation complète, on vérifierait le refresh token en base
        // Pour simplifier, on accepte n'importe quel refresh token valide
        // TODO: Implémenter la table RefreshTokens en base de données
        return await Task.FromResult<AuthResponseDto?>(null);
    }

    public string GenerateJwtToken(Utilisateur utilisateur)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured")));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, utilisateur.Id.ToString()),
            new Claim(ClaimTypes.Email, utilisateur.Email),
            new Claim(ClaimTypes.Name, $"{utilisateur.Prenom} {utilisateur.Nom}"),
            new Claim(ClaimTypes.Role, utilisateur.TypeUtilisateur.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:ExpirationInMinutes"] ?? "1440")),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public async Task<bool> SendPasswordResetCodeAsync(string email)
    {
        var utilisateur = await _context.Utilisateurs
            .FirstOrDefaultAsync(u => u.Email == email && u.EstActif);

        if (utilisateur == null)
            return false;

        // Supprimer les anciens codes non utilisés pour cet utilisateur
        var oldTokens = await _context.PasswordResetTokens
            .Where(t => t.UtilisateurId == utilisateur.Id && !t.EstUtilise)
            .ToListAsync();
        _context.PasswordResetTokens.RemoveRange(oldTokens);

        // Générer un code à 6 chiffres
        var random = new Random();
        var code = random.Next(100000, 999999).ToString();

        // Créer le token
        var resetToken = new PasswordResetToken
        {
            UtilisateurId = utilisateur.Id,
            Code = code,
            DateExpiration = DateTime.Now.AddMinutes(15), // Valide 15 minutes
            EstUtilise = false
        };

        _context.PasswordResetTokens.Add(resetToken);
        await _context.SaveChangesAsync();

        // Envoyer l'email
        await _emailService.EnvoyerCodeResetPasswordAsync(
            utilisateur.Email, 
            utilisateur.Prenom, 
            code
        );

        return true;
    }

    public async Task<bool> VerifyResetCodeAsync(string email, string code)
    {
        var utilisateur = await _context.Utilisateurs
            .FirstOrDefaultAsync(u => u.Email == email && u.EstActif);

        if (utilisateur == null)
            return false;

        var resetToken = await _context.PasswordResetTokens
            .Where(t => t.UtilisateurId == utilisateur.Id 
                     && t.Code == code 
                     && !t.EstUtilise 
                     && t.DateExpiration > DateTime.Now)
            .FirstOrDefaultAsync();

        return resetToken != null;
    }

    public async Task<bool> ResetPasswordAsync(string email, string code, string newPassword)
    {
        var utilisateur = await _context.Utilisateurs
            .FirstOrDefaultAsync(u => u.Email == email && u.EstActif);

        if (utilisateur == null)
            return false;

        var resetToken = await _context.PasswordResetTokens
            .Where(t => t.UtilisateurId == utilisateur.Id 
                     && t.Code == code 
                     && !t.EstUtilise 
                     && t.DateExpiration > DateTime.Now)
            .FirstOrDefaultAsync();

        if (resetToken == null)
            return false;

        // Mettre à jour le mot de passe
        utilisateur.MotDePasseHash = BCrypt.Net.BCrypt.HashPassword(newPassword);

        // Marquer le token comme utilisé
        resetToken.EstUtilise = true;

        await _context.SaveChangesAsync();

        return true;
    }
}

