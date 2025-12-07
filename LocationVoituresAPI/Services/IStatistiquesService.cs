namespace LocationVoituresAPI.Services;

public interface IStatistiquesService
{
    Task<Dictionary<string, object>> GetStatistiquesGlobalesAsync();
    Task<Dictionary<string, object>> GetStatistiquesPeriodeAsync(DateTime dateDebut, DateTime dateFin);
    Task<Dictionary<string, object>> GenererGraphiquesAsync();
}

