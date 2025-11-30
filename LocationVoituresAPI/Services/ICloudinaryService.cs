namespace LocationVoituresAPI.Services;

public interface ICloudinaryService
{
    Task<string> UploadImageAsync(Stream imageStream, string fileName);
    Task<string> UploadPDFAsync(Stream pdfStream, string fileName);
    Task<List<string>> UploadMultipleImagesAsync(List<(Stream stream, string fileName)> images);
    Task<bool> DeleteImageAsync(string publicId);
    string GetImageUrl(string publicId);
}

