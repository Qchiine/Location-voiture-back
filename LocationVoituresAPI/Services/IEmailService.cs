namespace LocationVoituresAPI.Services;

public interface IEmailService
{
    Task EnvoyerEmailConfirmationAsync(string to, string nom, string prenom, string numeroReservation);
    Task EnvoyerFactureParEmailAsync(string to, string nom, string prenom, string factureUrl);
    Task EnvoyerAlerteEntretienAsync(string to, string vehiculeInfo, DateTime dateEntretien);
    Task EnvoyerNotificationAsync(string to, string sujet, string message);
    Task EnvoyerRappelAsync(string to, string message);
}

