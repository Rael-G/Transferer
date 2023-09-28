using Api.Controllers;
using Api.Data.Interfaces;
using Api.Data.Repositories;
using Api.Models;
using Bogus;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Shouldly;
using System.IO;
using System.IO.Compression;
using Tests._Builder;

namespace Tests.Unit.Controllers
{
    public class ArchiveControllerTests
    {
        private readonly Mock<IArchiveRepository> _mockArchiveRepository;
        private readonly Mock<IFileStorage> _mockStorage;
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly ArchivesController _controller;

        public ArchiveControllerTests()
        {
            _mockArchiveRepository = new Mock<IArchiveRepository>();
            _mockStorage = new Mock<IFileStorage>();
            _mockUserRepository = new Mock<IUserRepository>();
            _controller = new ArchivesController(_mockArchiveRepository.Object, _mockStorage.Object, _mockUserRepository.Object);
        }

        [Fact]
        public async void ListAll_ReturnsOkWithArchives()
        {
            var archives = ArchiveBuilder.BuildArchives(10);

            _mockArchiveRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(archives);

            var result = await _controller.ListAll();

            _mockArchiveRepository.Verify(r => r.GetAllAsync(), Times.Once);

            result.ShouldBeAssignableTo<OkObjectResult>();
            if (result is OkObjectResult objResult)
            {
                objResult.Value.ShouldNotBeNull();
                objResult.Value.ShouldBeOfType(archives.GetType());
                if (objResult.Value is List<Archive>)
                {
                    objResult.Value.ShouldBe(archives);
                }
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async void Search_WhenNameIsNullOrWhiteSpace_ReturnsBadRequest(string names)
        {
            IActionResult result = await _controller.Search(names);

            _mockArchiveRepository.Verify(r => r.GetByNameAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);

            result.ShouldBeAssignableTo<BadRequestObjectResult>();
        }

        [Fact]
        public async void Search_WhenNameIsValid_ReturnsOkWithArchives()
        {
            List<Archive> archives = ArchiveBuilder.BuildArchives(10);
            var names = "picture";

            _mockArchiveRepository.Setup(r => r.GetByNameAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(archives);

            IActionResult result = await _controller.Search(names);

            _mockArchiveRepository.Verify(r => r.GetByNameAsync(names, It.IsAny<string>()), Times.Once);

            result.ShouldBeAssignableTo<OkObjectResult>();
            if (result is OkObjectResult objResult)
            {
                objResult.Value.ShouldNotBeNull();
                objResult.Value.ShouldBeOfType(archives.GetType());
                if (objResult.Value is List<Archive>)
                {
                    objResult.Value.ShouldBe(archives);
                }
            }
        }

        [Fact]
        public async void Download_WhenArchiveIsNotInRepository_ReturnsNotFound()
        {
            Guid id = Guid.NewGuid();
            Archive? notFoundArchive = null;

            _mockArchiveRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<string>())).ReturnsAsync(notFoundArchive);

            var result = await _controller.Download(id);

            _mockArchiveRepository.Verify(r => r.GetByIdAsync(id, It.IsAny<string>()), Times.Once);
            _mockStorage.Verify(s => s.GetByPath(It.IsAny<string>()), Times.Never);

            result.ShouldBeAssignableTo<NotFoundResult>();
        }

        [Fact]
        public async void Download_WhenStreamIsNotInStorage_ReturnsNotFound()
        {
            Guid id = Guid.NewGuid();
            Archive archive = new ArchiveBuilder().Build();
            Stream? notFoundStream = null;

            _mockArchiveRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<string>())).ReturnsAsync(archive);
            _mockStorage.Setup(s => s.GetByPath(It.IsAny<string>())).Returns(notFoundStream);

            var result = await _controller.Download(id);

            _mockArchiveRepository.Verify(r => r.GetByIdAsync(id, It.IsAny<string>()), Times.Once);
            _mockStorage.Verify(s => s.GetByPath(archive.Path), Times.Once);

            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        }

        [Fact]
        public async void Download_WhenArchiveIsInRepositoryAndStreamIsInStorage_ReturnsFileStream()
        {
            Guid id = Guid.NewGuid();
            Archive archive = new ArchiveBuilder().Build();
            Stream stream = new MemoryStream();

            _mockArchiveRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<string>())).ReturnsAsync(archive);
            _mockStorage.Setup(s => s.GetByPath(It.IsAny<string>())).Returns(stream);

            var result = await _controller.Download(id);

            _mockArchiveRepository.Verify(r => r.GetByIdAsync(id, It.IsAny<string>()), Times.Once);
            _mockStorage.Verify(s => s.GetByPath(archive.Path), Times.Once);

            result.ShouldBeAssignableTo<FileStreamResult>();
            if (result is FileStreamResult fileStreamResult)
            {
                fileStreamResult.FileStream.ShouldBe(stream);
                fileStreamResult.ContentType.ShouldBe(archive.ContentType);
                fileStreamResult.FileDownloadName.ShouldBe(archive.FileName);
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("1,2,3,4,S")]

        public async void DownloadZip_WhenIdsAreNotIntParseable_ReturnBadRequest(string id)
        {
            var result = await _controller.DownloadZip(id);

            _mockArchiveRepository.Verify(r => r.GetByIdsAsync(It.IsAny<Guid[]>(), It.IsAny<string>()), Times.Never);
            _mockStorage.Verify(s => s.GetByPath(It.IsAny<string>()), Times.Never);

            result.ShouldBeAssignableTo<BadRequestObjectResult>();
        }

        [Fact]
        public async void DownloadZip_WhenArchivesAreNotInRepository_ReturnsNotFound()
        {
            string idString = "1";
            Guid[] idArray = new Guid[] { Guid.NewGuid() };
            List<Archive> archives = new();
            List<int> notFoundIds = new() { 1 };

            _mockArchiveRepository.Setup(r => r.GetByIdsAsync(It.IsAny<Guid[]>(), It.IsAny<string>())).ReturnsAsync((archives));

            var result = await _controller.DownloadZip(idString);

            _mockArchiveRepository.Verify(r => r.GetByIdsAsync(idArray, It.IsAny<string>()), Times.Once);
            _mockStorage.Verify(s => s.GetByPath(It.IsAny<string>()), Times.Never);

            result.ShouldBeAssignableTo<NotFoundResult>();
        }

        [Fact]
        public async void DownloadZip_WhenStreamsAreNotInStorage_InternalServerError()
        {

            Stream? notFoundStream = null;
            List<Archive> archives = new() { new ArchiveBuilder().Build() };
            List<int> notFoundIds = new() { };

            _mockArchiveRepository.Setup(r => r.GetByIdsAsync(It.IsAny<Guid[]>(), It.IsAny<string>())).ReturnsAsync((archives));
            _mockStorage.Setup(s => s.GetByPath(It.IsAny<string>())).Returns(notFoundStream);

            var result = await _controller.DownloadZip("1");

            _mockArchiveRepository.Verify(r => r.GetByIdsAsync(It.IsAny<Guid[]>(), It.IsAny<string>()), Times.Once);
            _mockStorage.Verify(s => s.GetByPath(It.IsAny<string>()), Times.Once);

            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);

        }

        [Fact]
        public async void DownloadZip_WhenArchivesAreInRepositoryAndStreamsAreInStorage_ReturnsFileStreamResult()
        {
            string idString = "5, 6";
            Guid[] idArray = new Guid[] { Guid.NewGuid() };
            List<Archive> archives = new() { new ArchiveBuilder().SetId(5).SetPath("path").Build() };
            List<int> notFoundIds = new() { 6 };
            byte[] downloadZip = await CreateZipDataClone(archives);

            _mockArchiveRepository.Setup(r => r.GetByIdsAsync(It.IsAny<Guid[]>(), It.IsAny<string>())).ReturnsAsync((archives));
            _mockStorage.Setup(s => s.GetByPath(It.IsAny<string>())).Returns(new MemoryStream(new byte[255]));

            var result = await _controller.DownloadZip(idString);

            _mockArchiveRepository.Verify(r => r.GetByIdsAsync(idArray, It.IsAny<string>()), Times.Once);
            _mockStorage.Verify(s => s.GetByPath("path"), Times.Once);

            result.ShouldBeAssignableTo<FileContentResult>();
            if (result is FileContentResult fileStreamResult)
            {
                fileStreamResult.FileContents.ShouldBe(downloadZip);
                fileStreamResult.ContentType.ShouldBe("application/zip");
            }
        }

        [Fact]
        public async void Upload_WhenFilesAreNull_ReturnsBadRequest()
        {
            IEnumerable<IFormFile>? files = null;

            _mockArchiveRepository.Setup(r => r.SaveAsync(It.IsAny<Archive>())).
            ReturnsAsync((Archive archive) => archive);

            var result = await _controller.Upload(files);

            _mockArchiveRepository.Verify(r => r.GetByIdsAsync(It.IsAny<Guid[]>(), It.IsAny<string>()), Times.Never);
            _mockStorage.Verify(s => s.GetByPath(It.IsAny<string>()), Times.Never);

            result.ShouldBeAssignableTo<BadRequestResult>();

        }

        [Fact]
        public async void Upload_WhenFilesProvided_ReturnsOkWithArchives()
        {
            IEnumerable<IFormFile> files = new List<IFormFile>()
            {
                new FormFile(new MemoryStream(new byte[100]), 0, 100, "file1.txt", "file1.txt")
                {
                    Headers = new HeaderDictionary{{ "Content-Type", "text/plain" }}
                },
                new FormFile(new MemoryStream(new byte[150]), 0, 150, "file2.txt", "file2.txt")
                {
                    Headers = new HeaderDictionary{{ "Content-Type", "text/plain" }}
                }
            };
            var archives = new List<Archive>();
            foreach (IFormFile file in files)
            {
                archives.Add(new(file.FileName, file.ContentType, file.Length, "path", new User()));
            }

            _mockArchiveRepository.Setup(r => r.SaveAsync(It.IsAny<Archive>())).
                ReturnsAsync((Archive archive) => archive);
            _mockStorage.Setup(s => s.Store(It.IsAny<Stream>())).Returns("path");

            var result = await _controller.Upload(files);

            _mockArchiveRepository.Verify(r => r.SaveAsync(It.IsAny<Archive>()), Times.Exactly(2));
            _mockStorage.Verify(s => s.Store(It.IsAny<Stream>()), Times.Exactly(2));

            result.ShouldBeAssignableTo<OkObjectResult>();
            if (result is OkObjectResult objResult)
            {
                objResult.Value.ShouldNotBeNull();
                objResult.Value.ShouldBeOfType(archives.GetType());
                if (objResult.Value is IEnumerable<IFormFile>)
                {
                    objResult.Value.ShouldBe(archives);
                }
            }
        }

        [Fact]
        public async void Delete_WhenArchiveIsNotInRepository_ReturnsNotFound()
        {
            Guid id = Guid.NewGuid();
            Archive? archive = null;
            _mockArchiveRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<string>())).ReturnsAsync(archive);

            var result = await _controller.Delete(id);

            _mockArchiveRepository.Verify(r => r.GetByIdAsync(id, It.IsAny<string>()), Times.Once);
            _mockStorage.Verify(s => s.Delete(It.IsAny<string>()), Times.Never);

            result.ShouldBeAssignableTo<NotFoundResult>();
        }

        [Fact]
        public async void Delete_WhenArchiveIsInRepository_ReturnsNoContent()
        {
            Guid id = Guid.NewGuid();
            Archive? archive = new ArchiveBuilder().Build();
            _mockArchiveRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<string>())).ReturnsAsync(archive);

            var result = await _controller.Delete(id);

            _mockArchiveRepository.Verify(r => r.GetByIdAsync(id, It.IsAny<string>()), Times.Once);
            _mockStorage.Verify(s => s.Delete(archive.Path), Times.Once);

            result.ShouldBeAssignableTo<NoContentResult>();
        }

        //Auxiliary
        private static async Task<byte[]> CreateZipDataClone(List<Archive> archives)
        {
            using var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                foreach (var item in archives)
                {
                    var stream = new MemoryStream(new byte[255]);
                    if (stream == null)
                        continue;

                    var zipEntry = archive.CreateEntry(item.FileName, CompressionLevel.Optimal);

                    using var zipStream = zipEntry.Open();
                    await stream.CopyToAsync(zipStream);
                }
            }
            return memoryStream.ToArray();
        }
    }
}
