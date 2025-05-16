using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ConcordCloud.Core.DTOs;
using ConcordCloud.Core.Entities;
using ConcordCloud.Core.Interfaces;
using ConcordCloud.Core.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace ConcordCloud.Tests.Services
{
    public class FileServiceTests
    {
        private readonly Mock<IAppDbContext> _mockDbContext;
        private readonly Mock<IFileStorageService> _mockFileStorage;
        private readonly FileService _fileService;

        public FileServiceTests()
        {
            _mockDbContext = new Mock<IAppDbContext>();
            _mockFileStorage = new Mock<IFileStorageService>();
            _fileService = new FileService(_mockDbContext.Object, _mockFileStorage.Object);
        }

        [Fact]
        public async Task UploadFile_WithValidData_ShouldSucceed()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var fileName = "test.txt";
            var contentType = "text/plain";
            var fileSize = 1024L;
            var stream = new MemoryStream();

            var user = new User { Id = userId };
            var mockUserSet = new Mock<DbSet<User>>();
            mockUserSet.Setup(x => x.FindAsync(userId))
                .ReturnsAsync(user);

            _mockDbContext.Setup(x => x.Users)
                .Returns(mockUserSet.Object);

            _mockFileStorage.Setup(x => x.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>()))
                .ReturnsAsync((true, "storage/path", "文件保存成功"));

            // Act
            var result = await _fileService.UploadFileAsync(userId, fileName, contentType, fileSize, stream);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.File);
            Assert.Equal(fileName, result.File.FileName);
        }

        [Fact]
        public async Task DeleteFile_WithValidOwnership_ShouldSucceed()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var fileId = Guid.NewGuid();
            var file = new UserFile
            {
                Id = fileId,
                OwnerId = userId,
                StoragePath = "storage/path"
            };

            var mockFileSet = new Mock<DbSet<UserFile>>();
            mockFileSet.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<UserFile, bool>>>(), default))
                .ReturnsAsync(file);

            _mockDbContext.Setup(x => x.Files)
                .Returns(mockFileSet.Object);

            _mockFileStorage.Setup(x => x.DeleteFileAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            // Act
            var result = await _fileService.DeleteFileAsync(userId, fileId);

            // Assert
            Assert.True(result.Success);
            Assert.Contains("删除成功", result.Message);
        }
    }
} 