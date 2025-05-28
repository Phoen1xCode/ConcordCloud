using ConcordCloud.Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace ConcordCloud.Infrastructure.FileStorage;

public class LocalFileStorage : IFileStorageService
{
    private readonly string _storageBasePath;

    public LocalFileStorage(IConfiguration configuration)
    {
        _storageBasePath = configuration["FileStorage:LocalPath"] ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FileStorage");

        if (!Directory.Exists(_storageBasePath))
        {
            Directory.CreateDirectory(_storageBasePath);
        }
    }

    public async Task<(bool Success, string Path, string Message)> SaveFileAsync(Stream fileStream, string fileName)
    {
        try
        {

            string originalFileName = Path.GetFileName(fileName);
            string extension = Path.GetExtension(originalFileName);

            string uniqueStorageFileName = $"{Guid.NewGuid()}{extension}";
            string filePath = Path.Combine(_storageBasePath, uniqueStorageFileName);

            using (var fileWriter = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(fileWriter);
            }

            return (true, uniqueStorageFileName, "File saved successfully.");
        }
        catch (Exception ex)
        {
            return (false, string.Empty, $"File save failed: {ex.Message}");
        }
    }

    public Task<(bool Success, Stream? FileStream, string Message)> GetFileAsync(string filePath)
    {
        try
        {
            string fullPath = Path.Combine(_storageBasePath, filePath);

            if (!File.Exists(fullPath))
            {
                return Task.FromResult<(bool Success, Stream? FileStream, string Message)>((false, null, "File does not exist."));             }

            var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
            return Task.FromResult<(bool Success, Stream? FileStream, string Message)>((true, fileStream, "File retrieval successfully."));
        }
        catch (Exception ex)
        {
            return Task.FromResult<(bool Success, Stream? FileStream, string Message)>((false, null, $"File retrieval failed: {ex.Message}"));
        }
    }

    public Task<bool> DeleteFileAsync(string fileIdentifier)
    {
        try
        {
            string sanitizedIdentifier = Path.GetFileName(fileIdentifier);
            if (sanitizedIdentifier != fileIdentifier)
            {
                return Task.FromResult(false);
            }
            string fullPath = Path.Combine(_storageBasePath, sanitizedIdentifier);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                return Task.FromResult(true);
            }
            
            return Task.FromResult(false);
        }
        catch (Exception)
        {
            return Task.FromResult(false);
        }
    }
}