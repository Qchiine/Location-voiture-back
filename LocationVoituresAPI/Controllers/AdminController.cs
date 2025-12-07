using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LocationVoituresAPI.Data;
using LocationVoituresAPI.DTOs;
using LocationVoituresAPI.Models;
using LocationVoituresAPI.Services;

namespace LocationVoituresAPI.Controllers;

/// <summary>
/// Contrôleur pour la gestion des administrateurs
/// Endpoint temporaire pour créer le premier administrateur
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IAuthService _authService;

    public AdminController(ApplicationDbContext context, IAuthService authService)
    {
        _context = context;
        _authService = authService;
    }

    /// <summary>
    /// Créer le premier administrateur (endpoint public - à sécuriser après création)
    /// ATTENTION: Désactivez cet endpoint après avoir créé votre premier admin!
    /// </summary>
    [HttpPost("create-first-admin")]
    public async Task<ActionResult> CreateFirstAdmin([FromBody] CreateAdminDto createAdminDto)
    {
        // Vérifier s'il existe déjà un administrateur
        var adminExists = await _context.Utilisateurs
            .AnyAsync(u => u.TypeUtilisateur == TypeUtilisateur.ADMINISTRATEUR);

        if (adminExists)
        {
            return BadRequest("Un administrateur existe déjà. Utilisez l'endpoint sécurisé pour créer d'autres admins.");
        }

        // Créer l'administrateur
        var utilisateur = new Utilisateur
        {
            Nom = createAdminDto.Nom,
            Prenom = createAdminDto.Prenom,
            Email = createAdminDto.Email,
            MotDePasseHash = BCrypt.Net.BCrypt.HashPassword(createAdminDto.MotDePasse),
            TypeUtilisateur = TypeUtilisateur.ADMINISTRATEUR,
            DateCreation = DateTime.Now,
            EstActif = true
        };

        _context.Utilisateurs.Add(utilisateur);
        await _context.SaveChangesAsync();

        var token = _authService.GenerateJwtToken(utilisateur);

        return Ok(new
        {
            Message = "Administrateur créé avec succès",
            Token = token,
            User = new
            {
                Id = utilisateur.Id,
                Nom = utilisateur.Nom,
                Prenom = utilisateur.Prenom,
                Email = utilisateur.Email,
                TypeUtilisateur = utilisateur.TypeUtilisateur
            }
        });
    }

    /// <summary>
    /// Créer un autre administrateur (nécessite d'être admin)
    /// </summary>
    [HttpPost("create-admin")]
    [Authorize(Roles = "ADMINISTRATEUR")]
    public async Task<ActionResult> CreateAdmin([FromBody] CreateAdminDto createAdminDto)
    {
        if (await _context.Utilisateurs.AnyAsync(u => u.Email == createAdminDto.Email))
        {
            return BadRequest("Un utilisateur avec cet email existe déjà.");
        }

        var utilisateur = new Utilisateur
        {
            Nom = createAdminDto.Nom,
            Prenom = createAdminDto.Prenom,
            Email = createAdminDto.Email,
            MotDePasseHash = BCrypt.Net.BCrypt.HashPassword(createAdminDto.MotDePasse),
            TypeUtilisateur = TypeUtilisateur.ADMINISTRATEUR,
            DateCreation = DateTime.Now,
            EstActif = true
        };

        _context.Utilisateurs.Add(utilisateur);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            Message = "Administrateur créé avec succès",
            User = new
            {
                Id = utilisateur.Id,
                Nom = utilisateur.Nom,
                Prenom = utilisateur.Prenom,
                Email = utilisateur.Email
            }
        });
    }
}

/// <summary>
/// DTO pour créer un administrateur
/// </summary>
public class CreateAdminDto
{
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string MotDePasse { get; set; } = string.Empty;
}

