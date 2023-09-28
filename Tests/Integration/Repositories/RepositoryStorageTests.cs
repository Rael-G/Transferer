using Api.Controllers;
using Api.Data.Contexts;
using Api.Data.Interfaces;
using Api.Data.Repositories;
using Api.Models;
using Bogus;
using Microsoft.AspNetCore.Http;
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
        private readonly IUserRepository _userRepository;

        public RepositoryStorageTests()
        {
            var context = new TransfererDbContext(new DbContextOptionsBuilder<TransfererDbContext>()
            .UseInMemoryDatabase(databaseName: "InMemoryTestDatabase")
            .Options);
            var tempPath = Path.Combine(Path.GetTempPath(), "tests");

            _fileStorage = new LocalFileStorage(tempPath);
            _archiveRepository = new ArchiveRepository(context);
            _userRepository = new UserRepository();

            _controller = new ArchivesController(_archiveRepository, _fileStorage, _userRepository);
        }

        [Fact]
        public async Task UploadAndDownload_FileExists_ReturnsFileContent()
        {
            Guid Id;
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

            var uploadResponse = await _controller.Upload(files);
            var uploadedArchive = (Assert.IsType<OkObjectResult>(uploadResponse).Value as List<Archive>).First();
            
            Id = uploadedArchive.Id.Value;
            
            var downloadResponse = await _controller.Download(Id);
            var fileResult = Assert.IsType<FileStreamResult>(downloadResponse);

            using var downloadStream = new MemoryStream();
            fileResult.FileStream.CopyTo(downloadStream);

            var downloadedBytes = downloadStream.ToArray();
            var downloadedContent = Encoding.UTF8.GetString(downloadedBytes);

            Assert.Equal(fileContent, downloadedContent);
        }
    }
}
