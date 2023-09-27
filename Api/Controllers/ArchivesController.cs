using Api.Data.Interfaces;
using Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ArchivesController : ControllerBase
    {
        private readonly IArchiveRepository _archiveRepository;
        private readonly IFileStorage _fileStorage;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ArchivesController(IArchiveRepository repository, IFileStorage storage, 
            UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _archiveRepository = repository;
            _fileStorage = storage;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpGet("list")]
        public async Task<IActionResult> List()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "UserId").Value;
            if (userId == null)
            {
                return StatusCode(500, "Claim.UserId not found");
            }

            var archives = await _archiveRepository.GetAllAsync(userId);

            return Ok(archives);
        }

        [Authorize(AuthenticationSchemes = "Bearer", Roles = "admin")]
        [HttpGet("listall")]
        public async Task<IActionResult> ListAll()
        {
            var archives = await _archiveRepository.GetAllAsync();

            return Ok(archives);
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpGet("search/{name}")]
        public async Task<IActionResult> Search(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest(string.Empty);
            }

            var userId = User.Claims.FirstOrDefault(c => c.Type == "UserId").Value;
            if (userId == null)
            {
                return StatusCode(500, "Claim.UserId not found");
            }

            var archives = await _archiveRepository.GetByNameAsync(name, userId);

            return Ok(archives);
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpGet("download/{id}")]
        public async Task<IActionResult> Download(Guid id)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "UserId").Value;
            if (userId == null)
            {
                return StatusCode(500, "Claim.UserId not found");
            }

            var archive = await _archiveRepository.GetByIdAsync(id, userId);
            if (archive == null) 
                return NotFound();

            var stream = _fileStorage.GetByPath(archive.Path);
            if (stream == null)
                return StatusCode(500, "File is missing in Storage.");

            return File(stream, archive.ContentType, archive.FileName);
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpGet("download/zip/{id}")]
        public async Task<IActionResult> DownloadZip(string id)
        {
            Guid[] idArray;
            try
            {
                idArray = id.Split(',').Select(Guid.Parse).ToArray();
            }
            catch (Exception)
            {
                return BadRequest(id);
            }

            var userId = User.Claims.FirstOrDefault(c => c.Type == "UserId").Value;
            if (userId == null)
            {
                return StatusCode(500, "Claim.UserId not found");
            }

            var archives = await _archiveRepository.GetByIdsAsync(idArray, userId);

            if (archives == null || !archives.Any())
                return NotFound();

            byte[]? zipData = CreateZipData(archives);

            if (zipData == null)
                return StatusCode(500, "Files are missing in Storage.");
            return File(zipData, "application/zip", "archives.zip");
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IEnumerable<IFormFile> files)
        {
            if (files == null || !files.Any())
                return BadRequest();

            var userId = User.Claims.FirstOrDefault(c => c.Type == "UserId").Value;
            if (userId == null)
            {
                return StatusCode(500, "Claim.UserId not found");
            }

            var user = _userManager.Users.FirstOrDefault(u => u.Id == userId);

            List<Archive> archives = new();

            foreach (var file in files)
            {
                Stream stream = file.OpenReadStream();
                string filePath = _fileStorage.Store(stream);
                var archive = new Archive(file.FileName, file.ContentType, file.Length, filePath, user);
                archives.Add(await _archiveRepository.SaveAsync(archive));
                user.Archives.Add(archive);

            }
            return Ok(archives);
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "UserId").Value;
            if (userId == null)
            {
                return StatusCode(500, "Claim.UserId not found");
            }

            var archive = await _archiveRepository.GetByIdAsync(id, userId);
            if (archive == null)
                return NotFound();

            _fileStorage.Delete(archive.Path);
            await _archiveRepository.DeleteAsync(id, userId);

            return NoContent();
        }

        //Auxiliary Methods

        private byte[]? CreateZipData(List<Archive> archives)
        {
            using var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                int foundCount = 0;
                foreach (var item in archives)
                {
                    var stream = _fileStorage.GetByPath(item.Path);
                    if (stream == null)
                    {
                        continue;
                    }
                    foundCount++;
                    var zipEntry = archive.CreateEntry(item.FileName, CompressionLevel.Optimal);

                    using var zipStream = zipEntry.Open();
                    stream.CopyTo(zipStream);
                }
                if (foundCount == 0)
                    return null;
            }
            return memoryStream.ToArray();
        }

    }

}

