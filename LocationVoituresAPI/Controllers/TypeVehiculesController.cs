using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LocationVoituresAPI.Data;
using LocationVoituresAPI.Models;

namespace LocationVoituresAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TypeVehiculesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public TypeVehiculesController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Récupérer tous les types de véhicules
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<TypeVehicule>>> GetTypeVehicules()
    {
        var types = await _context.TypeVehicules
            .Include(t => t.Vehicules)
            .ToListAsync();

        return Ok(types);
    }

    /// <summary>
    /// Récupérer un type de véhicule par ID
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<TypeVehicule>> GetTypeVehicule(int id)
    {
        var typeVehicule = await _context.TypeVehicules
            .Include(t => t.Vehicules)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (typeVehicule == null)
            return NotFound(new { Message = "Type de véhicule introuvable" });

        return Ok(typeVehicule);
    }

    /// <summary>
    /// Créer un nouveau type de véhicule (Admin uniquement)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "ADMINISTRATEUR")]
    public async Task<ActionResult<TypeVehicule>> CreateTypeVehicule([FromBody] TypeVehicule typeVehicule)
    {
        // Vérifier si le type existe déjà
        var existingType = await _context.TypeVehicules
            .FirstOrDefaultAsync(t => t.Nom.ToLower() == typeVehicule.Nom.ToLower());

        if (existingType != null)
            return BadRequest(new { Message = "Un type de véhicule avec ce nom existe déjà" });

        _context.TypeVehicules.Add(typeVehicule);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTypeVehicule), new { id = typeVehicule.Id }, typeVehicule);
    }

    /// <summary>
    /// Modifier un type de véhicule (Admin uniquement)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "ADMINISTRATEUR")]
    public async Task<IActionResult> UpdateTypeVehicule(int id, [FromBody] TypeVehicule typeVehicule)
    {
        if (id != typeVehicule.Id)
            return BadRequest(new { Message = "L'ID ne correspond pas" });

        var existingType = await _context.TypeVehicules.FindAsync(id);
        if (existingType == null)
            return NotFound(new { Message = "Type de véhicule introuvable" });

        // Vérifier si le nouveau nom n'est pas déjà utilisé par un autre type
        var duplicateName = await _context.TypeVehicules
            .AnyAsync(t => t.Id != id && t.Nom.ToLower() == typeVehicule.Nom.ToLower());

        if (duplicateName)
            return BadRequest(new { Message = "Un type de véhicule avec ce nom existe déjà" });

        existingType.Nom = typeVehicule.Nom;
        existingType.Description = typeVehicule.Description;
        existingType.Categorie = typeVehicule.Categorie;
        existingType.PrixBaseJournalier = typeVehicule.PrixBaseJournalier;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!TypeVehiculeExists(id))
                return NotFound();
            throw;
        }

        return Ok(existingType);
    }

    /// <summary>
    /// Supprimer un type de véhicule (Admin uniquement)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "ADMINISTRATEUR")]
    public async Task<IActionResult> DeleteTypeVehicule(int id)
    {
        var typeVehicule = await _context.TypeVehicules
            .Include(t => t.Vehicules)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (typeVehicule == null)
            return NotFound(new { Message = "Type de véhicule introuvable" });

        // Vérifier s'il y a des véhicules associés
        if (typeVehicule.Vehicules.Any())
        {
            return BadRequest(new 
            { 
                Message = "Impossible de supprimer ce type de véhicule car il est utilisé par des véhicules existants",
                VehiculesCount = typeVehicule.Vehicules.Count
            });
        }

        _context.TypeVehicules.Remove(typeVehicule);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Type de véhicule supprimé avec succès" });
    }

    /// <summary>
    /// Obtenir les statistiques d'un type de véhicule
    /// </summary>
    [HttpGet("{id}/statistiques")]
    [Authorize(Roles = "ADMINISTRATEUR,EMPLOYE")]
    public async Task<ActionResult> GetTypeVehiculeStatistiques(int id)
    {
        var typeVehicule = await _context.TypeVehicules
            .Include(t => t.Vehicules)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (typeVehicule == null)
            return NotFound(new { Message = "Type de véhicule introuvable" });

        var stats = new
        {
            TypeVehicule = typeVehicule.Nom,
            NombreVehicules = typeVehicule.Vehicules.Count,
            VehiculesDisponibles = typeVehicule.Vehicules.Count(v => v.EstDisponible),
            PrixMoyenJournalier = typeVehicule.Vehicules.Any() 
                ? typeVehicule.Vehicules.Average(v => v.PrixJournalier) 
                : 0,
            PrixBaseJournalier = typeVehicule.PrixBaseJournalier
        };

        return Ok(stats);
    }

    private bool TypeVehiculeExists(int id)
    {
        return _context.TypeVehicules.Any(t => t.Id == id);
    }
}
