using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LocationVoituresAPI.Data;
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
            .Include(l => l.Employe)
                .ThenInclude(e => e.Utilisateur)
            .Include(l => l.Paiement)
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
            .Include(l => l.Employe)
                .ThenInclude(e => e.Utilisateur)
            .Include(l => l.Paiement)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (location == null)
            return NotFound();

        return Ok(location);
    }

    [HttpPost]
    public async Task<ActionResult<Location>> CreateLocation([FromBody] Location location)
    {
        var vehicule = await _context.Vehicules
            .Include(v => v.Locations)
            .FirstOrDefaultAsync(v => v.Id == location.VehiculeId);

        if (vehicule == null)
            return NotFound("Véhicule introuvable");

        if (!location.VerifierDisponibilite(vehicule))
            return BadRequest("Le véhicule n'est pas disponible pour cette période");

        location.CalculerMontant(vehicule);
        location.Statut = StatutLocation.EN_ATTENTE;

        // Générer QR Code
        var qrCodeData = $"LOC-{location.Id:D6}";
        using var qrGenerator = new QRCodeGenerator();
        var qrData = qrGenerator.CreateQrCode(qrCodeData, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrData);
        var qrCodeBytes = qrCode.GetGraphic(20);
        location.QRCode = Convert.ToBase64String(qrCodeBytes);

        _context.Locations.Add(location);
        await _context.SaveChangesAsync();

        // Envoyer email de confirmation
        var client = await _context.Clients
            .Include(c => c.Utilisateur)
            .FirstOrDefaultAsync(c => c.Id == location.ClientId);

        if (client != null)
        {
            await _emailService.EnvoyerEmailConfirmationAsync(
                client.Utilisateur.Email,
                client.Utilisateur.Nom,
                client.Utilisateur.Prenom,
                qrCodeData);
        }

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

    private bool LocationExists(int id)
    {
        return _context.Locations.Any(e => e.Id == id);
    }
}

