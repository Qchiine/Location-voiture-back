using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LocationVoituresAPI.Data;
using LocationVoituresAPI.Models;
using LocationVoituresAPI.DTOs;

namespace LocationVoituresAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ClientsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ClientsController> _logger;

    public ClientsController(ApplicationDbContext context, ILogger<ClientsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Récupérer le profil du client connecté
    /// </summary>
    [HttpGet("me")]
    public async Task<ActionResult<ClientProfileDto>> GetCurrentClientProfile()
    {
        // Extraire l'ID du utilisateur depuis le token JWT
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized("Utilisateur non identifié");
        }

        var client = await _context.Clients
            .Include(c => c.Utilisateur)
            .Include(c => c.Locations)
                .ThenInclude(l => l.Vehicule)
            .FirstOrDefaultAsync(c => c.Id == userId);

        if (client == null)
        {
            return NotFound("Profil client non trouvé");
        }

        return Ok(MapToDto(client));
    }

    /// <summary>
    /// Récupérer le profil d'un client par ID (Admin/Employé)
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "ADMINISTRATEUR,EMPLOYE")]
    public async Task<ActionResult<ClientProfileDto>> GetClientProfile(int id)
    {
        var client = await _context.Clients
            .Include(c => c.Utilisateur)
            .Include(c => c.Locations)
                .ThenInclude(l => l.Vehicule)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (client == null)
        {
            return NotFound("Client non trouvé");
        }

        return Ok(MapToDto(client));
    }

    /// <summary>
    /// Mettre à jour le profil du client connecté
    /// </summary>
    [HttpPut("me")]
    public async Task<IActionResult> UpdateCurrentClientProfile([FromBody] UpdateClientProfileDto dto)
    {
        // Extraire l'ID du utilisateur depuis le token JWT
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized("Utilisateur non identifié");
        }

        var client = await _context.Clients
            .Include(c => c.Utilisateur)
            .FirstOrDefaultAsync(c => c.Id == userId);

        if (client == null)
        {
            return NotFound("Client non trouvé");
        }

        // Mettre à jour les données du client
        if (!string.IsNullOrEmpty(dto.Telephone))
            client.Telephone = dto.Telephone;

        if (!string.IsNullOrEmpty(dto.Adresse))
            client.Adresse = dto.Adresse;

        if (!string.IsNullOrEmpty(dto.NumeroPermis))
            client.NumeroPermis = dto.NumeroPermis;

        // Mettre à jour les données de l'utilisateur
        if (client.Utilisateur != null)
        {
            if (!string.IsNullOrEmpty(dto.Nom))
                client.Utilisateur.Nom = dto.Nom;

            if (!string.IsNullOrEmpty(dto.Prenom))
                client.Utilisateur.Prenom = dto.Prenom;
        }

        try
        {
            await _context.SaveChangesAsync();
            
            // Retourner le profil mis à jour
            var profileDto = new ClientProfileDto
            {
                Id = client.Id,
                Nom = client.Utilisateur?.Nom ?? "",
                Prenom = client.Utilisateur?.Prenom ?? "",
                Email = client.Utilisateur?.Email ?? "",
                Telephone = client.Telephone,
                Adresse = client.Adresse,
                NumeroPermis = client.NumeroPermis,
                DateInscription = client.DateInscription,
                Locations = new List<LocationDetailDto>()
            };
            
            return Ok(profileDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise à jour du profil client");
            return StatusCode(500, "Erreur lors de la mise à jour du profil");
        }
    }

    /// <summary>
    /// Récupérer tous les clients (Admin seulement)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "ADMINISTRATEUR")]
    public async Task<ActionResult<IEnumerable<ClientListDto>>> GetAllClients()
    {
        var clients = await _context.Clients
            .Include(c => c.Utilisateur)
            .OrderByDescending(c => c.DateInscription)
            .ToListAsync();

        return Ok(clients.Select(c => new ClientListDto
        {
            Id = c.Id,
            Nom = c.Utilisateur?.Nom ?? "",
            Prenom = c.Utilisateur?.Prenom ?? "",
            Email = c.Utilisateur?.Email ?? "",
            Telephone = c.Telephone,
            DateInscription = c.DateInscription,
            NombreLocations = c.Locations?.Count ?? 0
        }));
    }

    /// <summary>
    /// Récupérer les locations d'un client
    /// </summary>
    [HttpGet("{id}/locations")]
    [Authorize(Roles = "ADMINISTRATEUR,EMPLOYE")]
    public async Task<ActionResult<IEnumerable<LocationDetailDto>>> GetClientLocations(int id)
    {
        var client = await _context.Clients
            .Include(c => c.Locations)
                .ThenInclude(l => l.Vehicule)
                    .ThenInclude(v => v.TypeVehicule)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (client == null)
        {
            return NotFound("Client non trouvé");
        }

        var locations = client.Locations?
            .OrderByDescending(l => l.DateCreation)
            .Select(l => new LocationDetailDto
            {
                Id = l.Id,
                DateDebut = l.DateDebut,
                DateFin = l.DateFin,
                VehiculeId = l.VehiculeId,
                VehiculeNom = l.Vehicule?.Marque + " " + l.Vehicule?.Modele,
                Statut = l.Statut.ToString(),
                PrixTotal = l.MontantTotal,
                DateCreation = l.DateCreation
            })
            .ToList() ?? new List<LocationDetailDto>();

        return Ok(locations);
    }

    /// <summary>
    /// Compter les clients totaux (Admin seulement)
    /// </summary>
    [HttpGet("count/total")]
    [Authorize(Roles = "ADMINISTRATEUR")]
    public async Task<ActionResult<int>> GetTotalClientsCount()
    {
        var count = await _context.Clients.CountAsync();
        return Ok(count);
    }

    /// <summary>
    /// Récupérer les statistiques clients (Admin seulement)
    /// </summary>
    [HttpGet("stats/overview")]
    [Authorize(Roles = "ADMINISTRATEUR")]
    public async Task<ActionResult<ClientStatsDto>> GetClientsStatistics()
    {
        var totalClients = await _context.Clients.CountAsync();
        var clientsThisMonth = await _context.Clients
            .Where(c => c.DateInscription.Year == DateTime.Now.Year && 
                        c.DateInscription.Month == DateTime.Now.Month)
            .CountAsync();

        var totalLocations = await _context.Locations.CountAsync();
        var averageLocationsPerClient = totalClients > 0 ? (decimal)totalLocations / totalClients : 0;

        return Ok(new ClientStatsDto
        {
            TotalClients = totalClients,
            ClientsThisMonth = clientsThisMonth,
            TotalLocations = totalLocations,
            AverageLocationsPerClient = Math.Round(averageLocationsPerClient, 2)
        });
    }

    // DTO Mapping Helper
    private ClientProfileDto MapToDto(Client client)
    {
        return new ClientProfileDto
        {
            Id = client.Id,
            Nom = client.Utilisateur?.Nom ?? "",
            Prenom = client.Utilisateur?.Prenom ?? "",
            Email = client.Utilisateur?.Email ?? "",
            Telephone = client.Telephone,
            Adresse = client.Adresse,
            NumeroPermis = client.NumeroPermis,
            DateInscription = client.DateInscription,
            EstActif = client.Utilisateur?.EstActif ?? false,
            NombreLocations = client.Locations?.Count ?? 0,
            Locations = client.Locations?
                .OrderByDescending(l => l.DateCreation)
                .Select(l => new LocationDetailDto
                {
                    Id = l.Id,
                    DateDebut = l.DateDebut,
                    DateFin = l.DateFin,
                    VehiculeId = l.VehiculeId,
                    VehiculeNom = l.Vehicule?.Marque + " " + l.Vehicule?.Modele,
                    Statut = l.Statut.ToString(),
                    PrixTotal = l.MontantTotal,
                    DateCreation = l.DateCreation
                })
                .ToList() ?? new List<LocationDetailDto>()
        };
    }
}

// DTOs for Client operations
public class ClientProfileDto
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telephone { get; set; } = string.Empty;
    public string Adresse { get; set; } = string.Empty;
    public string NumeroPermis { get; set; } = string.Empty;
    public DateTime DateInscription { get; set; }
    public bool EstActif { get; set; }
    public int NombreLocations { get; set; }
    public List<LocationDetailDto>? Locations { get; set; }
}

public class UpdateClientProfileDto
{
    public string? Nom { get; set; }
    public string? Prenom { get; set; }
    public string? Telephone { get; set; }
    public string? Adresse { get; set; }
    public string? NumeroPermis { get; set; }
}

public class ClientListDto
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telephone { get; set; } = string.Empty;
    public DateTime DateInscription { get; set; }
    public int NombreLocations { get; set; }
}

public class LocationDetailDto
{
    public int Id { get; set; }
    public DateTime DateDebut { get; set; }
    public DateTime DateFin { get; set; }
    public int VehiculeId { get; set; }
    public string VehiculeNom { get; set; } = string.Empty;
    public string Statut { get; set; } = string.Empty;
    public decimal PrixTotal { get; set; }
    public DateTime DateCreation { get; set; }
}

public class ClientStatsDto
{
    public int TotalClients { get; set; }
    public int ClientsThisMonth { get; set; }
    public int TotalLocations { get; set; }
    public decimal AverageLocationsPerClient { get; set; }
}
