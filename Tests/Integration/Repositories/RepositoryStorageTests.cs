using Api.Business;
using Api.Business.Implementation;
using Api.Controllers;
using Api.Data.Contexts;
using Api.Data.Repositories;
using Api.Models;
using Api.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;
using System.Text;

namespace Tests.Integration.Repositories
{
    public class RepositoryStorageTests
    {
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly ArchivesController _controller;
        private readonly IArchiveBusiness _archiveBusiness;

        public RepositoryStorageTests()
        {
            var context = new TransfererDbContext(new DbContextOptionsBuilder<TransfererDbContext>()
                .UseInMemoryDatabase(databaseName: "InMemoryTestDatabase")
                .Options);
            var tempPath = Path.Combine(Path.GetTempPath(), "tests");
            var fileStorage = new LocalFileStorage(tempPath);
            var archiveRepository = new ArchiveRepository(context);

            _userManagerMock = new Mock<UserManager<User>>(
                Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);
            var mockRoleStore = new Mock<IRoleStore<IdentityRole>>();
            var mockRoleManager = new Mock<RoleManager<IdentityRole>>(mockRoleStore.Object, null, null, null, null);

            var userRepository = new UserRepository(_userManagerMock.Object, mockRoleManager.Object);

            _archiveBusiness = new ArchiveBusiness(archiveRepository, fileStorage, userRepository);
            _controller = new ArchivesController(_archiveBusiness);
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
            _userManagerMock.Setup(u => u.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);

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
