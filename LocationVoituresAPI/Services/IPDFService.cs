using LocationVoituresAPI.Models;

namespace LocationVoituresAPI.Services;

public interface IPDFService
{
    Task<byte[]> GenererFactureAsync(Location location);
    Task<byte[]> GenererContratAsync(Location location);
    Task<byte[]> GenererRapportPDFAsync(Rapport rapport);
    Task<byte[]> GenererRecuAsync(Paiement paiement);
    Task<byte[]> GenererBonReservationAsync(Location location);
}

