using iTextSharp.text;
using iTextSharp.text.pdf;
using LocationVoituresAPI.Models;

namespace LocationVoituresAPI.Services;

public class PDFService : IPDFService
{
    private readonly ICloudinaryService _cloudinaryService;

    public PDFService(ICloudinaryService cloudinaryService)
    {
        _cloudinaryService = cloudinaryService;
    }

    public Task<byte[]> GenererFactureAsync(Location location)
    {
        using var ms = new MemoryStream();
        var document = new Document(PageSize.A4, 50, 50, 25, 25);
        var writer = PdfWriter.GetInstance(document, ms);

        document.Open();

        // En-tête
        var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
        var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 12);

        document.Add(new Paragraph("FACTURE DE LOCATION", titleFont));
        document.Add(new Paragraph($"Numéro: LOC-{location.Id:D6}", normalFont));
        document.Add(new Paragraph($"Date: {DateTime.Now:dd/MM/yyyy}", normalFont));
        document.Add(new Paragraph(" "));

        // Informations client
        document.Add(new Paragraph("CLIENT:", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12)));
        document.Add(new Paragraph($"{location.Client.Utilisateur.Prenom} {location.Client.Utilisateur.Nom}", normalFont));
        document.Add(new Paragraph(location.Client.Utilisateur.Email, normalFont));
        document.Add(new Paragraph(" "));

        // Informations véhicule
        document.Add(new Paragraph("VÉHICULE:", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12)));
        document.Add(new Paragraph($"{location.Vehicule.Marque} {location.Vehicule.Modele}", normalFont));
        document.Add(new Paragraph($"Immatriculation: {location.Vehicule.Immatriculation}", normalFont));
        document.Add(new Paragraph(" "));

        // Détails location
        document.Add(new Paragraph("DÉTAILS DE LA LOCATION:", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12)));
        document.Add(new Paragraph($"Date de début: {location.DateDebut:dd/MM/yyyy}", normalFont));
        document.Add(new Paragraph($"Date de fin: {location.DateFin:dd/MM/yyyy}", normalFont));
        document.Add(new Paragraph($"Durée: {location.DureeJours} jours", normalFont));
        document.Add(new Paragraph(" "));

        // Total
        var totalFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);
        document.Add(new Paragraph($"TOTAL: {location.MontantTotal:C}", totalFont));

        document.Close();
        return Task.FromResult(ms.ToArray());
    }

    public Task<byte[]> GenererContratAsync(Location location)
    {
        using var ms = new MemoryStream();
        var document = new Document(PageSize.A4, 50, 50, 25, 25);
        var writer = PdfWriter.GetInstance(document, ms);

        document.Open();

        var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
        var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 12);

        document.Add(new Paragraph("CONTRAT DE LOCATION", titleFont));
        document.Add(new Paragraph(" "));
        document.Add(new Paragraph($"Contrat de location n° LOC-{location.Id:D6}", normalFont));
        document.Add(new Paragraph(" "));
        document.Add(new Paragraph("Entre les soussignés:", normalFont));
        document.Add(new Paragraph(" "));
        document.Add(new Paragraph($"Loueur: Location Voitures", normalFont));
        document.Add(new Paragraph($"Locataire: {location.Client.Utilisateur.Prenom} {location.Client.Utilisateur.Nom}", normalFont));
        document.Add(new Paragraph(" "));
        document.Add(new Paragraph($"Véhicule: {location.Vehicule.Marque} {location.Vehicule.Modele} - {location.Vehicule.Immatriculation}", normalFont));
        document.Add(new Paragraph($"Période: du {location.DateDebut:dd/MM/yyyy} au {location.DateFin:dd/MM/yyyy}", normalFont));
        document.Add(new Paragraph($"Montant: {location.MontantTotal:C}", normalFont));

        document.Close();
        return Task.FromResult(ms.ToArray());
    }

    public Task<byte[]> GenererRapportPDFAsync(Rapport rapport)
    {
        using var ms = new MemoryStream();
        var document = new Document(PageSize.A4, 50, 50, 25, 25);
        var writer = PdfWriter.GetInstance(document, ms);

        document.Open();

        var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
        var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 12);

        document.Add(new Paragraph($"RAPPORT: {rapport.TypeRapport}", titleFont));
        document.Add(new Paragraph($"Date de génération: {rapport.DateGeneration:dd/MM/yyyy HH:mm}", normalFont));
        document.Add(new Paragraph(" "));
        document.Add(new Paragraph("Données:", normalFont));
        document.Add(new Paragraph(rapport.Donnees, normalFont));

        document.Close();
        return Task.FromResult(ms.ToArray());
    }

    public Task<byte[]> GenererRecuAsync(Paiement paiement)
    {
        using var ms = new MemoryStream();
        var document = new Document(PageSize.A4, 50, 50, 25, 25);
        var writer = PdfWriter.GetInstance(document, ms);

        document.Open();

        var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
        var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 12);

        document.Add(new Paragraph("REÇU DE PAIEMENT", titleFont));
        document.Add(new Paragraph($"Référence: {paiement.Reference}", normalFont));
        document.Add(new Paragraph($"Date: {paiement.DatePaiement:dd/MM/yyyy}", normalFont));
        document.Add(new Paragraph(" "));
        document.Add(new Paragraph($"Montant: {paiement.Montant:C}", normalFont));
        document.Add(new Paragraph($"Mode de paiement: {paiement.ModePaiement}", normalFont));
        document.Add(new Paragraph($"Statut: {paiement.Statut}", normalFont));

        document.Close();
        return Task.FromResult(ms.ToArray());
    }

    public Task<byte[]> GenererBonReservationAsync(Location location)
    {
        using var ms = new MemoryStream();
        var document = new Document(PageSize.A4, 50, 50, 25, 25);
        var writer = PdfWriter.GetInstance(document, ms);

        document.Open();

        var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
        var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 12);

        document.Add(new Paragraph("BON DE RÉSERVATION", titleFont));
        document.Add(new Paragraph($"Numéro: LOC-{location.Id:D6}", normalFont));
        document.Add(new Paragraph(" "));
        document.Add(new Paragraph($"Client: {location.Client.Utilisateur.Prenom} {location.Client.Utilisateur.Nom}", normalFont));
        document.Add(new Paragraph($"Véhicule: {location.Vehicule.Marque} {location.Vehicule.Modele}", normalFont));
        document.Add(new Paragraph($"Période: {location.DateDebut:dd/MM/yyyy} - {location.DateFin:dd/MM/yyyy}", normalFont));
        document.Add(new Paragraph($"Montant: {location.MontantTotal:C}", normalFont));

        document.Close();
        return Task.FromResult(ms.ToArray());
    }
}

