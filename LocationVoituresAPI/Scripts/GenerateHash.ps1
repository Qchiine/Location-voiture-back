# Script PowerShell pour générer un hash BCrypt
# Utilisez ce script pour générer le hash de votre mot de passe

Write-Host "=== Générateur de hash BCrypt ===" -ForegroundColor Green
Write-Host ""

$password = Read-Host "Entrez le mot de passe pour l'administrateur"

# Note: Ce script nécessite d'avoir BCrypt.Net installé
# Pour l'utiliser, exécutez-le dans un projet .NET ou utilisez l'endpoint API

Write-Host ""
Write-Host "Pour générer le hash, utilisez l'une des méthodes suivantes:" -ForegroundColor Yellow
Write-Host "1. Utilisez l'endpoint API: POST /api/admin/create-first-admin" -ForegroundColor Cyan
Write-Host "2. Utilisez un générateur en ligne: https://bcrypt-generator.com/" -ForegroundColor Cyan
Write-Host "3. Exécutez le script C# CreateAdmin.cs" -ForegroundColor Cyan
Write-Host ""

