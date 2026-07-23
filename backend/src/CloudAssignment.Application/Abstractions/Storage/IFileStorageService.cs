namespace CloudAssignment.Application.Abstractions.Storage;

public interface IFileStorageService
{
    Task<StoredFileResult> UploadAsync(
        Stream content,
        FileUploadDescriptor descriptor,
        CancellationToken cancellationToken);

    Task<Uri> CreateDownloadUriAsync(
        string objectKey,
        TimeSpan lifetime,
        CancellationToken cancellationToken);

    Task DeleteAsync(string objectKey, CancellationToken cancellationToken);
}

public sealed record FileUploadDescriptor(
    string ObjectKey,
    string OriginalFileName,
    string ContentType,
    long SizeBytes);

public sealed record StoredFileResult(
    string Provider,
    string ContainerName,
    string ObjectKey,
    string ContentType,
    long SizeBytes,
    string? Sha256);
