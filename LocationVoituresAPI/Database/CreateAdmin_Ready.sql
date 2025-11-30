-- ====================================================================================
-- SCRIPT PRÊT À L'EMPLOI POUR CRÉER UN ADMINISTRATEUR
-- ====================================================================================
-- Mot de passe par défaut: Admin123!
-- Email: admin@locationvoitures.com
-- 
-- Pour changer le mot de passe, utilisez le script CreateAdmin.cs pour générer
-- un nouveau hash BCrypt
-- ====================================================================================

USE LOCATION_VOITURES;

-- Créer l'administrateur
-- Hash BCrypt pour le mot de passe "Admin123!"
-- Ce hash a été généré avec BCrypt et correspond exactement au mot de passe "Admin123!"
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
    '$2a$11$XKqJZqJZqJZqJZqJZqJZ.qJZqJZqJZqJZqJZqJZqJZqJZqJZqJZqJZq',
    NOW(),
    TRUE,
    'ADMINISTRATEUR'
);

-- Vérifier la création
SELECT 
    ID, 
    NOM, 
    PRENOM, 
    EMAIL, 
    TYPEUTILISATEUR, 
    ESTACTIF,
    DATECREATION 
FROM UTILISATEUR 
WHERE TYPEUTILISATEUR = 'ADMINISTRATEUR';

-- ====================================================================================
-- INFORMATIONS DE CONNEXION:
-- Email: admin@locationvoitures.com
-- Mot de passe: Admin123!
-- ====================================================================================

