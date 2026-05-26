namespace CanteenSystem.Application.Interfaces;

public interface IMediaService
{
    // returns Upload result containing both the public URL and the public ID needed for deletion
    Task<MediaUploadResult> UploadAsync(Stream fileStream, string fileName, string mimeType, string folderName);

    /// Deletes a previously uploaded file by its public ID.
    Task<bool> DeleteAsync(string publicId, string mimeType);
}
public record MediaUploadResult(string Url, string PublicId);