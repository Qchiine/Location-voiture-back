using Microsoft.EntityFrameworkCore;
using LocationVoituresAPI.Models;

namespace LocationVoituresAPI.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Utilisateur> Utilisateurs { get; set; }
    public DbSet<Employe> Employes { get; set; }
    public DbSet<Client> Clients { get; set; }
    public DbSet<TypeVehicule> TypeVehicules { get; set; }
    public DbSet<Vehicule> Vehicules { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<Paiement> Paiements { get; set; }
    public DbSet<Entretien> Entretiens { get; set; }
    public DbSet<Rapport> Rapports { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuration Utilisateur
        modelBuilder.Entity<Utilisateur>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            // Convertir l'enum TypeUtilisateur en string pour MySQL
            entity.Property(e => e.TypeUtilisateur)
                  .HasConversion<string>()
                  .HasMaxLength(50);
        });

        // Configuration Employe
        modelBuilder.Entity<Employe>(entity =>
        {
            entity.HasIndex(e => e.Matricule).IsUnique();
            entity.HasOne(e => e.Utilisateur)
                  .WithOne(u => u.Employe)
                  .HasForeignKey<Employe>(e => e.Id)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configuration Client
        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasIndex(e => e.NumeroPermis).IsUnique();
            entity.HasOne(c => c.Utilisateur)
                  .WithOne(u => u.Client)
                  .HasForeignKey<Client>(c => c.Id)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configuration Vehicule
        modelBuilder.Entity<Vehicule>(entity =>
        {
            entity.HasIndex(e => e.Immatriculation).IsUnique();
            entity.HasOne(v => v.TypeVehicule)
                  .WithMany(t => t.Vehicules)
                  .HasForeignKey(v => v.TypeVehiculeId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configuration Location
        modelBuilder.Entity<Location>(entity =>
        {
            entity.HasOne(l => l.Client)
                  .WithMany(c => c.Locations)
                  .HasForeignKey(l => l.ClientId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(l => l.Vehicule)
                  .WithMany(v => v.Locations)
                  .HasForeignKey(l => l.VehiculeId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(l => l.Employe)
                  .WithMany(e => e.Locations)
                  .HasForeignKey(l => l.EmployeId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(l => l.Paiement)
                  .WithOne(p => p.Location)
                  .HasForeignKey<Paiement>(p => p.LocationId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configuration Paiement
        modelBuilder.Entity<Paiement>(entity =>
        {
            entity.HasIndex(e => e.Reference).IsUnique();
        });

        // Configuration Entretien
        modelBuilder.Entity<Entretien>(entity =>
        {
            entity.HasOne(e => e.Vehicule)
                  .WithMany(v => v.Entretiens)
                  .HasForeignKey(e => e.VehiculeId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Employe)
                  .WithMany(emp => emp.Entretiens)
                  .HasForeignKey(e => e.EmployeId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Configuration Rapport
        modelBuilder.Entity<Rapport>(entity =>
        {
            entity.HasOne(r => r.Administrateur)
                  .WithMany()
                  .HasForeignKey(r => r.AdministrateurId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

