using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LocationVoituresAPI.Data;
using LocationVoituresAPI.DTOs;
using LocationVoituresAPI.Models;
using LocationVoituresAPI.Services;
using QRCoder;

namespace LocationVoituresAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LocationsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IPDFService _pdfService;
    private readonly IEmailService _emailService;
    private readonly ICloudinaryService _cloudinaryService;

    public LocationsController(
        ApplicationDbContext context,
        IPDFService pdfService,
        IEmailService emailService,
        ICloudinaryService cloudinaryService)
    {
        _context = context;
        _pdfService = pdfService;
        _emailService = emailService;
        _cloudinaryService = cloudinaryService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Location>>> GetLocations()
    {
        var locations = await _context.Locations
            .Include(l => l.Client)
                .ThenInclude(c => c.Utilisateur)
            .Include(l => l.Vehicule)
                .ThenInclude(v => v.TypeVehicule)
            .Include(l => l.Employe!)
                .ThenInclude(e => e.Utilisateur)
            .Include(l => l.Paiements)
            .ToListAsync();

        return Ok(locations);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Location>> GetLocation(int id)
    {
        var location = await _context.Locations
            .Include(l => l.Client)
                .ThenInclude(c => c.Utilisateur)
            .Include(l => l.Vehicule)
                .ThenInclude(v => v.TypeVehicule)
            .Include(l => l.Employe!)
                .ThenInclude(e => e.Utilisateur)
            .Include(l => l.Paiements)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (location == null)
            return NotFound();

        return Ok(location);
    }

    [HttpPost]
    public async Task<ActionResult<Location>> CreateLocation([FromBody] CreateLocationDto dto)
    {
        // Valider les dates
        if (dto.DateFin <= dto.DateDebut)
            return BadRequest("La date de fin doit être postérieure à la date de début");

        if (dto.DateDebut < DateTime.UtcNow.Date)
            return BadRequest("La date de début ne peut pas être dans le passé");

        // Vérifier que le client existe
        var client = await _context.Clients
            .Include(c => c.Utilisateur)
            .FirstOrDefaultAsync(c => c.Id == dto.ClientId);

        if (client == null)
            return NotFound("Client introuvable");

        // Vérifier que le véhicule existe et est disponible
        var vehicule = await _context.Vehicules
            .Include(v => v.Locations)
            .FirstOrDefaultAsync(v => v.Id == dto.VehiculeId);

        if (vehicule == null)
            return NotFound("Véhicule introuvable");

        // Vérifier la disponibilité AVANT de créer la location
        // Créer un objet temporaire pour tester la disponibilité
        var tempLocation = new Location
        {
            DateDebut = dto.DateDebut,
            DateFin = dto.DateFin,
            ClientId = dto.ClientId,
            VehiculeId = dto.VehiculeId,
            EmployeId = dto.EmployeId,
            Statut = StatutLocation.EN_ATTENTE
        };

        // Vérifier la disponibilité
        Console.WriteLine($"[CreateLocation] Vérification: EstDisponible={vehicule.EstDisponible}, DateDebut={dto.DateDebut}, DateFin={dto.DateFin}");
        Console.WriteLine($"[CreateLocation] Locations existantes: {vehicule.Locations.Count}");
        foreach(var loc in vehicule.Locations)
            Console.WriteLine($"  - Location {loc.Id}: {loc.DateDebut} à {loc.DateFin}, Statut={loc.Statut}");
        
        if (!vehicule.VerifierDisponibilite(dto.DateDebut, dto.DateFin))
        {
            Console.WriteLine($"[CreateLocation] REJET: Véhicule non disponible");
            return BadRequest(new { message = "Le véhicule n'est pas disponible pour cette période" });
        }
        Console.WriteLine($"[CreateLocation] OK: Véhicule disponible");

        // Calculer le montant
        tempLocation.CalculerMontant(vehicule);

        // Ajouter à la base de données SEULEMENT si disponible
        _context.Locations.Add(tempLocation);
        await _context.SaveChangesAsync();

        // Recharger depuis la BD pour obtenir tous les champs
        var location = await _context.Locations
            .Include(l => l.Client)
                .ThenInclude(c => c.Utilisateur)
            .Include(l => l.Vehicule)
                .ThenInclude(v => v.TypeVehicule)
            .FirstOrDefaultAsync(l => l.Id == tempLocation.Id);

        if (location == null)
            return StatusCode(500, "Erreur lors de la récupération de la location créée");

        // Générer QR Code après avoir l'ID
        // Le QR doit pointer vers une URL scannable, pas juste du texte
        var qrCodeData = $"{Request.Scheme}://{Request.Host}/api/locations/{location.Id}/scan";
        using var qrGenerator = new QRCodeGenerator();
        var qrData = qrGenerator.CreateQrCode(qrCodeData, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrData);
        var qrCodeBytes = qrCode.GetGraphic(20);

        // Générer QR Code en base64 directement (pas d'upload cloud pour performance)
        // Stocker SEULEMENT le base64 sans le préfixe data:image
        string qrCodeBase64 = Convert.ToBase64String(qrCodeBytes);
        
        // Sauvegarder le base64 dans la base de données
        location.QRCode = qrCodeBase64;
        await _context.SaveChangesAsync();
        Console.WriteLine($"[CreateLocation] Saved QRCode for location {location.Id}, length={qrCodeBase64.Length}");

        // Recharger pour être certain
        _context.Entry(location).Reload();
        Console.WriteLine($"[CreateLocation] After reload: QRCode length={location.QRCode?.Length ?? 0}");

        // Envoyer email de confirmation EN ARRIÈRE-PLAN (ne pas bloquer la réponse)
        _ = Task.Run(async () =>
        {
            try
            {
                await _emailService.EnvoyerEmailConfirmationAsync(
                    client.Utilisateur.Email,
                    client.Utilisateur.Nom,
                    client.Utilisateur.Prenom,
                    qrCodeData);
                Console.WriteLine($"[Email] Confirmation envoyée pour location {location.Id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Email] Erreur envoi confirmation: {ex.Message}");
            }
        });

        Console.WriteLine($"[CreateLocation] Returning Location {location.Id} with QRCode: {(string.IsNullOrEmpty(location.QRCode) ? "NULL" : "OK")}");
        Console.WriteLine($"[CreateLocation] QRCode content: {location.QRCode?.Substring(0, Math.Min(100, location.QRCode?.Length ?? 0)) ?? "NULL"}");
        return CreatedAtAction(nameof(GetLocation), new { id = location.Id }, location);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateLocation(int id, [FromBody] Location location)
    {
        if (id != location.Id)
            return BadRequest();

        _context.Entry(location).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!LocationExists(id))
                return NotFound();
            throw;
        }

        return NoContent();
    }

    [HttpPost("{id}/generer-facture")]
    public async Task<IActionResult> GenererFacture(int id)
    {
        var location = await _context.Locations
            .Include(l => l.Client)
                .ThenInclude(c => c.Utilisateur)
            .Include(l => l.Vehicule)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (location == null)
            return NotFound();

        var pdfBytes = await _pdfService.GenererFactureAsync(location);
        var fileName = $"facture_LOC_{id:D6}.pdf";

        // Upload vers Cloudinary
        using var stream = new MemoryStream(pdfBytes);
        var factureUrl = await _cloudinaryService.UploadPDFAsync(stream, fileName);
        location.FactureUrl = factureUrl;

        await _context.SaveChangesAsync();

        // Envoyer par email
        await _emailService.EnvoyerFactureParEmailAsync(
            location.Client.Utilisateur.Email,
            location.Client.Utilisateur.Nom,
            location.Client.Utilisateur.Prenom,
            factureUrl);

        return Ok(new { FactureUrl = factureUrl });
    }

    [HttpPost("{id}/confirmer")]
    public async Task<IActionResult> ConfirmerLocation(int id)
    {
        var location = await _context.Locations.FindAsync(id);
        if (location == null)
            return NotFound();

        location.Statut = StatutLocation.CONFIRMEE;
        await _context.SaveChangesAsync();

        return Ok(location);
    }

    [HttpPost("{id}/annuler")]
    public async Task<IActionResult> AnnulerLocation(int id)
    {
        var location = await _context.Locations.FindAsync(id);
        if (location == null)
            return NotFound();

        location.Statut = StatutLocation.ANNULEE;
        await _context.SaveChangesAsync();

        return Ok(location);
    }

    [HttpPost("{id}/regenerer-qrcode")]
    public async Task<IActionResult> RegenererQRCode(int id)
    {
        var location = await _context.Locations
            .Include(l => l.Client)
                .ThenInclude(c => c.Utilisateur)
            .Include(l => l.Vehicule)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (location == null)
            return NotFound("Location non trouvée");

        try
        {
            // Générer les données du QR code
            var qrCodeData = $"Location #{location.Id}\n" +
                           $"Véhicule: {location.Vehicule.Marque} {location.Vehicule.Modele}\n" +
                           $"Client: {location.Client.Utilisateur.Nom} {location.Client.Utilisateur.Prenom}\n" +
                           $"Du {location.DateDebut:dd/MM/yyyy} au {location.DateFin:dd/MM/yyyy}\n" +
                           $"Montant: {location.MontantTotal:C}";

            // Générer le QR code
            using var qrGenerator = new QRCodeGenerator();
            var qrData = qrGenerator.CreateQrCode(qrCodeData, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrData);
            var qrCodeBytes = qrCode.GetGraphic(20);

            // Upload vers Cloudinary
            string qrCodeUrl;
            try
            {
                using (var qrStream = new MemoryStream(qrCodeBytes))
                {
                    var qrFileName = $"qr_code_LOC_{location.Id:D6}.png";
                    qrCodeUrl = await _cloudinaryService.UploadImageAsync(qrStream, qrFileName);
                }
            }
            catch (Exception cloudinaryEx)
            {
                return StatusCode(500, new { 
                    Message = "Erreur lors de l'upload vers Cloudinary",
                    Error = cloudinaryEx.Message,
                    InnerError = cloudinaryEx.InnerException?.Message
                });
            }

            // Mettre à jour la location
            location.QRCode = qrCodeUrl;
            await _context.SaveChangesAsync();

            return Ok(new { 
                Message = "QR Code régénéré avec succès",
                QRCodeUrl = qrCodeUrl 
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { 
                Message = "Erreur lors de la génération du QR code",
                Error = ex.Message,
                InnerError = ex.InnerException?.Message
            });
        }
    }

    private bool LocationExists(int id)
    {
        return _context.Locations.Any(e => e.Id == id);
    }

    /// <summary>
    /// Endpoint pour scanner un QR Code de réservation
    /// Utilisé par les agents de location pour accéder rapidement au dossier
    /// </summary>
    [HttpGet("{id}/scan")]
    [AllowAnonymous]
    public async Task<IActionResult> ScanQRCode(int id)
    {
        var location = await _context.Locations
            .Include(l => l.Client)
            .Include(l => l.Vehicule)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (location == null)
            return NotFound(new { message = "Réservation non trouvée" });

        // Retourner les infos essentielles pour l'agent
        return Ok(new
        {
            location.Id,
            ReservationNumber = $"LOC-{location.Id:D6}",
            location.Statut,
            location.DateDebut,
            location.DateFin,
            Vehicle = new
            {
                location.Vehicule?.Marque,
                location.Vehicule?.Modele,
                location.Vehicule?.Immatriculation,
                location.Vehicule?.Couleur
            },
            Client = new
            {
                location.Client?.Utilisateur?.Nom,
                location.Client?.Utilisateur?.Prenom,
                location.Client?.Utilisateur?.Email
            },
            location.DureeJours,
            location.MontantTotal,
            location.DateCreation
        });
    }
}

