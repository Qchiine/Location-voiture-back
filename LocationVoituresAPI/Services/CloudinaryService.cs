using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace LocationVoituresAPI.Services;

public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService(IConfiguration configuration)
    {
        var account = new Account(
            configuration["Cloudinary:CloudName"] ?? "",
            configuration["Cloudinary:ApiKey"] ?? "",
            configuration["Cloudinary:ApiSecret"] ?? ""
        );
        _cloudinary = new Cloudinary(account);
    }

    public async Task<string> UploadImageAsync(Stream imageStream, string fileName)
    {
        // Réinitialiser la position du stream
        if (imageStream.CanSeek)
        {
            imageStream.Seek(0, SeekOrigin.Begin);
        }

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, imageStream),
            Folder = "location-voitures/qrcodes",
            Transformation = new Transformation().Quality("auto").FetchFormat("auto")
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams);
        
        if (uploadResult.Error != null)
        {
            throw new Exception($"Cloudinary upload error: {uploadResult.Error.Message}");
        }
        
        return uploadResult.SecureUrl.ToString();
    }

    public async Task<string> UploadPDFAsync(Stream pdfStream, string fileName)
    {
        var uploadParams = new RawUploadParams
        {
            File = new FileDescription(fileName, pdfStream),
            Folder = "location-voitures/factures"
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams);
        return uploadResult.SecureUrl.ToString();
    }

    public async Task<List<string>> UploadMultipleImagesAsync(List<(Stream stream, string fileName)> images)
    {
        var urls = new List<string>();
        foreach (var (stream, fileName) in images)
        {
            var url = await UploadImageAsync(stream, fileName);
            urls.Add(url);
        }
        return urls;
    }

    public async Task<bool> DeleteImageAsync(string publicId)
    {
        var deleteParams = new DeletionParams(publicId);
        var result = await _cloudinary.DestroyAsync(deleteParams);
        return result.Result == "ok";
    }

    public string GetImageUrl(string publicId)
    {
        return _cloudinary.Api.UrlImgUp.BuildUrl(publicId);
    }
}

