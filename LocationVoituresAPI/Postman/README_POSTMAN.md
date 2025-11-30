# Guide d'utilisation de la collection Postman

Ce guide vous explique comment importer et utiliser la collection Postman pour tester l'API Location Voitures.

## 📦 Fichiers inclus

1. **LocationVoituresAPI.postman_collection.json** - Collection complète avec tous les endpoints
2. **LocationVoituresAPI.postman_environment.json** - Variables d'environnement

## 🚀 Installation

### Étape 1 : Importer la collection

1. Ouvrez Postman
2. Cliquez sur **Import** (en haut à gauche)
3. Sélectionnez le fichier `LocationVoituresAPI.postman_collection.json`
4. Cliquez sur **Import**

### Étape 2 : Importer l'environnement

1. Cliquez sur **Import** à nouveau
2. Sélectionnez le fichier `LocationVoituresAPI.postman_environment.json`
3. Cliquez sur **Import**
4. Sélectionnez l'environnement "Location Voitures - Environment" dans le menu déroulant en haut à droite

### Étape 3 : Configurer l'URL de base

Si votre API tourne sur un autre port, modifiez la variable `baseUrl` dans l'environnement :
- Par défaut : `https://localhost:5001`
- Alternative : `http://localhost:5000`

## 📋 Structure de la collection

### 1. Authentification
- **Register - Client** : Créer un compte client
- **Register - Employé** : Créer un compte employé
- **Login** : Se connecter (sauvegarde automatiquement le token)
- **Refresh Token** : Rafraîchir le token JWT

### 2. Véhicules
- **Get All Véhicules** : Liste tous les véhicules
- **Get Véhicules Disponibles** : Véhicules disponibles pour une période
- **Get Véhicule by ID** : Détails d'un véhicule
- **Create Véhicule** : Créer un véhicule (Admin/Employé)
- **Update Véhicule** : Modifier un véhicule (Admin/Employé)
- **Upload Images Véhicule** : Upload d'images (Admin/Employé)
- **Delete Véhicule** : Supprimer un véhicule (Admin)

### 3. Locations
- **Get All Locations** : Liste toutes les locations
- **Get Location by ID** : Détails d'une location
- **Create Location** : Créer une location (sauvegarde l'ID automatiquement)
- **Update Location** : Modifier une location
- **Générer Facture** : Générer et envoyer la facture par email
- **Confirmer Location** : Confirmer une location
- **Annuler Location** : Annuler une location

### 4. Paiements
- **Create Paiement** : Créer un paiement (sauvegarde l'ID automatiquement)
- **Get Paiement by ID** : Détails d'un paiement
- **Update Statut Paiement** : Modifier le statut (Admin/Employé)
- **Générer Reçu PDF** : Télécharger le reçu PDF

### 5. Entretiens
- **Get All Entretiens** : Liste tous les entretiens
- **Get Entretiens Urgents** : Liste des entretiens urgents
- **Get Entretien by ID** : Détails d'un entretien
- **Create Entretien** : Créer un entretien (Admin/Employé)
- **Update Entretien** : Modifier un entretien (Admin/Employé)
- **Terminer Entretien** : Marquer un entretien comme terminé
- **Delete Entretien** : Supprimer un entretien (Admin)

## 🔐 Authentification automatique

La collection inclut des scripts de test qui :
- Sauvegardent automatiquement le token JWT après login/register
- Sauvegardent les IDs créés (locationId, paiementId, entretienId)
- Ajoutent automatiquement le token Bearer dans les headers

## 📝 Ordre recommandé pour tester

1. **Créer un compte** : Utilisez "Register - Client" ou "Register - Employé"
2. **Se connecter** : Utilisez "Login" (le token sera sauvegardé automatiquement)
3. **Créer un TypeVehicule** (via SQL ou directement en base) :
   ```sql
   INSERT INTO TYPEVEHICULE (NOM, DESCRIPTION, CATEGORIE, PRIXBASEJOURNALIER) 
   VALUES ('Berline', 'Véhicule berline confortable', 'BERLINE', 50.00);
   ```
4. **Créer un véhicule** : Utilisez "Create Véhicule"
5. **Créer une location** : Utilisez "Create Location"
6. **Créer un paiement** : Utilisez "Create Paiement"
7. **Générer la facture** : Utilisez "Générer Facture"
8. **Tester les autres endpoints** selon vos besoins

## 🔧 Variables d'environnement

Les variables suivantes sont automatiquement gérées :

| Variable | Description | Mise à jour automatique |
|----------|-------------|------------------------|
| `baseUrl` | URL de base de l'API | Manuel |
| `authToken` | Token JWT | Automatique (après login/register) |
| `refreshToken` | Refresh token | Automatique |
| `userId` | ID de l'utilisateur connecté | Automatique |
| `userType` | Type d'utilisateur (CLIENT, EMPLOYE, ADMINISTRATEUR) | Automatique |
| `locationId` | ID de la dernière location créée | Automatique |
| `paiementId` | ID du dernier paiement créé | Automatique |
| `entretienId` | ID du dernier entretien créé | Automatique |

## ⚠️ Notes importantes

1. **HTTPS** : Si vous avez des problèmes avec HTTPS, changez `baseUrl` en `http://localhost:5000`
2. **Certificat SSL** : Pour HTTPS, vous devrez peut-être désactiver la vérification SSL dans Postman (Settings → SSL certificate verification)
3. **Rôles** : Certains endpoints nécessitent des rôles spécifiques (Admin ou Employé)
4. **Données de test** : Modifiez les données dans les requêtes selon vos besoins

## 🐛 Dépannage

### Erreur 401 Unauthorized
- Vérifiez que vous avez bien exécuté "Login" ou "Register"
- Vérifiez que le token est bien sauvegardé dans la variable `authToken`

### Erreur 404 Not Found
- Vérifiez que l'API est bien lancée (`dotnet run`)
- Vérifiez que l'URL dans `baseUrl` est correcte

### Erreur de connexion
- Vérifiez que MySQL est démarré
- Vérifiez la chaîne de connexion dans `appsettings.json`
- Vérifiez que la base de données `LOCATION_VOITURES` existe

## 📚 Ressources

- [Documentation Postman](https://learning.postman.com/)
- [Swagger UI](https://localhost:5001/swagger) - Documentation interactive de l'API

