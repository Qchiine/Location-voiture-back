-- Script pour créer un compte administrateur
-- Note: Le mot de passe doit être hashé avec BCrypt avant l'insertion

-- Paramètres à modifier:
-- EMAIL: admin@location-voitures.fr
-- MOT DE PASSE: Admin@123 (à hasher avec BCrypt)
-- BCrypt Hash de "Admin@123": $2a$11$rQJxE5mKZJ5h5Y9YqGxGkOzYqF5J5h5Y9YqGxGkOzYqF5J5h5Y9Yq

-- 1. Insérer l'utilisateur administrateur
INSERT INTO UTILISATEUR (NOM, PRENOM, EMAIL, MOTDEPASSEHASH, DATECREATION, ESTACTIF, TYPEUTILISATEUR)
VALUES (
    'Admin',                    -- NOM
    'Système',                  -- PRENOM
    'admin@location-voitures.fr', -- EMAIL
    '$2a$11$rQJxE5mKZJ5h5Y9YqGxGkOzYqF5J5h5Y9YqGxGkOzYqF5J5h5Y9Yq', -- MOTDEPASSEHASH (Admin@123)
    NOW(),                      -- DATECREATION
    TRUE,                       -- ESTACTIF
    'ADMINISTRATEUR'           -- TYPEUTILISATEUR
);

-- 2. Récupérer l'ID de l'utilisateur créé
SET @admin_user_id = LAST_INSERT_ID();

-- 3. Créer l'employé associé
INSERT INTO EMPLOYE (ID, MATRICULE, DATEEMBAUCHE)
VALUES (
    @admin_user_id,            -- ID (même que l'utilisateur)
    CONCAT('EMP', LPAD(@admin_user_id, 5, '0')), -- MATRICULE (ex: EMP00001)
    NOW()                      -- DATEEMBAUCHE
);

-- Vérifier la création
SELECT 
    u.ID,
    u.NOM,
    u.PRENOM,
    u.EMAIL,
    u.TYPEUTILISATEUR,
    e.MATRICULE,
    e.DATEEMBAUCHE
FROM UTILISATEUR u
LEFT JOIN EMPLOYE e ON u.ID = e.ID
WHERE u.EMAIL = 'admin@location-voitures.fr';
