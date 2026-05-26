using System.Net;
using CanteenSystem.Application.Common.Exceptions;
using CanteenSystem.Application.Interfaces;
using CanteenSystem.Infrastructure.Settings;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CanteenSystem.Infrastructure.Services;

/// Handles images (JPEG, PNG, WebP) and documents (PDF).
/// Register as a singleton in DI — Cloudinary client is thread-safe. <summary>
/// Handles images (JPEG, PNG, WebP) and documents (PDF).
/// </summary>
public class CloudinaryMediaService : IMediaService
{

    // Maximum allowed size for image uploads (5 MB)
    private const long MaxImageSizeBytes = 5 * 1024 * 1024;

    // Maximum allowed size for document uploads (10 MB)
    private const long MaxDocumentSizeBytes = 10 * 1024 * 1024;

    /// Adding a new type here is all that is needed to support it — no other code changes required.
    private static readonly Dictionary<string, bool> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["image/jpeg"]       = false,
        ["image/png"]        = false,
        ["image/webp"]       = false,
        ["application/pdf"]  = true,
    };


    private readonly Cloudinary _cloudinary;
    private readonly ILogger<CloudinaryMediaService> _logger;

    public CloudinaryMediaService(
        IOptions<CloudinarySettings> config,
        ILogger<CloudinaryMediaService> logger)
    {
        var settings = config.Value;

        var account = new Account(
            settings.CloudName,
            settings.ApiKey,
            settings.ApiSecret
        );

        // One Cloudinary client shared for the lifetime of the app
        _cloudinary = new Cloudinary(account);

        // Force every SDK-generated URL to use HTTPS.
        _cloudinary.Api.Secure = true;

        _logger = logger;
    }

    public async Task<MediaUploadResult> UploadAsync(
        Stream fileStream,
        string fileName,
        string mimeType,
        string folderName)
    {
        // Rejects anything not explicitly allowed (e.g. .exe renamed to .jpg).
        if (!AllowedMimeTypes.TryGetValue(mimeType, out bool isDocument))
            throw new AppException(
                $"File type '{mimeType}' is not allowed. " +
                $"Accepted types: {string.Join(", ", AllowedMimeTypes.Keys)}", HttpStatusCode.BadRequest);

        if (fileStream.CanSeek)
        {
            long maxSize = isDocument ? MaxDocumentSizeBytes : MaxImageSizeBytes;
            string label = isDocument ? "Documents" : "Images";

            if (fileStream.Length == 0)
                throw new AppException("File is empty.", HttpStatusCode.BadRequest);

            if (fileStream.Length > maxSize)
                throw new AppException(
                    $"{label} must be under {maxSize / (1024 * 1024)} MB. " +
                    $"Your file is {fileStream.Length / (1024.0 * 1024.0):F1} MB.", HttpStatusCode.BadRequest);
        }
        // Documents use ResourceType.Raw — Cloudinary stores them as-is.
        RawUploadParams uploadParams = isDocument
            ? BuildDocumentParams(fileStream, fileName, folderName)
            : BuildImageParams(fileStream, fileName, folderName);

        _logger.LogInformation(
            "Uploading {FileName} ({MimeType}) to Cloudinary folder {Folder}",
            fileName, mimeType, folderName);

        var uploadResult = await _cloudinary.UploadAsync(uploadParams);

        // Cloudinary returns errors in-band (not as exceptions).
        // Always check .Error after every upload.
        if (uploadResult.Error != null)
        {
            _logger.LogError(
                "Cloudinary upload failed for {FileName}: {Error}",
                fileName, uploadResult.Error.Message);

            throw new AppException($"Upload failed: {uploadResult.Error.Message}", HttpStatusCode.InternalServerError);
        }

        _logger.LogInformation(
            "Upload succeeded. PublicId={PublicId}", uploadResult.PublicId);

        // Return both URL and PublicId 
        // IMPORTANT: Store PublicId in your database alongside the URL.
        // PublicId is needed for deletion — never re-parse the URL.
        return new MediaUploadResult(
            Url:      uploadResult.SecureUrl.ToString(),
            PublicId: uploadResult.PublicId
        );
    }

    // ── DeleteAsync ──────────────────────────────────────────────────────

    public async Task<bool> DeleteAsync(string publicId, string mimeType)
    {
        if (string.IsNullOrWhiteSpace(publicId))
            return true;

        // Cloudinary requires the resource type to match at deletion time.
        AllowedMimeTypes.TryGetValue(mimeType, out bool isDocument);
        var resourceType = isDocument ? ResourceType.Raw : ResourceType.Image;

        var deleteParams = new DeletionParams(publicId)
        {
            ResourceType = resourceType
        };

        _logger.LogInformation(
            "Deleting Cloudinary asset. PublicId={PublicId}, ResourceType={ResourceType}",
            publicId, resourceType);

        var result = await _cloudinary.DestroyAsync(deleteParams);

        // Cloudinary returns "ok" on success and "not found" if already gone.
        // Both are treated as success — idempotent delete is the right behaviour.
        bool success = result.Result == "ok" || result.Result == "not found";

        if (!success)
            _logger.LogWarning(
                "Cloudinary delete returned unexpected result '{Result}' for PublicId={PublicId}",
                result.Result, publicId);

        return success;
    }

    /// Image upload params — includes auto quality and format transformations.
    private static ImageUploadParams BuildImageParams(
        Stream fileStream, string fileName, string folderName) =>
        new()
        {
            File   = new FileDescription(fileName, fileStream),
            Folder = $"CanteenSystem/{folderName}",
            Transformation = new Transformation()
                .Quality("auto")
                .FetchFormat("auto")
        };

    /// Document (PDF) upload params — stored as-is, no transformation.
    /// ResourceType.Raw tells Cloudinary not to treat it as a renderable image.
    private static RawUploadParams BuildDocumentParams(
        Stream fileStream, string fileName, string folderName) =>
        new()
        {
            File         = new FileDescription(fileName, fileStream),
            Folder       = $"CanteenSystem/{folderName}",
        };
}