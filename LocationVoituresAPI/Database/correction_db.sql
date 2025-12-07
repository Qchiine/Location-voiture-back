-- Script de correction pour la base de données Location Voitures
-- À exécuter sur votre base MySQL existante

USE LocationVoitures;

-- 1. Ajout de la table pour réinitialisation de mot de passe
CREATE TABLE IF NOT EXISTS PasswordResetToken (
    ID INT PRIMARY KEY AUTO_INCREMENT,
    UtilisateurID INT NOT NULL,
    Code VARCHAR(6) NOT NULL,
    DateExpiration DATETIME NOT NULL,
    EstUtilise BOOLEAN NOT NULL DEFAULT FALSE,
    DateCreation DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (UtilisateurID) REFERENCES Utilisateur(ID) ON DELETE CASCADE
);

-- 2. Modifier les colonnes ENUM pour correspondre aux modèles C# (avec underscores)
-- Note: MySQL ENUM ne supporte pas les underscores dans les valeurs directement
-- La solution est de stocker les enums comme VARCHAR et laisser EF Core gérer la conversion

-- Modifier TypeUtilisateur
ALTER TABLE Utilisateur MODIFY COLUMN TypeUtilisateur VARCHAR(50) NOT NULL;

-- Modifier Statut de Location
ALTER TABLE Location MODIFY COLUMN Statut VARCHAR(50) DEFAULT 'EN_ATTENTE';

-- Modifier ModePaiement et Statut de Paiement
ALTER TABLE Paiement MODIFY COLUMN ModePaiement VARCHAR(50) NOT NULL;
ALTER TABLE Paiement MODIFY COLUMN Statut VARCHAR(50) DEFAULT 'EN_ATTENTE';

-- Modifier TypeEntretien et Statut d'Entretien
ALTER TABLE Entretien MODIFY COLUMN TypeEntretien VARCHAR(100) NOT NULL;
ALTER TABLE Entretien MODIFY COLUMN Statut VARCHAR(50) DEFAULT 'PLANIFIE';

-- Modifier TypeRapport et Format de Rapport
ALTER TABLE Rapport MODIFY COLUMN TypeRapport VARCHAR(100) NOT NULL;
ALTER TABLE Rapport MODIFY COLUMN Format VARCHAR(50) DEFAULT 'PDF';

-- 3. Vérifier que AdministrateurID existe dans la table Rapport
-- (Certaines anciennes bases peuvent avoir EmployeID à la place)
-- Cette colonne doit pointer vers n'importe quel Utilisateur (pas seulement admin)
ALTER TABLE Rapport 
    DROP FOREIGN KEY IF EXISTS rapport_ibfk_1;

ALTER TABLE Rapport 
    ADD CONSTRAINT rapport_utilisateur_fk 
    FOREIGN KEY (AdministrateurID) 
    REFERENCES Utilisateur(ID) 
    ON DELETE CASCADE;

-- Afficher un message de confirmation
SELECT 'Base de données mise à jour avec succès!' as Message;
