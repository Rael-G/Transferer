using Api.Business;
using Api.Business.Implementation;
using Api.Data.Interfaces;
using Api.Models;
using Microsoft.AspNetCore.Http;
using Moq;
using Shouldly;
using Tests._Builder;

namespace Tests.Unit.Business
{
    public class ArchiveBusinessTests
    {
        private readonly Mock<IArchiveRepository> _archiveRepository;
        private readonly Mock<IFileStorage> _storage;
        private readonly Mock<IUserRepository> _userRepository;
        private readonly IArchiveBusiness _business;

        private Guid _guid = Guid.NewGuid();
        private string _userId = Guid.NewGuid().ToString();
        private Archive _archive = new ArchiveBuilder().Build();
        private User _user = new UserBuilder().Build();
        private string _path = new Guid().ToString();

        public ArchiveBusinessTests() 
        {
            _archiveRepository = new Mock<IArchiveRepository>();
            _storage = new Mock<IFileStorage>();
            _userRepository = new Mock<IUserRepository>();

            _business = new ArchiveBusiness(_archiveRepository.Object, _storage.Object, _userRepository.Object);
        }

        [Fact]
        public async void GetByIds_IfArchivesAreMissing_ReturnsMissingArchivesIdInString()
        {
            var ids = new Guid[] { _guid };
            _archiveRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<string>()))
                .ReturnsAsync(() => null);
            var result = await _business.GetByIdsAsync(ids, _userId);

            result.missing.ShouldContain(_guid.ToString());
        }

        [Fact]
        public async void GetByIds_IfArchivesAreFound_ReturnsEmptyMissingStringAndArchives()
        {
            var ids = new Guid[] { _guid };
            var emptyMissing = "";
            _archiveRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<string>()))
                .ReturnsAsync(_archive);
            var result = await _business.GetByIdsAsync(ids, _userId);

            result.missing.ShouldBeEmpty();
            result.archives.ShouldContain(_archive);
        }


        [Fact]
        public async void Upload_shouldCallMethodsAndReturnsArchives()
        {
            var fileName = "archive.txt";
            var contentType = "application/zip";
            var files = new List<IFormFile>()
            {
                new FormFile(new MemoryStream(new byte[]{ }), 0, 255, "", fileName)
                {
                    Headers = new HeaderDictionary(),
                    ContentType = contentType
                }
            };

            _userRepository.Setup(u => u.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(_user);
            _storage.Setup(s => s.StoreAsync(It.IsAny<Stream>()))
                .ReturnsAsync(_path);

            var result = await _business.UploadAsync(files, _userId);

            _archiveRepository.Verify(a => a.SaveAsync(It.IsAny<Archive>()), Times.Once);
            _userRepository.Verify(u => u.UpdateAsync(_user), Times.Once);

            result.FirstOrDefault().FileName.ShouldBe(fileName);
            result.FirstOrDefault().ContentType.ShouldBe(contentType);

        }

        [Fact]
        public async void DownloadMultiple_IfArchivesAreMissing_ReturnsMissingArchivesIdInString()
        {
            var archive = new ArchiveBuilder().Build();
            List<Archive> archives = new() { archive };
            _storage.Setup(s => s.GetByPathAsync(It.IsAny<string>()))
                .ReturnsAsync(() => null);

            var result = await _business.DownloadMultipleAsync(archives);

            result.missing.ShouldContain(archive.FileName);
        }

        [Fact]
        public async void DownloadMultiple_IfArchivesAreFound_ReturnsEmptyMissingStringAndZip()
        {
            var stream = new MemoryStream();
            var emptyMissing = "";
            var archives = ArchiveBuilder.BuildArchives(1);

            _storage.Setup(r => r.GetByPathAsync(It.IsAny<string>()))
                .ReturnsAsync(stream);

            var result = await _business.DownloadMultipleAsync(archives);


            result.missing.ShouldBeEmpty();
            result.data.Length.ShouldBeGreaterThan(0);
        }
    }
}
