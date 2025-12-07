using LocationVoituresAPI.Data;
using LocationVoituresAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocationVoituresAPI.Controllers;

/// <summary>
/// Contrôleur pour les opérations de setup initial
/// ⚠️ À DÉSACTIVER EN PRODUCTION - Seulement pour le développement
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SetupController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SetupController> _logger;

    public SetupController(ApplicationDbContext context, ILogger<SetupController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Créer un compte administrateur par défaut
    /// GET: api/Setup/create-admin
    /// </summary>
    [HttpPost("create-admin")]
    public async Task<IActionResult> CreateDefaultAdmin()
    {
        try
        {
            string adminEmail = "admin@location-voitures.fr";
            
            // Vérifier si un admin existe déjà
            var existingAdmin = await _context.Utilisateurs
                .FirstOrDefaultAsync(u => u.Email == adminEmail);

            if (existingAdmin != null)
            {
                return BadRequest(new { 
                    message = "Un administrateur avec cet email existe déjà",
                    email = adminEmail
                });
            }

            // Créer l'admin
            string password = "Admin@123";
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            var adminUser = new Utilisateur
            {
                Nom = "Admin",
                Prenom = "Système",
                Email = adminEmail,
                MotDePasseHash = passwordHash,
                DateCreation = DateTime.Now,
                EstActif = true,
                TypeUtilisateur = TypeUtilisateur.ADMINISTRATEUR
            };

            _context.Utilisateurs.Add(adminUser);
            await _context.SaveChangesAsync();

            // Créer l'employé associé
            var employe = new Employe
            {
                Id = adminUser.Id,
                Matricule = $"EMP{adminUser.Id:D5}",
                DateEmbauche = DateTime.Now,
                Utilisateur = adminUser
            };

            _context.Employes.Add(employe);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Compte administrateur créé: {Email}", adminEmail);

            return Ok(new
            {
                message = "Compte administrateur créé avec succès",
                email = adminEmail,
                password = password,
                matricule = employe.Matricule,
                warning = "⚠️ CHANGEZ CE MOT DE PASSE IMMÉDIATEMENT!"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la création de l'admin");
            return StatusCode(500, new { message = "Erreur lors de la création de l'admin" });
        }
    }

    /// <summary>
    /// Créer un administrateur personnalisé
    /// POST: api/Setup/create-custom-admin
    /// </summary>
    [HttpPost("create-custom-admin")]
    public async Task<IActionResult> CreateCustomAdmin([FromBody] CreateAdminRequest request)
    {
        try
        {
            // Vérifier si l'email existe déjà
            var existingUser = await _context.Utilisateurs
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (existingUser != null)
            {
                return BadRequest(new { 
                    message = "Un utilisateur avec cet email existe déjà",
                    email = request.Email
                });
            }

            // Hasher le mot de passe
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Créer l'utilisateur
            var adminUser = new Utilisateur
            {
                Nom = request.Nom,
                Prenom = request.Prenom,
                Email = request.Email,
                MotDePasseHash = passwordHash,
                DateCreation = DateTime.Now,
                EstActif = true,
                TypeUtilisateur = TypeUtilisateur.ADMINISTRATEUR
            };

            _context.Utilisateurs.Add(adminUser);
            await _context.SaveChangesAsync();

            // Créer l'employé associé
            var employe = new Employe
            {
                Id = adminUser.Id,
                Matricule = $"EMP{adminUser.Id:D5}",
                DateEmbauche = DateTime.Now,
                Utilisateur = adminUser
            };

            _context.Employes.Add(employe);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Compte administrateur créé: {Email}", request.Email);

            return Ok(new
            {
                message = "Compte administrateur créé avec succès",
                email = request.Email,
                nom = $"{request.Prenom} {request.Nom}",
                matricule = employe.Matricule
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la création de l'admin personnalisé");
            return StatusCode(500, new { message = "Erreur lors de la création de l'admin" });
        }
    }
}

public class CreateAdminRequest
{
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
