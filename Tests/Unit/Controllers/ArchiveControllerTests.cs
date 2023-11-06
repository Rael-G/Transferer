﻿using Api.Business;
using Api.Controllers;
using Api.Data.Interfaces;
using Api.Models;
using Api.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Shouldly;
using System.IO.Compression;
using System.Security.Claims;
using Tests._Builder;

namespace Tests.Unit.Controllers
{
    public class ArchiveControllerTests
    {
        private readonly Mock<IArchiveBusiness> _business;
        private readonly ArchivesController _controller;

        public ArchiveControllerTests()
        {
            _business = new Mock<IArchiveBusiness>();
            _controller = new ArchivesController(_business.Object);
        }

        [Fact]
        public async void List_ReturnsOkWithArchives()
        {
            var archives = ArchiveBuilder.BuildArchives(10);

            _business.Setup(r => r.GetAllAsync()).ReturnsAsync(archives);

            var result = await _controller.ListAll();

            _business.Verify(r => r.GetAllAsync(), Times.Once);

            result.ShouldBeAssignableTo<OkObjectResult>();
            if (result is OkObjectResult objResult)
            {
                objResult.Value.ShouldNotBeNull();
                objResult.Value.ShouldBeOfType(typeof(List<ArchiveViewModel>));
                if (objResult.Value is List<Archive>)
                {
                    objResult.Value.ShouldBe(archives);
                }
            }
        }

        //TODO:
        //ListAll tests

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async void Search_WhenNameIsNullOrWhiteSpace_ReturnsBadRequest(string names)
        {
            IActionResult result = await _controller.Search(names);

            _business.Verify(r => r.GetByNameAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);

            result.ShouldBeAssignableTo<BadRequestObjectResult>();
        }

        [Fact]
        public async void Search_WhenNameIsValid_ReturnsOkWithArchives()
        {
            List<Archive> archives = ArchiveBuilder.BuildArchives(10);
            var names = "picture";
            string userId = Guid.NewGuid().ToString();

            SetupClaim();

            _business.Setup(r => r.GetByNameAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(archives);

            IActionResult result = await _controller.Search(names);

            _business.Verify(r => r.GetByNameAsync(names, It.IsAny<string>()), Times.Once);

            result.ShouldBeAssignableTo<OkObjectResult>();
            if (result is OkObjectResult objResult)
            {
                objResult.Value.ShouldNotBeNull();
                objResult.Value.ShouldBeOfType(typeof(List<ArchiveViewModel>));
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
            string userId = Guid.NewGuid().ToString();

            SetupClaim();
            _business.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<string>())).ReturnsAsync(notFoundArchive);

            var result = await _controller.Download(id);

            _business.Verify(r => r.GetByIdAsync(id, It.IsAny<string>()), Times.Once);
            _mockStorage.Verify(s => s.GetByPath(It.IsAny<string>()), Times.Never);

            result.ShouldBeAssignableTo<NotFoundResult>();
        }

        [Fact]
        public async void Download_WhenStreamIsNotInStorage_ReturnsNotFound()
        {
            Guid id = Guid.NewGuid();
            Archive archive = new ArchiveBuilder().Build();
            Stream? notFoundStream = null;
            string userId = Guid.NewGuid().ToString();

            SetupClaim();
            _business.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<string>())).ReturnsAsync(archive);
            _mockStorage.Setup(s => s.GetByPath(It.IsAny<string>())).Returns(notFoundStream);

            var result = await _controller.Download(id);

            _business.Verify(r => r.GetByIdAsync(id, It.IsAny<string>()), Times.Once);
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
            string userId = Guid.NewGuid().ToString();

            SetupClaim();
            _business.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<string>())).ReturnsAsync(archive);
            _mockStorage.Setup(s => s.GetByPath(It.IsAny<string>())).Returns(stream);

            var result = await _controller.Download(id);

            _business.Verify(r => r.GetByIdAsync(id, It.IsAny<string>()), Times.Once);
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

            _business.Verify(r => r.GetByIdsAsync(It.IsAny<Guid[]>(), It.IsAny<string>()), Times.Never);
            _mockStorage.Verify(s => s.GetByPath(It.IsAny<string>()), Times.Never);

            result.ShouldBeAssignableTo<BadRequestObjectResult>();
        }

        [Fact]
        public async void DownloadZip_WhenArchivesAreNotInRepository_ReturnsNotFound()
        {
            string idString = Guid.NewGuid().ToString();
            Guid[] idArray = new Guid[] { Guid.Parse(idString) };
            List<Archive> archives = new();
            List<int> notFoundIds = new() { 1 };
            string userId = Guid.NewGuid().ToString();

            SetupClaim();
            _business.Setup(r => r.GetByIdsAsync(It.IsAny<Guid[]>(), It.IsAny<string>())).ReturnsAsync((archives));

            var result = await _controller.DownloadZip(idString);

            _business.Verify(r => r.GetByIdsAsync(idArray, It.IsAny<string>()), Times.Once);
            _mockStorage.Verify(s => s.GetByPath(It.IsAny<string>()), Times.Never);

            result.ShouldBeAssignableTo<NotFoundResult>();
        }

        [Fact]
        public async void DownloadZip_WhenStreamsAreNotInStorage_InternalServerError()
        {

            Stream? notFoundStream = null;
            List<Archive> archives = new() { new ArchiveBuilder().Build() };
            string userId = Guid.NewGuid().ToString();

            SetupClaim();
            _business.Setup(r => r.GetByIdsAsync(It.IsAny<Guid[]>(), It.IsAny<string>())).ReturnsAsync((archives));
            _mockStorage.Setup(s => s.GetByPath(It.IsAny<string>())).Returns(notFoundStream);

            var result = await _controller.DownloadZip(Guid.NewGuid().ToString());

            _business.Verify(r => r.GetByIdsAsync(It.IsAny<Guid[]>(), It.IsAny<string>()), Times.Once);
            _mockStorage.Verify(s => s.GetByPath(It.IsAny<string>()), Times.Once);

            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);

        }

        [Fact]
        public async void DownloadZip_WhenArchivesAreInRepositoryAndStreamsAreInStorage_ReturnsFileStreamResult()
        {
            string idString = Guid.NewGuid().ToString();
            Guid[] idArray = new Guid[] { Guid.Parse(idString) };
            List<Archive> archives = new() { new ArchiveBuilder().SetId(5).SetPath("path").Build() };
            List<int> notFoundIds = new() { 6 };
            byte[] downloadZip = await CreateZipDataClone(archives);
            string userId = Guid.NewGuid().ToString();

            SetupClaim();
            _business.Setup(r => r.GetByIdsAsync(It.IsAny<Guid[]>(), It.IsAny<string>())).ReturnsAsync((archives));
            _mockStorage.Setup(s => s.GetByPath(It.IsAny<string>())).Returns(new MemoryStream(new byte[255]));

            var result = await _controller.DownloadZip(idString);

            _business.Verify(r => r.GetByIdsAsync(idArray, It.IsAny<string>()), Times.Once);
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

            _business.Setup(r => r.SaveAsync(It.IsAny<Archive>())).
            ReturnsAsync((Archive archive) => archive);

            var result = await _controller.Upload(files);

            _business.Verify(r => r.GetByIdsAsync(It.IsAny<Guid[]>(), It.IsAny<string>()), Times.Never);
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
            string userId = Guid.NewGuid().ToString();
            var user = new User { Id  = userId, Archives = new() };
            SetupClaim();
            _mockUserManager.Setup(u => u.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);
            _business.Setup(r => r.SaveAsync(It.IsAny<Archive>())).
                ReturnsAsync((Archive archive) => archive);
            _mockStorage.Setup(s => s.Store(It.IsAny<Stream>())).Returns("path");

            var result = await _controller.Upload(files);

            _business.Verify(r => r.SaveAsync(It.IsAny<Archive>()), Times.Exactly(2));
            _mockStorage.Verify(s => s.Store(It.IsAny<Stream>()), Times.Exactly(2));

            result.ShouldBeAssignableTo<OkObjectResult>();
            if (result is OkObjectResult objResult)
            {
                objResult.Value.ShouldNotBeNull();
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
            string userId = Guid.NewGuid().ToString();
            var user = new User { Id = userId, Archives = new() };

            _mockUserManager.Setup(u => u.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);
            SetupClaim();
            _business.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<string>())).ReturnsAsync(archive);

            var result = await _controller.Delete(id);

            _business.Verify(r => r.GetByIdAsync(id, It.IsAny<string>()), Times.Once);
            _mockStorage.Verify(s => s.Delete(It.IsAny<string>()), Times.Never);

            result.ShouldBeAssignableTo<NotFoundResult>();
        }

        [Fact]
        public async void Delete_WhenArchiveIsInRepository_ReturnsNoContent()
        {
            Guid id = Guid.NewGuid();
            Archive? archive = new ArchiveBuilder().Build();
            string userId = id.ToString();
            var user = new User { Id = userId, Archives = new() };

            _mockUserManager.Setup(u => u.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);
            SetupClaim();
            _business.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<string>())).ReturnsAsync(archive);

            var result = await _controller.Delete(id);

            _business.Verify(r => r.GetByIdAsync(id, It.IsAny<string>()), Times.Once);
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

        public void SetupClaim()
        {

            var mockClaimsPrincipal = new Mock<ClaimsPrincipal>();
            var mockHttpContext = new Mock<HttpContext>();
            mockClaimsPrincipal.Setup(c => c.Claims).Returns(new List<Claim>
            {
                new Claim("UserId", "123")
            });

            mockHttpContext.Setup(h => h.User).Returns(mockClaimsPrincipal.Object);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = mockHttpContext.Object,
            };
        }
    }
}
