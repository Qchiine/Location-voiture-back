namespace LocationVoituresAPI.Services;

public interface INotificationService
{
    Task EnvoyerNotificationAsync(string destinataire, string message);
    Task EnvoyerSMSAsync(string telephone, string message);
    Task EnvoyerNotificationPushAsync(string deviceToken, string message);
}

