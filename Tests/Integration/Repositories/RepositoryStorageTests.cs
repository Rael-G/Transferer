using Api.Controllers;
using Api.Data.Contexts;
using Api.Data.Interfaces;
using Api.Data.Repositories;
using Api.Extensions;
using Api.Models;
using Api.Models.ViewModels;
using Bogus;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Net.Mime;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Tests.Integration.Repositories
{
    public class RepositoryStorageTests
    {
        private readonly LocalFileStorage _fileStorage;
        private readonly ArchivesController _controller;
        private readonly IArchiveRepository _archiveRepository;
        private readonly Mock<UserManager<User>> _mockUserManager;

        public RepositoryStorageTests()
        {
            var context = new TransfererDbContext(new DbContextOptionsBuilder<TransfererDbContext>()
                .UseInMemoryDatabase(databaseName: "InMemoryTestDatabase")
                .Options);
            var tempPath = Path.Combine(Path.GetTempPath(), "tests");

            _fileStorage = new LocalFileStorage(tempPath);
            _archiveRepository = new ArchiveRepository(context);
            _mockUserManager = new Mock<UserManager<User>>(
                Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);
            _controller = new ArchivesController(_archiveRepository, _fileStorage, _mockUserManager.Object);
        }

        [Fact]
        public async Task UploadAndDownload_FileExists_ReturnsFileContent()
        {
            Guid archiveId;
            string userId = Guid.NewGuid().ToString();
            User user = new User() { Id = userId, Archives = new() };
            string fileName = "testfile.txt";
            string fileContent = "Hello, World!";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));
            var files = new List<IFormFile>
            {
                new FormFile(stream, 0, stream.Length, "file", fileName)
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "text/plain"
                }
            };


            SetupClaim(userId);
            _mockUserManager.Setup(u => u.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);

            var uploadResponse = await _controller.Upload(files);
            var uploadedArchive = (Assert.IsType<OkObjectResult>(uploadResponse).Value as List<ArchiveViewModel>).First();

            archiveId = Guid.Parse(uploadedArchive.Id);

            var downloadResponse = await _controller.Download(archiveId);
            var fileResult = Assert.IsType<FileStreamResult>(downloadResponse);

            using var downloadStream = new MemoryStream();
            fileResult.FileStream.CopyTo(downloadStream);

            var downloadedBytes = downloadStream.ToArray();
            var downloadedContent = Encoding.UTF8.GetString(downloadedBytes);

            Assert.Equal(fileContent, downloadedContent);
        }

        //Auxiliary

        public void SetupClaim(string userId)
        {
            var mockClaimsPrincipal = new Mock<ClaimsPrincipal>();
            var mockHttpContext = new Mock<HttpContext>();
            mockClaimsPrincipal.Setup(c => c.Claims).Returns(new List<Claim>
            {
                new Claim("UserId", userId)
            });

            mockHttpContext.Setup(h => h.User).Returns(mockClaimsPrincipal.Object);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = mockHttpContext.Object,
            };
        }
    }
}
