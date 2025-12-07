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
public class VehiculesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ICloudinaryService _cloudinaryService;

    public VehiculesController(ApplicationDbContext context, ICloudinaryService cloudinaryService)
    {
        _context = context;
        _cloudinaryService = cloudinaryService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Vehicule>>> GetVehicules()
    {
        var vehicules = await _context.Vehicules
            .Include(v => v.TypeVehicule)
            .ToListAsync();

        return Ok(vehicules);
    }

    [HttpGet("disponibles")]
    public async Task<ActionResult<IEnumerable<Vehicule>>> GetVehiculesDisponibles(
        [FromQuery] DateTime? dateDebut = null,
        [FromQuery] DateTime? dateFin = null)
    {
        var query = _context.Vehicules
            .Include(v => v.TypeVehicule)
            .Where(v => v.EstDisponible);

        if (dateDebut.HasValue && dateFin.HasValue)
        {
            query = query.Where(v => v.VerifierDisponibilite(dateDebut.Value, dateFin.Value));
        }

        var vehicules = await query.ToListAsync();
        return Ok(vehicules);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Vehicule>> GetVehicule(int id)
    {
        var vehicule = await _context.Vehicules
            .Include(v => v.TypeVehicule)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (vehicule == null)
            return NotFound();

        return Ok(vehicule);
    }

    [HttpPost]
    [Authorize(Roles = "ADMINISTRATEUR,EMPLOYE")]
    public async Task<ActionResult<Vehicule>> CreateVehicule([FromBody] Vehicule vehicule)
    {
        _context.Vehicules.Add(vehicule);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetVehicule), new { id = vehicule.Id }, vehicule);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "ADMINISTRATEUR,EMPLOYE")]
    public async Task<IActionResult> UpdateVehicule(int id, [FromBody] Vehicule vehicule)
    {
        if (id != vehicule.Id)
            return BadRequest();

        _context.Entry(vehicule).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!VehiculeExists(id))
                return NotFound();
            throw;
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "ADMINISTRATEUR")]
    public async Task<IActionResult> DeleteVehicule(int id)
    {
        var vehicule = await _context.Vehicules.FindAsync(id);
        if (vehicule == null)
            return NotFound();

        _context.Vehicules.Remove(vehicule);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{id}/upload-images")]
    [Authorize(Roles = "ADMINISTRATEUR,EMPLOYE")]
    public async Task<IActionResult> UploadImages(int id, [FromForm] List<IFormFile> files)
    {
        var vehicule = await _context.Vehicules.FindAsync(id);
        if (vehicule == null)
            return NotFound();

        var imageStreams = new List<(Stream stream, string fileName)>();
        var urls = new List<string>();

        foreach (var file in files)
        {
            if (file.Length > 0)
            {
                var stream = file.OpenReadStream();
                var url = await _cloudinaryService.UploadImageAsync(stream, file.FileName);
                urls.Add(url);

                if (string.IsNullOrEmpty(vehicule.ImagePrincipaleUrl))
                {
                    vehicule.ImagePrincipaleUrl = url;
                }
            }
        }

        vehicule.ImagesUrls = System.Text.Json.JsonSerializer.Serialize(urls);
        await _context.SaveChangesAsync();

        return Ok(new { ImageUrls = urls });
    }

    private bool VehiculeExists(int id)
    {
        return _context.Vehicules.Any(e => e.Id == id);
    }
}

