using Microsoft.EntityFrameworkCore;
using LocationVoituresAPI.Data;
using LocationVoituresAPI.Models;

namespace LocationVoituresAPI.Services;

public class StatistiquesService : IStatistiquesService
{
    private readonly ApplicationDbContext _context;

    public StatistiquesService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Dictionary<string, object>> GetStatistiquesGlobalesAsync()
    {
        var totalVehicules = await _context.Vehicules.CountAsync();
        var vehiculesDisponibles = await _context.Vehicules.CountAsync(v => v.EstDisponible);
        var totalLocations = await _context.Locations.CountAsync();
        var locationsEnCours = await _context.Locations.CountAsync(l => l.Statut == StatutLocation.EN_COURS);
        var revenusTotal = await _context.Paiements
            .Where(p => p.Statut == StatutPaiement.VALIDE)
            .SumAsync(p => p.Montant);
        var totalClients = await _context.Clients.CountAsync();

        return new Dictionary<string, object>
        {
            { "TotalVehicules", totalVehicules },
            { "VehiculesDisponibles", vehiculesDisponibles },
            { "TotalLocations", totalLocations },
            { "LocationsEnCours", locationsEnCours },
            { "RevenusTotal", revenusTotal },
            { "TotalClients", totalClients }
        };
    }

    public async Task<Dictionary<string, object>> GetStatistiquesPeriodeAsync(DateTime dateDebut, DateTime dateFin)
    {
        var locations = await _context.Locations
            .Where(l => l.DateCreation >= dateDebut && l.DateCreation <= dateFin)
            .ToListAsync();

        var revenus = await _context.Paiements
            .Where(p => p.DatePaiement >= dateDebut && p.DatePaiement <= dateFin && p.Statut == StatutPaiement.VALIDE)
            .SumAsync(p => p.Montant);

        return new Dictionary<string, object>
        {
            { "NombreLocations", locations.Count },
            { "Revenus", revenus },
            { "DateDebut", dateDebut },
            { "DateFin", dateFin }
        };
    }

    public async Task<Dictionary<string, object>> GenererGraphiquesAsync()
    {
        var locationsParMois = await _context.Locations
            .GroupBy(l => new { l.DateCreation.Year, l.DateCreation.Month })
            .Select(g => new { Mois = $"{g.Key.Year}-{g.Key.Month:D2}", Count = g.Count() })
            .ToListAsync();

        var revenusParMois = await _context.Paiements
            .Where(p => p.Statut == StatutPaiement.VALIDE)
            .GroupBy(p => new { p.DatePaiement.Year, p.DatePaiement.Month })
            .Select(g => new { Mois = $"{g.Key.Year}-{g.Key.Month:D2}", Revenus = g.Sum(p => p.Montant) })
            .ToListAsync();

        return new Dictionary<string, object>
        {
            { "LocationsParMois", locationsParMois },
            { "RevenusParMois", revenusParMois }
        };
    }
}

