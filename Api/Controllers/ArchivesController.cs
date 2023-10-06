using Api.Data.Interfaces;
using Api.Extensions;
using Api.Models;
using Api.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;
using System.Security.Claims;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ArchivesController : ControllerBase
    {
        private readonly IArchiveRepository _archiveRepository;
        private readonly IFileStorage _fileStorage;
        private readonly UserManager<User> _userManager;

        public ArchivesController(IArchiveRepository repository, 
            IFileStorage storage, UserManager<User> userManager)
        {
            _archiveRepository = repository;
            _fileStorage = storage;
            _userManager = userManager;
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpGet("list")]
        public async Task<IActionResult> List()
        {

            var userId = _userManager.GetUserIdFromClaims(User);
            if (userId == null)
            {
                return StatusCode(500, "Claim.UserId not found");
            }

            var archives = await _archiveRepository.GetAllAsync(userId);
            var archivesViewModel = ArchiveViewModel.MapArchivesToViewModel(archives);

            return Ok(archivesViewModel);
        }

        [Authorize(AuthenticationSchemes = "Bearer", Roles = "admin")]
        [HttpGet("listall")]
        public async Task<IActionResult> ListAll()
        {
            var archives = await _archiveRepository.GetAllAsync();
            var archivesViewModel = ArchiveViewModel.MapArchivesToViewModel(archives);

            return Ok(archivesViewModel);
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpGet("search/{name}")]
        public async Task<IActionResult> Search(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest(string.Empty);
            }

            var userId = _userManager.GetUserIdFromClaims(User);
            if (userId == null)
            {
                return StatusCode(500, "Claim.UserId not found");
            }

            var archives = await _archiveRepository.GetByNameAsync(name, userId);
            var archivesViewModel = ArchiveViewModel.MapArchivesToViewModel(archives);

            return Ok(archivesViewModel);
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpGet("download/{id}")]
        public async Task<IActionResult> Download(Guid id)
        {
            var userId = _userManager.GetUserIdFromClaims(User);
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

            var userId = _userManager.GetUserIdFromClaims(User);
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

            var userId = _userManager.GetUserIdFromClaims(User);
            if (userId == null)
            {
                return StatusCode(500, "Claim.UserId not found");
            }

            var user = await _userManager.FindByIdAsync(userId);

            List<ArchiveViewModel> archives = new();

            foreach (var file in files)
            {
                Stream stream = file.OpenReadStream();
                string filePath = _fileStorage.Store(stream);
                var archive = new Archive(file.FileName, file.ContentType, file.Length, filePath, user);
                await _archiveRepository.SaveAsync(archive);
                archives.Add(new ArchiveViewModel(archive));
                user.Archives.Add(archive);
            }
            await _userManager.UpdateAsync(user);
            return Ok(archives);
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {

            var userId = _userManager.GetUserIdFromClaims(User);
            if (userId == null)
            {
                return StatusCode(500, "Claim.UserId not found");
            }

            var archive = await _archiveRepository.GetByIdAsync(id, userId);
            if (archive == null)
                return NotFound();

            _fileStorage.Delete(archive.Path);
            await _archiveRepository.DeleteAsync(id, userId);

            var user = await _userManager.FindByIdAsync(userId);
            user.Archives.RemoveAll(a => a.Id == archive.Id);
            _userManager.UpdateAsync(user);

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

