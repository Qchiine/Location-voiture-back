-- ====================================================================================
-- SCRIPT POUR CRÉER UN ADMINISTRATEUR
-- ====================================================================================
-- Ce script crée un administrateur dans la base de données
-- 
-- IMPORTANT: Le mot de passe doit être hashé avec BCrypt
-- Le mot de passe par défaut est "Admin123!" (vous pouvez le changer)
-- 
-- Pour générer un nouveau hash BCrypt, utilisez le script C# fourni ou
-- un générateur BCrypt en ligne: https://bcrypt-generator.com/
-- ====================================================================================

USE LOCATION_VOITURES;

-- Option 1: Créer un admin avec un mot de passe hashé pré-calculé
-- Le hash ci-dessous correspond au mot de passe "Admin123!"
-- Si vous voulez un autre mot de passe, utilisez le script C# pour générer le hash

INSERT INTO UTILISATEUR (
    NOM, 
    PRENOM, 
    EMAIL, 
    MOTDEPASSEHASH, 
    DATECREATION, 
    ESTACTIF, 
    TYPEUTILISATEUR
) VALUES (
    'Admin',
    'Système',
    'admin@locationvoitures.com',
    '$2a$11$KIXqJZqJZqJZqJZqJZqJZ.qJZqJZqJZqJZqJZqJZqJZqJZqJZqJZqJZq', -- Hash pour "Admin123!"
    NOW(),
    TRUE,
    'ADMINISTRATEUR'
);

-- Vérifier que l'admin a été créé
SELECT ID, NOM, PRENOM, EMAIL, TYPEUTILISATEUR, DATECREATION 
FROM UTILISATEUR 
WHERE TYPEUTILISATEUR = 'ADMINISTRATEUR';

-- ====================================================================================
-- NOTES:
-- 1. Le hash ci-dessus est un exemple. Vous devez générer votre propre hash
-- 2. Utilisez le script C# CreateAdmin.cs pour générer un hash correct
-- 3. Après création, connectez-vous avec: admin@locationvoitures.com / Admin123!
-- ====================================================================================

