using System.ComponentModel.DataAnnotations;
using LocationVoituresAPI.Models;

namespace LocationVoituresAPI.DTOs;

public class CreateLocationDto
{
    [Required(ErrorMessage = "La date de début est requise")]
    public DateTime DateDebut { get; set; }

    [Required(ErrorMessage = "La date de fin est requise")]
    public DateTime DateFin { get; set; }

    [Required(ErrorMessage = "L'ID du client est requis")]
    public int ClientId { get; set; }

    [Required(ErrorMessage = "L'ID du véhicule est requis")]
    public int VehiculeId { get; set; }

    public int? EmployeId { get; set; }
}

public class UpdateLocationDto
{
    public DateTime? DateDebut { get; set; }
    public DateTime? DateFin { get; set; }
    public int? VehiculeId { get; set; }
    public StatutLocation? Statut { get; set; }
}
