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
public class PaiementsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IPDFService _pdfService;

    public PaiementsController(ApplicationDbContext context, IPDFService pdfService)
    {
        _context = context;
        _pdfService = pdfService;
    }

    [HttpPost]
    public async Task<ActionResult<Paiement>> CreatePaiement([FromBody] Paiement paiement)
    {
        var location = await _context.Locations.FindAsync(paiement.LocationId);
        if (location == null)
            return NotFound("Location introuvable");

        paiement.Reference = $"PAY-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid():N}";
        paiement.DatePaiement = DateTime.Now;

        _context.Paiements.Add(paiement);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPaiement), new { id = paiement.Id }, paiement);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Paiement>> GetPaiement(int id)
    {
        var paiement = await _context.Paiements
            .Include(p => p.Location)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (paiement == null)
            return NotFound();

        return Ok(paiement);
    }

    [HttpPut("{id}/statut")]
    [Authorize(Roles = "ADMINISTRATEUR,EMPLOYE")]
    public async Task<IActionResult> UpdateStatutPaiement(int id, [FromBody] StatutPaiement statut)
    {
        var paiement = await _context.Paiements.FindAsync(id);
        if (paiement == null)
            return NotFound();

        paiement.Statut = statut;
        await _context.SaveChangesAsync();

        return Ok(paiement);
    }

    [HttpGet("{id}/recu")]
    public async Task<IActionResult> GenererRecu(int id)
    {
        var paiement = await _context.Paiements
            .Include(p => p.Location)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (paiement == null)
            return NotFound();

        var pdfBytes = await _pdfService.GenererRecuAsync(paiement);
        return File(pdfBytes, "application/pdf", $"recu_{id}.pdf");
    }
}

