# Instructions pour créer un administrateur

## 🎯 Méthode recommandée : Utiliser l'endpoint API

### Étape 1 : Lancer l'API
```bash
dotnet run
```

### Étape 2 : Utiliser Postman ou curl

**POST** `http://localhost:5269/api/admin/create-first-admin`

**Headers:**
```
Content-Type: application/json
```

**Body:**
```json
{
    "nom": "Admin",
    "prenom": "Système",
    "email": "admin@locationvoitures.com",
    "motDePasse": "Admin123!"
}
```

**Réponse attendue:**
```json
{
    "message": "Administrateur créé avec succès",
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "user": {
        "id": 1,
        "nom": "Admin",
        "prenom": "Système",
        "email": "admin@locationvoitures.com",
        "typeUtilisateur": "ADMINISTRATEUR"
    }
}
```

### Étape 3 : Utiliser le token pour créer des employés

Copiez le `token` de la réponse et utilisez-le dans l'header Authorization :
```
Authorization: Bearer <votre_token>
```

Ensuite, utilisez l'endpoint `/api/auth/register-employe` pour créer des employés.

---

## 📝 Méthode alternative : Script SQL direct

Si vous préférez utiliser SQL directement, vous devez d'abord générer un hash BCrypt.

### Option A : Utiliser un générateur en ligne
1. Allez sur https://bcrypt-generator.com/
2. Entrez votre mot de passe (ex: `Admin123!`)
3. Cliquez sur "Generate Hash"
4. Copiez le hash généré

### Option B : Utiliser le script C#
Le fichier `Scripts/CreateAdmin.cs` contient un script pour générer le hash.

### Option C : Utiliser le script SQL avec hash pré-calculé
Le fichier `Database/CreateAdmin_Ready.sql` contient un script avec un hash pour "Admin123!"

**⚠️ ATTENTION:** Le hash dans `CreateAdmin_Ready.sql` est un exemple. Pour la sécurité, générez votre propre hash.

---

## ✅ Vérification

Après création, vérifiez dans MySQL Workbench :

```sql
SELECT ID, NOM, PRENOM, EMAIL, TYPEUTILISATEUR, ESTACTIF 
FROM UTILISATEUR 
WHERE TYPEUTILISATEUR = 'ADMINISTRATEUR';
```

---

## 🔐 Informations de connexion par défaut

- **Email:** admin@locationvoitures.com
- **Mot de passe:** Admin123!
- **Type:** ADMINISTRATEUR

**⚠️ IMPORTANT:** Changez le mot de passe après la première connexion !

---

## 🚀 Après création de l'admin

1. Connectez-vous avec `/api/auth/login`
2. Utilisez le token pour créer des employés via `/api/auth/register-employe`
3. Les clients peuvent s'inscrire via `/api/auth/register`

---

## 🔒 Sécurité

**IMPORTANT:** Après avoir créé votre premier administrateur, l'endpoint `/api/admin/create-first-admin` devrait être désactivé ou sécurisé pour éviter la création non autorisée d'admins.

Pour créer d'autres admins, utilisez `/api/admin/create-admin` qui nécessite d'être déjà connecté en tant qu'administrateur.

