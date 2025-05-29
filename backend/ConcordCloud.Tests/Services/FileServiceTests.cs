using System;
using System.Threading.Tasks;
using ConcordCloud.Core.DTOs;
using ConcordCloud.Core.Entities;
using ConcordCloud.Core.Interfaces;
using ConcordCloud.Core.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Moq;

namespace ConcordCloud.Tests.Services
{
    public class FileServiceTests
    {
        private class TestDbContext : DbContext, IAppDbContext
        {
            public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
            public DbSet<User> Users { get; set; }
            public DbSet<UserFile> Files { get; set; }
            public DbSet<ShareFile> ShareFiles { get; set; }
            public DbSet<Admin> Admins { get; set; }
            public override Task<int> SaveChangesAsync(System.Threading.CancellationToken cancellationToken = default) => base.SaveChangesAsync(cancellationToken);
        }

        private static FileService CreateFileService(DbContextOptions<TestDbContext> options)
        {
            var dbContext = new TestDbContext(options);
            var fileStorageService = new Mock<IFileStorageService>().Object; // 模拟文件存储服务
            return new FileService(dbContext, fileStorageService);
        }

        [Fact]
        public async Task UploadFileAsync_WithValidData_ShouldSucceed()
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var fileService = CreateFileService(options);

            var userId = Guid.NewGuid();
            var uploadDto = new FileUploadDto
            {
                FileName = "test.txt",
                FileSize = 1024,
                FileType = "text/plain",
                FilePath = "/uploads/test.txt"
            };

            var result = await fileService.UploadFileAsync(userId, uploadDto);

            Assert.True(result.Success);
            Assert.Equal("文件上传成功", result.Message);
            Assert.NotNull(result.File);
            Assert.Equal(uploadDto.FileName, result.File.FileName);
            Assert.Equal(uploadDto.FileSize, result.File.FileSize);
            Assert.Equal(uploadDto.FileType, result.File.FileType);
            Assert.Equal(uploadDto.FilePath, result.File.FilePath);
            Assert.Equal(userId, result.File.UserId);
        }

        [Fact]
        public async Task GetUserFilesAsync_WithValidUserId_ShouldReturnFiles()
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var fileService = CreateFileService(options);

            var userId = Guid.NewGuid();
            var file = new UserFile
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                FileName = "test.txt",
                FileSize = 1024,
                FileType = "text/plain",
                FilePath = "/uploads/test.txt",
                UploadTime = DateTime.UtcNow
            };
            using (var context = new TestDbContext(options))
            {
                context.Files.Add(file);
                await context.SaveChangesAsync();
            }

            var result = await fileService.GetUserFilesAsync(userId);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(file.FileName, result[0].FileName);
        }

        [Fact]
        public async Task GetFileByIdAsync_WithValidId_ShouldReturnFile()
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var fileService = CreateFileService(options);

            var userId = Guid.NewGuid();
            var file = new UserFile
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                FileName = "test.txt",
                FileSize = 1024,
                FileType = "text/plain",
                FilePath = "/uploads/test.txt",
                UploadTime = DateTime.UtcNow
            };
            using (var context = new TestDbContext(options))
            {
                context.Files.Add(file);
                await context.SaveChangesAsync();
            }

            var result = await fileService.GetFileByIdAsync(file.Id);

            Assert.NotNull(result);
            Assert.Equal(file.FileName, result.FileName);
        }

        [Fact]
        public async Task GetFileByIdAsync_WithInvalidId_ShouldReturnNull()
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var fileService = CreateFileService(options);

            var result = await fileService.GetFileByIdAsync(Guid.NewGuid());

            Assert.Null(result);
        }
    }
} 