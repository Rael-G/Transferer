using Api.Controllers;
using Api.Data.Contexts;
using Api.Data.Interfaces;
using Api.Data.Repositories;
using Api.Models;
using Bogus;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests._Builder;

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
        public async void ListAll_ReturnsEveryArchiveInRepository()
        {
            var archives = ArchiveBuilder.BuildArchives(10);
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(archives);

            var result = await _controller.ListAll();

            _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
            result.ShouldBeOfType(new OkObjectResult(archives).GetType());
            var actualArchives = (result as OkObjectResult).Value as List<Archive>;
            actualArchives.ShouldNotBeNull();
            actualArchives.ShouldBe(archives);
        }
    }
}
