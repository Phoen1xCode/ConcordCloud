namespace ConcordCloud.Core.Interfaces;
public interface IFileStorageService
{
    Task<(bool Success, string Path, string Message)> SaveFileAsync(Stream fileStream, string fileName);
    Task<(bool Success, Stream? FileStream, string Message)> GetFileAsync(string filePath);
    Task<bool> DeleteFileAsync(string filePath);
}
