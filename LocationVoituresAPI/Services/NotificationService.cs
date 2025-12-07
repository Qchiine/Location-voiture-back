namespace LocationVoituresAPI.Services;

public class NotificationService : INotificationService
{
    private readonly IEmailService _emailService;

    public NotificationService(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task EnvoyerNotificationAsync(string destinataire, string message)
    {
        await _emailService.EnvoyerNotificationAsync(destinataire, "Notification", message);
    }

    public async Task EnvoyerSMSAsync(string telephone, string message)
    {
        // Implémentation SMS - à compléter avec un service SMS (Twilio, etc.)
        await Task.CompletedTask;
    }

    public async Task EnvoyerNotificationPushAsync(string deviceToken, string message)
    {
        // Implémentation Push Notification - à compléter avec Firebase, etc.
        await Task.CompletedTask;
    }
}

