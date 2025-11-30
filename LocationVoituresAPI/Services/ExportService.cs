using System.Globalization;
using System.Text;
using OfficeOpenXml;

namespace LocationVoituresAPI.Services;

public class ExportService : IExportService
{
    public ExportService()
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    public async Task<byte[]> ExporterExcelAsync<T>(List<T> data, string sheetName = "Données")
    {
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add(sheetName);

        // Headers
        var properties = typeof(T).GetProperties();
        for (int i = 0; i < properties.Length; i++)
        {
            worksheet.Cells[1, i + 1].Value = properties[i].Name;
        }

        // Data
        for (int row = 0; row < data.Count; row++)
        {
            for (int col = 0; col < properties.Length; col++)
            {
                var value = properties[col].GetValue(data[row]);
                worksheet.Cells[row + 2, col + 1].Value = value;
            }
        }

        worksheet.Cells.AutoFitColumns();
        return await Task.FromResult(package.GetAsByteArray());
    }

    public async Task<byte[]> ExporterCSVAsync<T>(List<T> data)
    {
        var csv = new StringBuilder();
        var properties = typeof(T).GetProperties();

        // Headers
        csv.AppendLine(string.Join(",", properties.Select(p => p.Name)));

        // Data
        foreach (var item in data)
        {
            var values = properties.Select(p =>
            {
                var value = p.GetValue(item);
                return value?.ToString()?.Replace(",", ";") ?? "";
            });
            csv.AppendLine(string.Join(",", values));
        }

        return await Task.FromResult(Encoding.UTF8.GetBytes(csv.ToString()));
    }

    public async Task<byte[]> ExporterDonneesAsync<T>(List<T> data, string format)
    {
        return format.ToUpper() switch
        {
            "EXCEL" => await ExporterExcelAsync(data),
            "CSV" => await ExporterCSVAsync(data),
            _ => throw new ArgumentException($"Format non supporté: {format}")
        };
    }

    public async Task<List<T>> ImporterDonneesAsync<T>(Stream stream, string format)
    {
        // Implémentation simplifiée - à compléter selon les besoins
        throw new NotImplementedException("Import non implémenté");
    }
}

