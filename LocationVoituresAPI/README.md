# Location Voitures API

API REST pour la gestion de location de voitures développée avec .NET 9.0 et Entity Framework Core.

## Fonctionnalités

- **Authentification JWT** : Système d'authentification sécurisé avec JWT
- **Gestion des utilisateurs** : Administrateurs, Employés, Clients
- **Gestion des véhicules** : CRUD complet avec upload d'images via Cloudinary
- **Gestion des locations** : Réservation, confirmation, annulation avec génération de QR codes
- **Gestion des paiements** : Suivi des paiements avec génération de reçus
- **Gestion des entretiens** : Planification et suivi des entretiens de véhicules
- **Génération de documents** : Factures, contrats, reçus en PDF
- **Statistiques** : Calculs de statistiques en temps réel
- **Export de données** : Export Excel et CSV

## Prérequis

- .NET 9.0 SDK
- MySQL Server 8.0+
- Compte Cloudinary (pour l'upload d'images)
- Compte email SMTP (pour l'envoi d'emails)

## Installation

1. **Cloner le projet**
   ```bash
   git clone <repository-url>
   cd LocationVoituresAPI
   ```

2. **Créer la base de données**
   - Exécuter le script SQL dans `Database/CreateDatabase.sql` dans MySQL Workbench
   - Ou utiliser la commande :
   ```bash
   mysql -u root -p < Database/CreateDatabase.sql
   ```

3. **Configurer appsettings.json**
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=LOCATION_VOITURES;User=root;Password=votre_mot_de_passe;Port=3306;CharSet=utf8mb4;"
     },
     "Jwt": {
       "Key": "VotreCleSecreteSuperLongueEtSecuriseePourJWT2024!",
       "Issuer": "LocationVoituresAPI",
       "Audience": "LocationVoituresAPI",
       "ExpirationInMinutes": 1440
     },
     "Cloudinary": {
       "CloudName": "votre-cloud-name",
       "ApiKey": "votre-api-key",
       "ApiSecret": "votre-api-secret"
     },
     "Email": {
       "SmtpServer": "smtp.gmail.com",
       "SmtpPort": 587,
       "SenderEmail": "votre-email@gmail.com",
       "SenderPassword": "votre-mot-de-passe"
     }
   }
   ```

4. **Installer les dépendances**
   ```bash
   dotnet restore
   ```

5. **Créer les migrations (optionnel)**
   ```bash
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

6. **Lancer l'application**
   ```bash
   dotnet run
   ```

L'API sera accessible sur `https://localhost:5001` ou `http://localhost:5000`

## Documentation API

Une fois l'application lancée, accédez à la documentation Swagger :
- **Swagger UI** : `https://localhost:5001/swagger`

## Endpoints principaux

### Authentification
- `POST /api/auth/login` - Connexion
- `POST /api/auth/register` - Inscription
- `POST /api/auth/refresh-token` - Rafraîchir le token

### Locations
- `GET /api/locations` - Liste des locations
- `POST /api/locations` - Créer une location
- `GET /api/locations/{id}` - Détails d'une location
- `PUT /api/locations/{id}` - Modifier une location
- `POST /api/locations/{id}/generer-facture` - Générer la facture
- `POST /api/locations/{id}/confirmer` - Confirmer une location
- `POST /api/locations/{id}/annuler` - Annuler une location

### Véhicules
- `GET /api/vehicules` - Liste des véhicules
- `GET /api/vehicules/disponibles` - Véhicules disponibles
- `GET /api/vehicules/{id}` - Détails d'un véhicule
- `POST /api/vehicules` - Créer un véhicule (Admin/Employé)
- `PUT /api/vehicules/{id}` - Modifier un véhicule (Admin/Employé)
- `DELETE /api/vehicules/{id}` - Supprimer un véhicule (Admin)
- `POST /api/vehicules/{id}/upload-images` - Upload d'images (Admin/Employé)

### Paiements
- `POST /api/paiements` - Créer un paiement
- `GET /api/paiements/{id}` - Détails d'un paiement
- `PUT /api/paiements/{id}/statut` - Modifier le statut (Admin/Employé)
- `GET /api/paiements/{id}/recu` - Générer le reçu PDF

### Entretiens
- `GET /api/entretiens` - Liste des entretiens
- `GET /api/entretiens/urgents` - Entretiens urgents
- `GET /api/entretiens/{id}` - Détails d'un entretien
- `POST /api/entretiens` - Créer un entretien (Admin/Employé)
- `PUT /api/entretiens/{id}` - Modifier un entretien (Admin/Employé)
- `POST /api/entretiens/{id}/terminer` - Terminer un entretien (Admin/Employé)
- `DELETE /api/entretiens/{id}` - Supprimer un entretien (Admin)

## Structure du projet

```
LocationVoituresAPI/
├── Controllers/          # Contrôleurs API
├── Data/                 # DbContext et configuration EF
├── DTOs/                 # Data Transfer Objects
├── Models/               # Modèles de données
├── Services/             # Services métier
├── Database/             # Scripts SQL
└── Program.cs            # Point d'entrée de l'application
```

## Technologies utilisées

- **.NET 9.0** - Framework
- **Entity Framework Core 9.0** - ORM
- **Pomelo.EntityFrameworkCore.MySql** - Provider MySQL
- **JWT Bearer** - Authentification
- **BCrypt.Net** - Hashage des mots de passe
- **QRCoder** - Génération de QR codes
- **Cloudinary** - Gestion d'images
- **iTextSharp** - Génération de PDF
- **EPPlus** - Export Excel
- **MailKit** - Envoi d'emails
- **Swagger/OpenAPI** - Documentation API

## Sécurité

- Authentification JWT avec expiration
- Hashage des mots de passe avec BCrypt
- Autorisation basée sur les rôles (Admin, Employé, Client)
- CORS configuré
- Validation des données avec Data Annotations

## Notes

- Assurez-vous de configurer correctement les clés JWT dans `appsettings.json`
- Les identifiants Cloudinary et SMTP doivent être configurés pour utiliser les fonctionnalités correspondantes
- La base de données doit être créée avant le premier lancement

## Licence

Ce projet est développé pour un usage éducatif.

