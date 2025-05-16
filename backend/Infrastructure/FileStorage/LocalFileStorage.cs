using System;
using System.IO;
using System.Threading.Tasks;
using ConcordCloud.Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace ConcordCloud.Infrastructure.FileStorage
{
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
                // 清理原始文件名，获取扩展名
                string originalFileNameClean = Path.GetFileName(fileName); // 确保只取文件名部分
                string extension = Path.GetExtension(originalFileNameClean); // 获取扩展名，例如 ".txt"

                // 生成基于 GUID 和扩展名的唯一文件名
                string uniqueStorageFileName = $"{Guid.NewGuid()}{extension}";
                string filePath = Path.Combine(_storageBasePath, uniqueStorageFileName);

                using (var fileWriter = new FileStream(filePath, FileMode.Create))
                {
                    await fileStream.CopyToAsync(fileWriter);
                }

                // 返回成功状态和生成的唯一存储文件名 (不含原始文件名)
                return (true, uniqueStorageFileName, "文件保存成功");
            }
            catch (Exception ex)
            {
                return (false, string.Empty, $"文件保存失败: {ex.Message}");
            }
        }

        public async Task<(bool Success, Stream FileStream, string Message)> GetFileAsync(string filePath)
        {
            try
            {
                string fullPath = Path.Combine(_storageBasePath, filePath);
                
                if (!File.Exists(fullPath))
                {
                    return (false, null, "文件不存在");
                }

                var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
                return (true, fileStream, "文件获取成功");
            }
            catch (Exception ex)
            {
                return (false, null, $"文件获取失败: {ex.Message}");
            }
        }

        public Task<bool> DeleteFileAsync(string filePath)
        {
            try
            {
                string fullPath = Path.Combine(_storageBasePath, filePath);
                
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return Task.FromResult(true);
                }
                
                return Task.FromResult(false);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }
    }
} 