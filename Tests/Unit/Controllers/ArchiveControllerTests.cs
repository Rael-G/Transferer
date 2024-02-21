using Api.Business;
using Api.Controllers;
using Api.Models;
using Api.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Shouldly;
using System.Security.Claims;
using Tests._Builder;

namespace Tests.Unit.Controllers
{
    public class ArchiveControllerTests
    {
        private readonly Mock<IArchiveService> _business;
        private readonly ArchivesController _controller;

        public ArchiveControllerTests()
        {
            _business = new Mock<IArchiveService>();
            _controller = new ArchivesController(_business.Object);
        }

        [Fact]
        public async Task List_ReturnsOkWithArchives()
        {
            var userId = Guid.NewGuid().ToString();
            var archives = ArchiveBuilder.BuildArchives(10);
            _business.Setup(b => b.GetUserIdFromClaims(It.IsAny<ClaimsPrincipal>())).Returns(userId);
            _business.Setup(b => b.GetAllAsync(userId)).ReturnsAsync(archives);

            var result = await _controller.List();

            result.ShouldBeOfType<OkObjectResult>()
                .Value.ShouldBeOfType<List<ArchiveViewModel>>()
                .ShouldNotBeEmpty();

            _business.Verify(b => b.GetUserIdFromClaims(It.IsAny<ClaimsPrincipal>()), Times.Once);
            _business.Verify(b => b.GetAllAsync(userId), Times.Once);
        }

        [Fact]
        public async Task ListAll_ReturnsOkWithArchives()
        {
            var archives = ArchiveBuilder.BuildArchives(10);
            _business.Setup(b => b.GetAllAsync()).ReturnsAsync(archives);

            var result = await _controller.ListAll();

            result.ShouldBeOfType<OkObjectResult>()
                .Value.ShouldBeOfType<List<ArchiveViewModel>>()
                .ShouldNotBeEmpty();

            _business.Verify(b => b.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task ListAll_WhenNoArchives_ReturnsOkWithEmptyList()
        {
            var emptyArchives = new List<Archive>();
            _business.Setup(b => b.GetAllAsync()).ReturnsAsync(emptyArchives);

            var result = await _controller.ListAll();

            VerifyListAllResult(result, emptyArchives);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("        ")]
        public async Task Search_WhenNameIsNullOrWhiteSpace_ReturnsBadRequest(string names)
        {
            IActionResult result = await _controller.Search(names);

            _business.Verify(b => b.GetByNameAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);

            result.ShouldBeAssignableTo<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Search_WhenNameIsValid_ReturnsOkWithArchives()
        {
            var archives = ArchiveBuilder.BuildArchives(10);
            var names = "picture";
            _business.Setup(b => b.GetByNameAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(archives);

            IActionResult result = await _controller.Search(names);

            VerifySearchResult(result, archives);
        }

        [Fact]
        public async Task Download_WhenArchiveIsNotInRepository_ReturnsNotFound()
        {
            Guid id = Guid.NewGuid();

            _business.Setup(b => b.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<string>())).ReturnsAsync(() => null);

            var result = await _controller.Download(id);

            _business.Verify(b => b.GetByIdAsync(id, It.IsAny<string>()), Times.Once);
            result.ShouldBeAssignableTo<NotFoundResult>();
        }

        [Fact]
        public async Task Download_WhenStreamIsNotInStorage_ReturnsNotFound()
        {
            Archive archive = new ArchiveBuilder().Build();

            _business.Setup(b => b.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<string>())).ReturnsAsync(archive);

            var result = await _controller.Download(archive.Id);

            _business.Verify(b => b.GetByIdAsync(archive.Id, It.IsAny<string>()), Times.Once);

            result.ShouldBeAssignableTo<NotFoundObjectResult>();
        }

        [Fact]
        public async Task Download_WhenArchiveIsInRepositoryAndStreamIsInStorage_ReturnsFileStream()
        {
            Archive archive = new ArchiveBuilder().Build();
            Stream stream = new MemoryStream();

            _business.Setup(b => b.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<string>()))
                    .ReturnsAsync(archive);
            _business.Setup(b => b.DownloadAsync(archive))
                    .ReturnsAsync(stream);

            IActionResult result = await _controller.Download(archive.Id);

            VerifyDownloadResult(result, archive, stream);
        }

        [Fact]
        public async Task DownloadZip_WhenArchivesAreNotInRepository_ReturnsNotFoundWithIds()
        {
            Guid[] idArray = new Guid[] { Guid.NewGuid() };
            List<Archive> archives = new();
            var missing = "arquivo1; arquivo2;";

            _business.Setup(b => b.GetByIdsAsync(It.IsAny<Guid[]>(), It.IsAny<string>()))
                    .ReturnsAsync((archives, missing));

            IActionResult result = await _controller.DownloadZip(idArray);

            VerifyDownloadZipNotFoundResult(result, missing);
        }

        [Fact]
        public async Task DownloadZip_WhenStreamsAreNotInStorage_ReturnsNotFoundWithIds()
        {
            var id = new Guid[] { Guid.NewGuid() };
            var bytes = Array.Empty<byte>();
            var msg = "archive1; archive2";

            List<Archive> archives = ArchiveBuilder.BuildArchives(1);

            _business.Setup(b => b.GetByIdsAsync(It.IsAny<Guid[]>(), It.IsAny<string>()))
                    .ReturnsAsync((archives, ""));

            _business.Setup(b => b.DownloadMultipleAsync(It.IsAny<List<Archive>>()))
                    .ReturnsAsync((bytes, msg));

            IActionResult result = await _controller.DownloadZip(id);

            VerifyDownloadZipNotFoundResult(result, msg);
        }

        [Fact]
        public async Task DownloadZip_WhenArchivesAreInRepositoryAndStreamsAreInStorage_ReturnsFileStreamResult()
        {
            Guid[] ids = new Guid[] { Guid.NewGuid() };
            List<Archive> archives = ArchiveBuilder.BuildArchives(4);
            byte[] downloadZip = new byte[] { 255, 0, 255, 0, 255 };

            _business.Setup(b => b.GetByIdsAsync(It.IsAny<Guid[]>(), It.IsAny<string>()))
                    .ReturnsAsync((archives, ""));

            _business.Setup(b => b.DownloadMultipleAsync(It.IsAny<List<Archive>>()))
                    .ReturnsAsync((downloadZip, ""));

            IActionResult result = await _controller.DownloadZip(ids);

            VerifyDownloadZipFileStreamResult(result, downloadZip);
        }

        [Fact]
        public async Task Upload_WhenFilesAreNull_ReturnsBadRequest()
        {
            IEnumerable<IFormFile>? files = null;

            var result = await _controller.Upload(files);

            result.ShouldBeAssignableTo<BadRequestResult>();
        }

        [Fact]
        public async Task Upload_WhenFilesProvided_ReturnsOkWithArchives()
        {
            var files = new List<IFormFile>()
            {
                new FormFile(new MemoryStream(new byte[]{ }), 0, 0, "", "")
            };
            var archives = ArchiveBuilder.BuildArchives(6);

            _business.Setup(b => b.UploadAsync(It.IsAny<IEnumerable<IFormFile>>(), It.IsAny<string>()))
                    .ReturnsAsync(archives);

            IActionResult result = await _controller.Upload(files);

            VerifyUploadOkResult(result, archives);
        }

        [Fact]
        public async Task Delete_WhenArchiveIsNotInRepository_ReturnsNotFound()
        {
            Guid id = Guid.NewGuid();
            Archive? nullArchive = null;

            _business.Setup(b => b.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<string>()))
                .ReturnsAsync(nullArchive);

            var result = await _controller.Delete(id);

            result.ShouldBeAssignableTo<NotFoundResult>();
        }

        [Fact]
        public async Task Delete_WhenArchiveIsInRepository_ReturnsNoContent()
        {
            Guid id = Guid.NewGuid();
            Archive? archive = new ArchiveBuilder().Build();

            _business.Setup(b => b.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<string>()))
                .ReturnsAsync(archive);

            var result = await _controller.Delete(id);

            result.ShouldBeAssignableTo<NoContentResult>();
        }

        //Auxiliary methods start here

        private void VerifyListAllResult(IActionResult result, List<Archive> archives)
        {
            // Verify that the GetAllAsync method of the business was called once
            _business.Verify(b => b.GetAllAsync(), Times.Once);

            var resultValue = result.ShouldNotBeNull()
                .ShouldBeAssignableTo<OkObjectResult>()
                .ShouldNotBeNull()
                .Value.ShouldNotBeNull()
                .ShouldBeAssignableTo<List<ArchiveViewModel>>()
                .ShouldNotBeNull();

            if (archives.Any())
            {
                // If there are archives, assert that the FileName of the first result matches the first archive's FileName
                resultValue
                    .ShouldNotBeNull()
                    .FirstOrDefault()
                    .ShouldNotBeNull()
                    .FileName.ShouldBe(archives.FirstOrDefault()?.FileName);

                // Assert that the count of results matches the count of archives
                resultValue.Count.ShouldBe(archives.Count);
            }
            else
            {
                // If there are no archives, assert that the result is empty
                resultValue.ShouldBeEmpty();
            }
        }

        private void VerifySearchResult(IActionResult result, List<Archive> archives)
        {
            // Verify that the GetByNameAsync method of the business was called once
            _business.Verify(b => b.GetByNameAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            var objResult = result as OkObjectResult;

            var resultValue = result.ShouldNotBeNull()
                .ShouldBeAssignableTo<OkObjectResult>()
                .ShouldNotBeNull()
                .Value.ShouldNotBeNull()
                .ShouldBeAssignableTo<List<ArchiveViewModel>>()
                .ShouldNotBeNull();

            // If there are archives, assert that the FileName of the first result matches the first archive's FileName
            resultValue.FirstOrDefault()
                .ShouldNotBeNull()
                .FileName.ShouldBe(
                    archives.FirstOrDefault().ShouldNotBeNull()
                    .FileName);

            // Assert that the count of results matches the count of archives
            resultValue.Count.ShouldBe(archives.Count);
        }

        private void VerifyDownloadResult(IActionResult result, Archive archive, Stream expectedStream)
        {
            // Verify that GetByIdAsync and DownloadAsync methods were called once each
            _business.Verify(b => b.GetByIdAsync(archive.Id, It.IsAny<string>()), Times.Once);
            _business.Verify(b => b.DownloadAsync(archive), Times.Once);

            // Assert that the result is not null, is of type FileStreamResult, and its FileStream property matches the expected stream
            result.ShouldNotBeNull()
                  .ShouldBeOfType<FileStreamResult>()
                  .FileStream.ShouldBe(expectedStream);
        }

        private void VerifyDownloadZipNotFoundResult(IActionResult result, string expectedMissing)
        {
            // Verify that GetByIdsAsync method was called once
            _business.Verify(b => b.GetByIdsAsync(It.IsAny<Guid[]>(), It.IsAny<string>()), Times.Once);

            // Assert that the result is of type NotFoundObjectResult
            var objResult = result.ShouldNotBeNull()
                .ShouldBeOfType<NotFoundObjectResult>();

            // Assert that the value of NotFoundObjectResult is not null, is assignable to string, and contains the expected missing message
            var resultMsg = objResult.Value.ShouldNotBeNull()
                .ShouldBeAssignableTo<string>();

            Assert.Contains(expectedMissing, resultMsg);
        }

        private void VerifyDownloadZipFileStreamResult(IActionResult result, byte[] expectedFileContents)
        {
            // Verify that GetByIdsAsync and DownloadMultipleAsync methods were called once each
            _business.Verify(b => b.GetByIdsAsync(It.IsAny<Guid[]>(), It.IsAny<string>()), Times.Once);
            _business.Verify(b => b.DownloadMultipleAsync(It.IsAny<List<Archive>>()), Times.Once);

            // Assert that the result is not null, is of type FileContentResult, and its FileContents property matches the expected file contents
            result.ShouldNotBeNull()
                  .ShouldBeOfType<FileContentResult>()
                  .FileContents.ShouldBe(expectedFileContents);
        }

        private void VerifyUploadOkResult(IActionResult result, List<Archive> expectedArchives)
        {
            // Verify that UploadAsync method was called once
            _business.Verify(b => b.UploadAsync(It.IsAny<IEnumerable<IFormFile>>(), It.IsAny<string>()), Times.Once);

            // Assert that the result is not null, is of type OkObjectResult, its Value property is not null, is assignable to List<ArchiveViewModel>, and is not null
            var objResult = result.ShouldNotBeNull()
                                .ShouldBeOfType<OkObjectResult>()
                                .Value.ShouldNotBeNull()
                                .ShouldBeAssignableTo<List<ArchiveViewModel>>()
                                .ShouldNotBeNull();

            // Assert specific properties of the result value, such as count and file names
            objResult.Count.ShouldBe(expectedArchives.Count);
            objResult.FirstOrDefault()?.FileName.ShouldBe(expectedArchives.FirstOrDefault()?.FileName);
        }
    }
}
