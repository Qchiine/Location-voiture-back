using System;
using BCrypt.Net;

namespace LocationVoituresAPI.Scripts;

/// <summary>
/// Script pour créer un administrateur et générer le hash BCrypt du mot de passe
/// Exécutez ce script dans une console ou utilisez-le pour générer le hash SQL
/// </summary>
public class CreateAdminScript
{
    public static void Main(string[] args)
    {
        Console.WriteLine("=== Script de création d'administrateur ===\n");

        // Configuration
        string nom = "Admin";
        string prenom = "Système";
        string email = "admin@locationvoitures.com";
        string motDePasse = "Admin123!"; // Changez ce mot de passe selon vos besoins

        // Générer le hash BCrypt
        string motDePasseHash = BCrypt.Net.BCrypt.HashPassword(motDePasse, BCrypt.Net.BCrypt.GenerateSalt());

        Console.WriteLine("Informations de l'administrateur:");
        Console.WriteLine($"Nom: {nom}");
        Console.WriteLine($"Prénom: {prenom}");
        Console.WriteLine($"Email: {email}");
        Console.WriteLine($"Mot de passe: {motDePasse}");
        Console.WriteLine($"\nHash BCrypt généré:");
        Console.WriteLine(motDePasseHash);
        Console.WriteLine("\n=== Requête SQL à exécuter ===\n");

        // Générer la requête SQL
        string sql = $@"INSERT INTO UTILISATEUR (
    NOM, 
    PRENOM, 
    EMAIL, 
    MOTDEPASSEHASH, 
    DATECREATION, 
    ESTACTIF, 
    TYPEUTILISATEUR
) VALUES (
    '{nom}',
    '{prenom}',
    '{email}',
    '{motDePasseHash}',
    NOW(),
    TRUE,
    'ADMINISTRATEUR'
);";

        Console.WriteLine(sql);
        Console.WriteLine("\n=== Copiez cette requête et exécutez-la dans MySQL Workbench ===");
    }
}

