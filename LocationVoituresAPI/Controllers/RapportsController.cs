using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LocationVoituresAPI.Data;
using LocationVoituresAPI.Models;
using LocationVoituresAPI.Services;
using System.Text.Json;

namespace LocationVoituresAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "ADMINISTRATEUR")]
public class RapportsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IStatistiquesService _statistiquesService;
    private readonly IPDFService _pdfService;
    private readonly IExportService _exportService;

    public RapportsController(
        ApplicationDbContext context,
        IStatistiquesService statistiquesService,
        IPDFService pdfService,
        IExportService exportService)
    {
        _context = context;
        _statistiquesService = statistiquesService;
        _pdfService = pdfService;
        _exportService = exportService;
    }

    /// <summary>
    /// Récupérer tous les rapports
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Rapport>>> GetRapports()
    {
        var rapports = await _context.Rapports
            .Include(r => r.Administrateur)
            .OrderByDescending(r => r.DateGeneration)
            .ToListAsync();

        return Ok(rapports);
    }

    /// <summary>
    /// Récupérer un rapport par ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Rapport>> GetRapport(int id)
    {
        var rapport = await _context.Rapports
            .Include(r => r.Administrateur)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (rapport == null)
            return NotFound(new { Message = "Rapport introuvable" });

        return Ok(rapport);
    }

    /// <summary>
    /// Générer un rapport de statistiques globales
    /// </summary>
    [HttpPost("generer-statistiques-globales")]
    public async Task<ActionResult<Rapport>> GenererRapportStatistiquesGlobales([FromQuery] FormatRapport format = FormatRapport.PDF)
    {
        var stats = await _statistiquesService.GetStatistiquesGlobalesAsync();
        var donnees = JsonSerializer.Serialize(stats);

        var rapport = new Rapport
        {
            TypeRapport = TypeRapport.STATISTIQUES_GLOBALES,
            DateGeneration = DateTime.Now,
            Donnees = donnees,
            Format = format,
            AdministrateurId = GetCurrentUserId(),
            NomFichier = $"Rapport_Stats_Globales_{DateTime.Now:yyyyMMdd_HHmmss}.{format.ToString().ToLower()}"
        };

        _context.Rapports.Add(rapport);
        await _context.SaveChangesAsync();

        // Générer le fichier selon le format
        byte[] fichier = format switch
        {
            FormatRapport.PDF => await GenererPDFStatistiques(stats),
            _ => Array.Empty<byte>()
        };

        return Ok(new
        {
            Rapport = rapport,
            Fichier = Convert.ToBase64String(fichier),
            ContentType = GetContentType(format)
        });
    }

    /// <summary>
    /// Générer un rapport de locations pour une période
    /// </summary>
    [HttpPost("generer-locations-periode")]
    public async Task<ActionResult<Rapport>> GenererRapportLocationsPeriode(
        [FromQuery] DateTime dateDebut,
        [FromQuery] DateTime dateFin,
        [FromQuery] FormatRapport format = FormatRapport.PDF)
    {
        var locations = await _context.Locations
            .Include(l => l.Client).ThenInclude(c => c.Utilisateur)
            .Include(l => l.Vehicule).ThenInclude(v => v.TypeVehicule)
            .Include(l => l.Paiements)
            .Where(l => l.DateCreation >= dateDebut && l.DateCreation <= dateFin)
            .ToListAsync();

        var donnees = JsonSerializer.Serialize(new
        {
            DateDebut = dateDebut,
            DateFin = dateFin,
            NombreLocations = locations.Count,
            MontantTotal = locations.Sum(l => l.MontantTotal),
            Locations = locations.Select(l => new
            {
                l.Id,
                ClientNom = $"{l.Client.Utilisateur.Prenom} {l.Client.Utilisateur.Nom}",
                Vehicule = $"{l.Vehicule.Marque} {l.Vehicule.Modele}",
                l.DateDebut,
                l.DateFin,
                l.MontantTotal,
                Statut = l.Statut.ToString(),
                StatutPaiement = l.Paiements.Any() 
                    ? l.Paiements.First().Statut.ToString() 
                    : "Non payé"
            })
        });

        var rapport = new Rapport
        {
            TypeRapport = TypeRapport.LOCATIONS_PERIODE,
            DateGeneration = DateTime.Now,
            Donnees = donnees,
            Format = format,
            AdministrateurId = GetCurrentUserId(),
            NomFichier = $"Rapport_Locations_{dateDebut:yyyyMMdd}_{dateFin:yyyyMMdd}.{format.ToString().ToLower()}"
        };

        _context.Rapports.Add(rapport);
        await _context.SaveChangesAsync();

        return Ok(rapport);
    }

    /// <summary>
    /// Générer un rapport de revenus pour une période
    /// </summary>
    [HttpPost("generer-revenus-periode")]
    public async Task<ActionResult<Rapport>> GenererRapportRevenusPeriode(
        [FromQuery] DateTime dateDebut,
        [FromQuery] DateTime dateFin,
        [FromQuery] FormatRapport format = FormatRapport.PDF)
    {
        var paiements = await _context.Paiements
            .Include(p => p.Location).ThenInclude(l => l.Client).ThenInclude(c => c.Utilisateur)
            .Include(p => p.Location).ThenInclude(l => l.Vehicule)
            .Where(p => p.DatePaiement >= dateDebut && p.DatePaiement <= dateFin)
            .ToListAsync();

        var donnees = JsonSerializer.Serialize(new
        {
            DateDebut = dateDebut,
            DateFin = dateFin,
            NombrePaiements = paiements.Count,
            RevenuTotal = paiements.Where(p => p.Statut == StatutPaiement.VALIDE).Sum(p => p.Montant),
            PaiementsEnAttente = paiements.Count(p => p.Statut == StatutPaiement.EN_ATTENTE),
            MontantEnAttente = paiements.Where(p => p.Statut == StatutPaiement.EN_ATTENTE).Sum(p => p.Montant),
            Paiements = paiements.Select(p => new
            {
                p.Id,
                p.Reference,
                ClientNom = $"{p.Location.Client.Utilisateur.Prenom} {p.Location.Client.Utilisateur.Nom}",
                Vehicule = $"{p.Location.Vehicule.Marque} {p.Location.Vehicule.Modele}",
                p.Montant,
                p.DatePaiement,
                p.ModePaiement,
                p.Statut
            })
        });

        var rapport = new Rapport
        {
            TypeRapport = TypeRapport.REVENUS_PERIODE,
            DateGeneration = DateTime.Now,
            Donnees = donnees,
            Format = format,
            AdministrateurId = GetCurrentUserId(),
            NomFichier = $"Rapport_Revenus_{dateDebut:yyyyMMdd}_{dateFin:yyyyMMdd}.{format.ToString().ToLower()}"
        };

        _context.Rapports.Add(rapport);
        await _context.SaveChangesAsync();

        return Ok(rapport);
    }

    /// <summary>
    /// Générer un rapport d'entretiens
    /// </summary>
    [HttpPost("generer-entretiens")]
    public async Task<ActionResult<Rapport>> GenererRapportEntretiens(
        [FromQuery] DateTime? dateDebut = null,
        [FromQuery] DateTime? dateFin = null,
        [FromQuery] FormatRapport format = FormatRapport.PDF)
    {
        var query = _context.Entretiens
            .Include(e => e.Vehicule).ThenInclude(v => v.TypeVehicule)
            .Include(e => e.Employe).ThenInclude(emp => emp.Utilisateur)
            .AsQueryable();

        if (dateDebut.HasValue)
            query = query.Where(e => e.DateEntretien >= dateDebut.Value);

        if (dateFin.HasValue)
            query = query.Where(e => e.DateEntretien <= dateFin.Value);

        var entretiens = await query.ToListAsync();

        var donnees = JsonSerializer.Serialize(new
        {
            DateDebut = dateDebut,
            DateFin = dateFin,
            NombreEntretiens = entretiens.Count,
            CoutTotal = entretiens.Sum(e => e.Cout),
            EntretiensUrgents = entretiens.Count(e => e.EstUrgent),
            EntretiensTermines = entretiens.Count(e => e.Statut == StatutEntretien.TERMINE),
            Entretiens = entretiens.Select(e => new
            {
                e.Id,
                Vehicule = $"{e.Vehicule.Marque} {e.Vehicule.Modele} - {e.Vehicule.Immatriculation}",
                e.DateEntretien,
                e.TypeEntretien,
                e.Description,
                e.Cout,
                e.EstUrgent,
                e.Statut,
                Employe = e.Employe != null ? $"{e.Employe.Utilisateur.Prenom} {e.Employe.Utilisateur.Nom}" : "Non assigné"
            })
        });

        var rapport = new Rapport
        {
            TypeRapport = TypeRapport.ENTRETIENS,
            DateGeneration = DateTime.Now,
            Donnees = donnees,
            Format = format,
            AdministrateurId = GetCurrentUserId(),
            NomFichier = $"Rapport_Entretiens_{DateTime.Now:yyyyMMdd_HHmmss}.{format.ToString().ToLower()}"
        };

        _context.Rapports.Add(rapport);
        await _context.SaveChangesAsync();

        return Ok(rapport);
    }

    /// <summary>
    /// Générer un rapport de véhicules
    /// </summary>
    [HttpPost("generer-vehicules")]
    public async Task<ActionResult<Rapport>> GenererRapportVehicules([FromQuery] FormatRapport format = FormatRapport.PDF)
    {
        var vehicules = await _context.Vehicules
            .Include(v => v.TypeVehicule)
            .Include(v => v.Locations)
            .Include(v => v.Entretiens)
            .ToListAsync();

        var donnees = JsonSerializer.Serialize(new
        {
            NombreVehicules = vehicules.Count,
            VehiculesDisponibles = vehicules.Count(v => v.EstDisponible),
            VehiculesEnLocation = vehicules.Count(v => !v.EstDisponible),
            PrixMoyenJournalier = vehicules.Average(v => v.PrixJournalier),
            Vehicules = vehicules.Select(v => new
            {
                v.Id,
                v.Marque,
                v.Modele,
                v.Immatriculation,
                v.Annee,
                TypeVehicule = v.TypeVehicule.Nom,
                v.PrixJournalier,
                v.EstDisponible,
                v.Kilometrage,
                NombreLocations = v.Locations.Count,
                NombreEntretiens = v.Entretiens.Count
            })
        });

        var rapport = new Rapport
        {
            TypeRapport = TypeRapport.VEHICULES,
            DateGeneration = DateTime.Now,
            Donnees = donnees,
            Format = format,
            AdministrateurId = GetCurrentUserId(),
            NomFichier = $"Rapport_Vehicules_{DateTime.Now:yyyyMMdd_HHmmss}.{format.ToString().ToLower()}"
        };

        _context.Rapports.Add(rapport);
        await _context.SaveChangesAsync();

        return Ok(rapport);
    }

    /// <summary>
    /// Exporter un rapport existant en PDF
    /// </summary>
    [HttpGet("{id}/exporter-pdf")]
    public async Task<IActionResult> ExporterPDF(int id)
    {
        var rapport = await _context.Rapports.FindAsync(id);
        if (rapport == null)
            return NotFound(new { Message = "Rapport introuvable" });

        var donnees = JsonSerializer.Deserialize<Dictionary<string, object>>(rapport.Donnees);
        var pdfBytes = await GenererPDFStatistiques(donnees!);

        return File(pdfBytes, "application/pdf", rapport.NomFichier ?? $"rapport_{id}.pdf");
    }

    /// <summary>
    /// Exporter un rapport existant en Excel
    /// </summary>
    [HttpGet("{id}/exporter-excel")]
    public async Task<IActionResult> ExporterExcel(int id)
    {
        var rapport = await _context.Rapports.FindAsync(id);
        if (rapport == null)
            return NotFound(new { Message = "Rapport introuvable" });

        // Pour l'instant, retourner NotImplemented
        // À implémenter selon vos besoins spécifiques
        return await Task.FromResult(StatusCode(501, new { Message = "Export Excel à implémenter" }));
    }

    /// <summary>
    /// Supprimer un rapport
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRapport(int id)
    {
        var rapport = await _context.Rapports.FindAsync(id);
        if (rapport == null)
            return NotFound(new { Message = "Rapport introuvable" });

        _context.Rapports.Remove(rapport);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Rapport supprimé avec succès" });
    }

    // Méthodes privées
    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("UserId")?.Value;
        return int.TryParse(userIdClaim, out int userId) ? userId : 0;
    }

    private async Task<byte[]> GenererPDFStatistiques(Dictionary<string, object> donnees)
    {
        // Utiliser le service PDF pour générer le rapport
        // Pour l'instant, retourner un tableau vide
        // À implémenter selon vos besoins spécifiques
        return await Task.FromResult(Array.Empty<byte>());
    }

    private string GetContentType(FormatRapport format)
    {
        return format switch
        {
            FormatRapport.PDF => "application/pdf",
            FormatRapport.EXCEL => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            FormatRapport.CSV => "text/csv",
            _ => "application/octet-stream"
        };
    }
}
