using Application.Dtos;
using Application.Interfaces.Services;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Http;
using Moq;
using Shouldly;
using System.Security.Claims;
using Tests._Builder;

namespace Tests.Unit.Business
{
    public class ArchiveBusinessTests
    {
        private readonly Mock<IArchiveRepository> _archiveRepository;
        private readonly Mock<IFileStorage> _storage;
        private readonly Mock<IMapper> _mapper;
        private readonly Mock<IUserRepository> _userRepository;
        private readonly IArchiveService _business;

        private Guid _guid = Guid.NewGuid();
        private string _userId = Guid.NewGuid().ToString();
        private Archive _archive = new ArchiveBuilder().Build();
        private ArchiveDto _archiveDto = new ArchiveBuilder().BuildDto();
        private User _user = new UserBuilder().Build();
        private string _path = new Guid().ToString();

        public ArchiveBusinessTests() 
        {
            _archiveRepository = new Mock<IArchiveRepository>();
            _storage = new Mock<IFileStorage>();
            _userRepository = new Mock<IUserRepository>();
            _mapper = new Mock<IMapper>();

            _business = new ArchiveService(_archiveRepository.Object, _storage.Object, _userRepository.Object, _mapper.Object);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async void GetAllAsync_WithNullOrEmptyUserId_ReturnsAllArchives(string userId)
        {
            _archiveRepository.Setup(r => r.GetAllAsync(userId))
                .ReturnsAsync(new List<Archive>());
            _mapper.Setup(m => m.Map<IEnumerable<ArchiveDto>>(It.IsAny<IEnumerable<Archive>>()))
                .Returns(new List<ArchiveDto>());

            var result = await _business.GetAllAsync(userId);

            result.ShouldNotBeNull().ShouldBeEmpty();
            _archiveRepository.Verify(r => r.GetAllAsync(userId), Times.Once);
        }

        [Fact]
        public async void GetByIds_IfArchivesAreMissing_ReturnsMissingArchivesIdInString()
        {
            var ids = new Guid[] { _guid };
            _archiveRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<string>()))
                .ReturnsAsync(() => null);
            var result = await _business.GetByIdsAsync(ids, _userId);

            result.missing.ShouldNotBeNull()
                .ShouldContain(_guid.ToString());
        }

        [Fact]
        public async void GetByIds_IfArchivesAreFound_ReturnsEmptyMissingStringAndArchives()
        {
            var ids = new Guid[] { _guid };
            _archiveRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<string>()))
                .ReturnsAsync(_archive);
            _mapper.Setup(m => m.Map<ArchiveDto>(It.IsAny<Archive>()))
                .Returns(_archiveDto);

            var result = await _business.GetByIdsAsync(ids, _userId);

            result.missing.ShouldBeEmpty();
            result.archives.ShouldContain(a => a.Id == _archive.Id);
        }


        [Fact]
        public async void Upload_shouldSaveArchiveAndAddToUser()
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
        }

        [Fact]
        public async void DownloadAsync_ReturnsCorrectStream()
        {
            _storage.Setup(s => s.GetByPathAsync(It.IsAny<string>())).ReturnsAsync(new MemoryStream());

            var result = await _business.DownloadAsync(_archiveDto);

            result.ShouldNotBeNull().ShouldBeAssignableTo<Stream>();
            _storage.Verify(s => s.GetByPathAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async void DownloadMultiple_IfArchivesAreMissing_ReturnsMissingArchivesIdInString()
        {
            var archive = new ArchiveBuilder().BuildDto();
            List<ArchiveDto> archives = new() { archive };
            _storage.Setup(s => s.GetByPathAsync(It.IsAny<string>()))
                .ReturnsAsync(() => null);

            var result = await _business.DownloadMultipleAsync(archives);

            result.missing.ShouldNotBeNull()
                .ShouldContain(archive.FileName);
        }

        [Fact]
        public async void DownloadMultiple_IfArchivesAreFound_ReturnsEmptyMissingStringAndZip()
        {
            var stream = new MemoryStream();
            var archives = ArchiveBuilder.BuildArchivesDto(1);

            _storage.Setup(r => r.GetByPathAsync(It.IsAny<string>()))
                .ReturnsAsync(stream);

            var result = await _business.DownloadMultipleAsync(archives);

            result.missing.ShouldBeEmpty();
            result.data.Length.ShouldBeGreaterThan(0);
        }

        [Fact]
        public async void DeleteAsync_RemovesArchiveFromUserAndDeletesFile()
        {
            _userRepository.Setup(u => u.UpdateAsync(It.IsAny<User>()));
            _storage.Setup(s => s.DeleteAsync(It.IsAny<string>()));
            _mapper.Setup(m => m.Map<Archive>(It.IsAny<ArchiveDto>()))
                .Returns(_archive);
            await _business.DeleteAsync(_archiveDto);

            _userRepository.Verify(u => u.UpdateAsync(It.IsAny<User>()), Times.Once);
            _storage.Verify(s => s.DeleteAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void GetUserIdFromClaims_ExtractsUserIdFromClaims()
        {
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("UserId", _userId)
            }));

            var result = _business.GetUserIdFromClaims(claimsPrincipal);

            result.ShouldNotBeNull().ShouldBe(_userId);
        }
    }
}
