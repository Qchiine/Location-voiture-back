using LocationVoituresAPI.Data;
using LocationVoituresAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace LocationVoituresAPI.Scripts;

/// <summary>
/// Script pour créer un compte administrateur
/// Usage: Appeler cette méthode depuis Program.cs au démarrage ou via un endpoint temporaire
/// </summary>
public static class CreateAdminScript
{
    public static async Task CreateAdminUser(ApplicationDbContext context)
    {
        // Paramètres de l'admin
        string adminEmail = "admin@location-voitures.fr";
        string adminPassword = "Admin@123";
        
        // Vérifier si l'admin existe déjà
        var existingAdmin = await context.Utilisateurs
            .FirstOrDefaultAsync(u => u.Email == adminEmail);

        if (existingAdmin != null)
        {
            Console.WriteLine($"❌ Un utilisateur avec l'email {adminEmail} existe déjà.");
            return;
        }

        // Hasher le mot de passe avec BCrypt
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword);

        // Créer l'utilisateur administrateur
        var adminUser = new Utilisateur
        {
            Nom = "Admin",
            Prenom = "Système",
            Email = adminEmail,
            MotDePasseHash = passwordHash,
            DateCreation = DateTime.Now,
            EstActif = true,
            TypeUtilisateur = TypeUtilisateur.ADMINISTRATEUR
        };

        // Ajouter à la base de données
        context.Utilisateurs.Add(adminUser);
        await context.SaveChangesAsync();

        // Créer l'employé associé
        var employe = new Employe
        {
            Id = adminUser.Id,
            Matricule = $"EMP{adminUser.Id:D5}", // Ex: EMP00001
            DateEmbauche = DateTime.Now,
            Utilisateur = adminUser
        };

        context.Employes.Add(employe);
        await context.SaveChangesAsync();

        Console.WriteLine("✅ Compte administrateur créé avec succès!");
        Console.WriteLine($"   Email: {adminEmail}");
        Console.WriteLine($"   Mot de passe: {adminPassword}");
        Console.WriteLine($"   Matricule: {employe.Matricule}");
        Console.WriteLine("\n⚠️  IMPORTANT: Changez ce mot de passe après la première connexion!");
    }

    /// <summary>
    /// Créer un admin personnalisé
    /// </summary>
    public static async Task CreateCustomAdmin(
        ApplicationDbContext context,
        string nom,
        string prenom,
        string email,
        string password)
    {
        // Vérifier si l'email existe déjà
        var existingUser = await context.Utilisateurs
            .FirstOrDefaultAsync(u => u.Email == email);

        if (existingUser != null)
        {
            Console.WriteLine($"❌ Un utilisateur avec l'email {email} existe déjà.");
            return;
        }

        // Hasher le mot de passe
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        // Créer l'utilisateur
        var adminUser = new Utilisateur
        {
            Nom = nom,
            Prenom = prenom,
            Email = email,
            MotDePasseHash = passwordHash,
            DateCreation = DateTime.Now,
            EstActif = true,
            TypeUtilisateur = TypeUtilisateur.ADMINISTRATEUR
        };

        context.Utilisateurs.Add(adminUser);
        await context.SaveChangesAsync();

        // Créer l'employé associé
        var employe = new Employe
        {
            Id = adminUser.Id,
            Matricule = $"EMP{adminUser.Id:D5}",
            DateEmbauche = DateTime.Now,
            Utilisateur = adminUser
        };

        context.Employes.Add(employe);
        await context.SaveChangesAsync();

        Console.WriteLine("✅ Compte administrateur créé avec succès!");
        Console.WriteLine($"   Nom: {nom} {prenom}");
        Console.WriteLine($"   Email: {email}");
        Console.WriteLine($"   Matricule: {employe.Matricule}");
    }
}
