using Api.Controllers;
using Api.Data.Contexts;
using Api.Data.Interfaces;
using Api.Data.Repositories;
using Api.Models;
using Bogus;
using Bogus.DataSets;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests._Builder;
using static System.Net.Mime.MediaTypeNames;

namespace Tests.Archives
{
    public class ArchiveControllerTests
    {
        private readonly Mock<IArchiveRepository> _mockRepository;
        private readonly Mock<IFileStorage> _mockStorage;
        private readonly ArchivesController _controller;
        private readonly Faker _faker;

        public ArchiveControllerTests()
        {
            _faker = new Faker();

            _mockRepository = new Mock<IArchiveRepository>();
            _mockStorage = new Mock<IFileStorage>();
            _controller = new ArchivesController(_mockRepository.Object, _mockStorage.Object);
        }

        [Fact]
        public async void ListAll_ReturnsOkAndArchives()
        {
            var archives = ArchiveBuilder.BuildArchives(10);
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(archives);

            var result = await _controller.ListAll();

            _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
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
        public async void Search_ReturnsOkAndArchives()
        {
            List<Archive> archives = ArchiveBuilder.BuildArchives(10);
            var names = "picture";
            _mockRepository.Setup(r => r.GetByNameAsync(names)).ReturnsAsync(archives);

            IActionResult result = await _controller.Search(names);

            _mockRepository.Verify(r => r.GetByNameAsync(names), Times.Once);
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
            var id = 1;
            Archive? notFoundArchive = null;
            _mockRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(notFoundArchive);

            var result = await _controller.Download(id);

            _mockRepository.Verify(r => r.GetByIdAsync(id), Times.Once);
            result.ShouldBeAssignableTo<NotFoundResult>();
        }

        [Fact]
        public async void Download_WhenStreamIsNotInStorage_ReturnsNotFound()
        {
            var id = 1;
            Archive archive = new ArchiveBuilder().Build();
            Stream? notFoundStream = null;
            _mockRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(archive);
            _mockStorage.Setup(s => s.GetByPath(archive.Path)).Returns(notFoundStream);

            var result = await _controller.Download(id);

            _mockRepository.Verify(r => r.GetByIdAsync(id), Times.Once);
            _mockStorage.Verify(s => s.GetByPath(archive.Path), Times.Once);
            result.ShouldBeAssignableTo<NotFoundResult>();
        }

        [Fact]
        public async void Download_WhenArchiveInRepositoryAndStreamInStorage_ReturnsFileStreamResult()
        {
            var id = 1;
            Archive archive = new ArchiveBuilder().Build();
            Stream stream = new MemoryStream();
            _mockRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(archive);
            _mockStorage.Setup(s => s.GetByPath(archive.Path)).Returns(stream);

            var result = await _controller.Download(id);

            _mockRepository.Verify(r => r.GetByIdAsync(id), Times.Once);
            _mockStorage.Verify(s => s.GetByPath(archive.Path), Times.Once);
            result.ShouldBeAssignableTo<FileStreamResult>();
            if (result is FileStreamResult fileStreamResult)
            {
                fileStreamResult.FileStream.ShouldBe(stream);
                fileStreamResult.ContentType.ShouldBe(archive.ContentType);
                fileStreamResult.FileDownloadName.ShouldBe(archive.FileName);
            }
        }
    }
}
