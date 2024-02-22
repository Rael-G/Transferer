using Application.Dtos;
using Application.Interfaces.Services;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO.Compression;
using System.Security.Claims;

namespace Application.Services
{
    public class ArchiveService : IArchiveService
    {
        private readonly IArchiveRepository _archiveRepository;
        private readonly IFileStorage _storage;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public ArchiveService(IArchiveRepository repository,
            IFileStorage storage, IUserRepository userRepository, IMapper mapper)
        {
            _archiveRepository = repository;
            _storage = storage;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<List<ArchiveDto>> GetAllAsync(string userId)
            => _mapper.Map<List<ArchiveDto>>(await _archiveRepository.GetAllAsync(userId));

        public async Task<List<ArchiveDto>> GetAllAsync()
            => _mapper.Map<List<ArchiveDto>>(await _archiveRepository.GetAllAsync());

        public async Task<ArchiveDto?> GetByIdAsync(Guid id, string userId)
            => _mapper.Map<ArchiveDto>(await _archiveRepository.GetByIdAsync(id, userId));

        public async Task<List<ArchiveDto>?> GetByNameAsync(string name, string userId)
            => _mapper.Map<List<ArchiveDto>>(await _archiveRepository.GetByNameAsync(name, userId));

        public async Task<(List<ArchiveDto> archives, string? missing)> GetByIdsAsync(Guid[] ids, string userId)
        {
            List<ArchiveDto> archives = new();
            var missing = "";
            foreach (var id in ids)
            {
                var archive = await GetByIdAsync(id, userId);
                if (archive == null)
                {
                    missing = id.ToString();
                    break;
                }
                archives.Add(archive);
            }

            return (archives, missing);
        }

        public async Task<List<ArchiveDto>> UploadAsync(IEnumerable<IFormFile> files, string userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            List<Archive> archives = new();

            foreach (var file in files)
            {
                Stream stream = file.OpenReadStream();
                string filePath = await _storage.StoreAsync(stream);
                var archive = new Archive(file.FileName, file.ContentType, file.Length, filePath, user);
                await _archiveRepository.SaveAsync(archive);
                archives.Add(archive);
                user.Archives.Add(archive);
            }
            await _userRepository.UpdateAsync(user);

            return _mapper.Map<List<ArchiveDto>>(archives);
        }

        public async Task<Stream?> DownloadAsync(ArchiveDto archive)
            => await _storage.GetByPathAsync(archive.Path);

        public async Task<(byte[] data, string? missing)> DownloadMultipleAsync(List<ArchiveDto> archives)
        {
            // Initialize a string to track missing archives and a memory stream to store the zip file content.
            var missing = "";
            using var memoryStream = new MemoryStream();

            // Create a ZipArchive using the memory stream in write mode with compression.
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                foreach (var item in archives)
                {
                    var stream = await _storage.GetByPathAsync(item.Path);

                    // If the stream is null, the archive is missing, add its name to the missing list.
                    if (stream == null)
                    {
                        missing = item.FileName;
                        break;
                    }
                    var zipEntry = archive.CreateEntry(item.FileName, CompressionLevel.Optimal);

                    // Open a stream for writing to the zip entry and copy the content from the archive's stream.
                    using var zipStream = zipEntry.Open();
                    stream.CopyTo(zipStream);
                }
            }

            // Return a tuple containing the byte array of the zip file data and the missing archives string.
            return (memoryStream.ToArray(), missing);
        }

        // Deletes an archive, removes it from the associated user, and deletes the file from storage
        public async Task DeleteAsync(ArchiveDto archiveDto)
        {
            var user = archiveDto.User;
            var archive = _mapper.Map<Archive>(archiveDto);
            user.Archives.Remove(archive);

            await _userRepository.UpdateAsync(user);

            await _storage.DeleteAsync(archive.Path);
        }


        public string GetUserIdFromClaims(ClaimsPrincipal user)
            => user.Claims.FirstOrDefault(c => c.Type == "UserId").Value;
    }
}
