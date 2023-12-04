using Api.Data.Interfaces;
using Api.Models;
using System.IO.Compression;
using System.Security.Claims;

namespace Api.Business.Implementation
{
    public class ArchiveBusiness : IArchiveBusiness
    {
        private readonly IArchiveRepository _archiveRepository;
        private readonly IFileStorage _storage;
        private readonly IUserRepository _userRepository;

        public ArchiveBusiness(IArchiveRepository repository,
            IFileStorage storage, IUserRepository userRepository)
        {
            _archiveRepository = repository;
            _storage = storage;
            _userRepository = userRepository;
        }

        public async Task<List<Archive>> GetAllAsync(string userId)
            => await _archiveRepository.GetAllAsync(userId);
        
        public async Task<List<Archive>> GetAllAsync()
            => await _archiveRepository.GetAllAsync();

        public async Task<Archive?> GetByIdAsync(Guid id, string userId)
            => await _archiveRepository.GetByIdAsync(id, userId);

        public async Task<List<Archive>?> GetByNameAsync(string name, string userId)
            => await _archiveRepository.GetByNameAsync(name, userId);

        public async Task<(List<Archive> archives, string? missing)> GetByIdsAsync(Guid[] ids, string userId)
        {
            List<Archive> archives = new();
            var missing = "";
            foreach (var id in ids)
            {
                var archive = await GetByIdAsync(id, userId);
                if (archive == null)
                    missing += id.ToString() + "; ";
                archives.Add(archive);
            }

            return (archives, missing);
        }

        public async Task<List<Archive>> UploadAsync(IEnumerable<IFormFile> files, string userId)
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

            return archives;
        }

        public async Task<Stream?> DownloadAsync(Archive archive)
            => await _storage.GetByPathAsync(archive.Path);

        public async Task<(byte[] data, string? missing)> DownloadMultipleAsync(List<Archive> archives)
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
                        missing += $"{item.FileName}; ";
                        continue;
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
        public async Task DeleteAsync(Archive archive)
        {
            var user = archive.User;

            user.Archives.Remove(archive);

            await _userRepository.UpdateAsync(user);

            await _storage.DeleteAsync(archive.Path);
        }


        public string GetUserIdFromClaims(ClaimsPrincipal user)
            => user.Claims.FirstOrDefault(c => c.Type == "UserId").Value;
    }
}
