namespace LocationVoituresAPI.Services;

public interface IExportService
{
    Task<byte[]> ExporterExcelAsync<T>(List<T> data, string sheetName = "Données");
    Task<byte[]> ExporterCSVAsync<T>(List<T> data);
    Task<byte[]> ExporterDonneesAsync<T>(List<T> data, string format);
    Task<List<T>> ImporterDonneesAsync<T>(Stream stream, string format);
}

