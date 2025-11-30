using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LocationVoituresAPI.Data;
using LocationVoituresAPI.Models;
using LocationVoituresAPI.Services;

namespace LocationVoituresAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EntretiensController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;

    public EntretiensController(ApplicationDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Entretien>>> GetEntretiens()
    {
        var entretiens = await _context.Entretiens
            .Include(e => e.Vehicule)
            .Include(e => e.Employe)
                .ThenInclude(emp => emp.Utilisateur)
            .ToListAsync();

        return Ok(entretiens);
    }

    [HttpGet("urgents")]
    public async Task<ActionResult<IEnumerable<Entretien>>> GetEntretiensUrgents()
    {
        var entretiens = await _context.Entretiens
            .Include(e => e.Vehicule)
            .Include(e => e.Employe)
                .ThenInclude(emp => emp.Utilisateur)
            .Where(e => e.EstUrgent && e.Statut != StatutEntretien.TERMINE)
            .ToListAsync();

        return Ok(entretiens);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Entretien>> GetEntretien(int id)
    {
        var entretien = await _context.Entretiens
            .Include(e => e.Vehicule)
            .Include(e => e.Employe)
                .ThenInclude(emp => emp.Utilisateur)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (entretien == null)
            return NotFound();

        return Ok(entretien);
    }

    [HttpPost]
    [Authorize(Roles = "ADMINISTRATEUR,EMPLOYE")]
    public async Task<ActionResult<Entretien>> CreateEntretien([FromBody] Entretien entretien)
    {
        _context.Entretiens.Add(entretien);
        await _context.SaveChangesAsync();

        // Vérifier si urgent et envoyer alerte
        if (entretien.EstUrgent)
        {
            var vehicule = await _context.Vehicules.FindAsync(entretien.VehiculeId);
            if (vehicule != null)
            {
                // Envoyer alerte aux administrateurs
                var admins = await _context.Utilisateurs
                    .Where(u => u.TypeUtilisateur == TypeUtilisateur.ADMINISTRATEUR)
                    .ToListAsync();

                foreach (var admin in admins)
                {
                    await _emailService.EnvoyerAlerteEntretienAsync(
                        admin.Email,
                        $"{vehicule.Marque} {vehicule.Modele} - {vehicule.Immatriculation}",
                        entretien.DateEntretien);
                }
            }
        }

        return CreatedAtAction(nameof(GetEntretien), new { id = entretien.Id }, entretien);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "ADMINISTRATEUR,EMPLOYE")]
    public async Task<IActionResult> UpdateEntretien(int id, [FromBody] Entretien entretien)
    {
        if (id != entretien.Id)
            return BadRequest();

        _context.Entry(entretien).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!EntretienExists(id))
                return NotFound();
            throw;
        }

        return NoContent();
    }

    [HttpPost("{id}/terminer")]
    [Authorize(Roles = "ADMINISTRATEUR,EMPLOYE")]
    public async Task<IActionResult> TerminerEntretien(int id)
    {
        var entretien = await _context.Entretiens.FindAsync(id);
        if (entretien == null)
            return NotFound();

        entretien.MarquerTermine();
        await _context.SaveChangesAsync();

        return Ok(entretien);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "ADMINISTRATEUR")]
    public async Task<IActionResult> DeleteEntretien(int id)
    {
        var entretien = await _context.Entretiens.FindAsync(id);
        if (entretien == null)
            return NotFound();

        _context.Entretiens.Remove(entretien);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool EntretienExists(int id)
    {
        return _context.Entretiens.Any(e => e.Id == id);
    }
}

