using MailKit.Net.Smtp;
using MimeKit;

namespace LocationVoituresAPI.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private async Task EnvoyerEmailAsync(string to, string sujet, string corpsHtml)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Location Voitures", _configuration["Email:SenderEmail"]));
        message.To.Add(new MailboxAddress("", to));
        message.Subject = sujet;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = corpsHtml
        };
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(_configuration["Email:SmtpServer"], 
            int.Parse(_configuration["Email:SmtpPort"] ?? "587"), false);
        await client.AuthenticateAsync(_configuration["Email:SenderEmail"], 
            _configuration["Email:SenderPassword"]);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

    public async Task EnvoyerEmailConfirmationAsync(string to, string nom, string prenom, string numeroReservation)
    {
        var sujet = "Confirmation de réservation";
        var corps = $@"
            <html>
            <body>
                <h2>Confirmation de réservation</h2>
                <p>Bonjour {prenom} {nom},</p>
                <p>Votre réservation a été confirmée avec le numéro : <strong>{numeroReservation}</strong></p>
                <p>Merci de votre confiance !</p>
            </body>
            </html>";
        await EnvoyerEmailAsync(to, sujet, corps);
    }

    public async Task EnvoyerFactureParEmailAsync(string to, string nom, string prenom, string factureUrl)
    {
        var sujet = "Votre facture de location";
        var corps = $@"
            <html>
            <body>
                <h2>Facture de location</h2>
                <p>Bonjour {prenom} {nom},</p>
                <p>Veuillez trouver ci-joint votre facture de location.</p>
                <p><a href='{factureUrl}'>Télécharger la facture</a></p>
            </body>
            </html>";
        await EnvoyerEmailAsync(to, sujet, corps);
    }

    public async Task EnvoyerAlerteEntretienAsync(string to, string vehiculeInfo, DateTime dateEntretien)
    {
        var sujet = "Alerte : Entretien de véhicule requis";
        var corps = $@"
            <html>
            <body>
                <h2>Alerte d'entretien</h2>
                <p>Le véhicule {vehiculeInfo} nécessite un entretien prévu le {dateEntretien:dd/MM/yyyy}.</p>
            </body>
            </html>";
        await EnvoyerEmailAsync(to, sujet, corps);
    }

    public async Task EnvoyerNotificationAsync(string to, string sujet, string message)
    {
        var corps = $@"
            <html>
            <body>
                <p>{message}</p>
            </body>
            </html>";
        await EnvoyerEmailAsync(to, sujet, corps);
    }

    public async Task EnvoyerRappelAsync(string to, string message)
    {
        var sujet = "Rappel - Location Voitures";
        var corps = $@"
            <html>
            <body>
                <p>{message}</p>
            </body>
            </html>";
        await EnvoyerEmailAsync(to, sujet, corps);
    }

    public async Task EnvoyerCodeResetPasswordAsync(string to, string nom, string code)
    {
        var sujet = "Réinitialisation de mot de passe";
        var corps = $@"
            <html>
            <body>
                <h2>Réinitialisation de mot de passe</h2>
                <p>Bonjour {nom},</p>
                <p>Vous avez demandé à réinitialiser votre mot de passe.</p>
                <p>Votre code de vérification est : <strong style='font-size: 24px; color: #007bff;'>{code}</strong></p>
                <p>Ce code est valide pendant 15 minutes.</p>
                <p>Si vous n'avez pas demandé cette réinitialisation, veuillez ignorer cet email.</p>
                <br/>
                <p>Cordialement,<br/>L'équipe Location Voitures</p>
            </body>
            </html>";
        await EnvoyerEmailAsync(to, sujet, corps);
    }
}

