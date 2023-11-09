using Api.Business;
using Api.Controllers;
using Api.Models;
using Api.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Shouldly;
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

            _business.Setup(b => b.GetAllAsync()).ReturnsAsync(archives);

            var result = await _controller.ListAll();

            _business.Verify(b => b.GetAllAsync(), Times.Once);

            var objResult = result as OkObjectResult;
            var resultValue = objResult.Value as List<ArchiveViewModel>;

            resultValue.FirstOrDefault().FileName.ShouldBe(archives.FirstOrDefault().FileName);
            resultValue.Count.ShouldBe(archives.Count);
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

            _business.Verify(b => b.GetByNameAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);

            result.ShouldBeAssignableTo<BadRequestObjectResult>();
        }

        [Fact]
        public async void Search_WhenNameIsValid_ReturnsOkWithArchives()
        {
            List<Archive> archives = ArchiveBuilder.BuildArchives(10);
            var names = "picture";

            _business.Setup(b => b  .GetByNameAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(archives);

            IActionResult result = await _controller.Search(names);

            _business.Verify(b => b.GetByNameAsync(names, It.IsAny<string>()), Times.Once);

            var objResult = result as OkObjectResult;
            var resultValue = objResult.Value as List<ArchiveViewModel>;

            resultValue.FirstOrDefault().FileName.ShouldBe(archives.FirstOrDefault().FileName);
            resultValue.Count.ShouldBe(archives.Count);
        }

        [Fact]
        public async void Download_WhenArchiveIsNotInRepository_ReturnsNotFound()
        {
            Guid id = Guid.NewGuid();

            _business.Setup(b => b.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<string>())).ReturnsAsync(() => null);

            var result = await _controller.Download(id);

            _business.Verify(b => b.GetByIdAsync(id, It.IsAny<string>()), Times.Once);
            result.ShouldBeAssignableTo<NotFoundResult>();
        }

        [Fact]
        public async void Download_WhenStreamIsNotInStorage_ReturnsNotFound()
        {
            Guid id = Guid.NewGuid();
            Archive archive = new ArchiveBuilder().Build();

            _business.Setup(b => b.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<string>())).ReturnsAsync(archive);

            var result = await _controller.Download(id);

            _business.Verify(b => b.GetByIdAsync(id, It.IsAny<string>()), Times.Once);

            result.ShouldBeAssignableTo<NotFoundObjectResult>();
        }

        [Fact]
        public async void Download_WhenArchiveIsInRepositoryAndStreamIsInStorage_ReturnsFileStream()
        {
            Guid id = Guid.NewGuid();
            Archive archive = new ArchiveBuilder().Build();
            Stream stream = new MemoryStream();

            _business.Setup(b => b.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<string>()))
                .ReturnsAsync(archive);
            _business.Setup(b => b.DownloadAsync(archive))
                .ReturnsAsync(stream);

            var result = await _controller.Download(id);

            _business.Verify(b => b.GetByIdAsync(id, It.IsAny<string>()), Times.Once);
            _business.Verify(b => b.DownloadAsync(archive), Times.Once);

            var fileStreamResult = result as FileStreamResult;
            
            fileStreamResult.FileStream.ShouldBe(stream);
        }

        [Fact]
        public async void DownloadZip_WhenArchivesAreNotInRepository_ReturnsNotFoundWithIds()
        {
            Guid[] idArray = new Guid[] { Guid.NewGuid() };
            List<Archive> archives = new();
            var missing = "arquivo1; arquivo2;";

            _business.Setup(b => b.GetByIdsAsync(It.IsAny<Guid[]>(), It.IsAny<string>()))
                .ReturnsAsync((archives, missing));

            var result = await _controller.DownloadZip(idArray);

            var resultObj = result as NotFoundObjectResult;
            var resultMsg = resultObj.Value as string;

            resultMsg.ShouldContain(missing);
        }

        [Fact]
        public async void DownloadZip_WhenStreamsAreNotInStorage_ReturnsNotFoundWithIds()
        {
            var id = new Guid[] { Guid.NewGuid() };
            var bytes = Array.Empty<byte>();
            var msg = "archive1; archive2";

            List<Archive> archives = ArchiveBuilder.BuildArchives(1);

            _business.Setup(b => b.GetByIdsAsync(It.IsAny<Guid[]>(), It.IsAny<string>()))
                .ReturnsAsync((archives, ""));

            _business.Setup(b => b.DownloadMultipleAsync(It.IsAny<List<Archive>>()))
                .ReturnsAsync((bytes, msg));

            var result = await _controller.DownloadZip(id);

            var resultObj = result as NotFoundObjectResult;
            var resultMsg = resultObj.Value as string;

            resultMsg.ShouldContain(msg);

        }

        [Fact]
        public async void DownloadZip_WhenArchivesAreInRepositoryAndStreamsAreInStorage_ReturnsFileStreamResult()
        {
            Guid[] ids = new Guid[] { Guid.NewGuid() };
            List<Archive> archives = ArchiveBuilder.BuildArchives(4);
            byte[] downloadZip = new byte[] { 255, 0, 255, 0, 255};

            _business.Setup(b => b.GetByIdsAsync(It.IsAny<Guid[]>(), It.IsAny<string>()))
                .ReturnsAsync((archives, ""));

            _business.Setup(b => b.DownloadMultipleAsync(It.IsAny<List<Archive>>()))
                .ReturnsAsync((downloadZip, ""));

            var result = await _controller.DownloadZip(ids);

            var fileStreamResult = result as FileContentResult;
            fileStreamResult.FileContents.ShouldBe(downloadZip);
        }

        [Fact]
        public async void Upload_WhenFilesAreNull_ReturnsBadRequest()
        {
            IEnumerable<IFormFile>? files = null;

            var result = await _controller.Upload(files);

            result.ShouldBeAssignableTo<BadRequestResult>();
        }

        [Fact]
        public async void Upload_WhenFilesProvided_ReturnsOkWithArchives()
        {
            var files = new List<IFormFile>()
            {
                new FormFile(new MemoryStream(new byte[]{ }), 0, 0, "", "")
            };
            var archives = ArchiveBuilder.BuildArchives(6);

            _business.Setup(b => b.UploadAsync(It.IsAny<IEnumerable<IFormFile>>(), It.IsAny<string>()))
                .ReturnsAsync(archives);

            var result = await _controller.Upload(files);

            result.ShouldBeAssignableTo<OkObjectResult>();
            var objResult = result as OkObjectResult;
            var resultValue = objResult.Value as List<ArchiveViewModel>;

            resultValue.FirstOrDefault().FileName.ShouldBe(archives.FirstOrDefault().FileName);
            resultValue.Count.ShouldBe(archives.Count);
        }

        [Fact]
        public async void Delete_WhenArchiveIsNotInRepository_ReturnsNotFound()
        {
            Guid id = Guid.NewGuid();
            Archive? nullArchive = null;

            _business.Setup(b => b.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<string>()))
                .ReturnsAsync(nullArchive);

            var result = await _controller.Delete(id);

            result.ShouldBeAssignableTo<NotFoundResult>();
        }

        [Fact]
        public async void Delete_WhenArchiveIsInRepository_ReturnsNoContent()
        {
            Guid id = Guid.NewGuid();
            Archive? archive = new ArchiveBuilder().Build();

            _business.Setup(b => b.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<string>()))
                .ReturnsAsync(archive);

            var result = await _controller.Delete(id);

            result.ShouldBeAssignableTo<NoContentResult>();
        }
    }
}
