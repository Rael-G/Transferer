using Api.Business;
using Api.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ArchivesController : ControllerBase
    {
        IArchiveBusiness _business;

        public ArchivesController(IArchiveBusiness business )
        {
            _business = business;
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpGet("list")]
        public async Task<IActionResult> List()
        {
            var userId = _business.GetUserIdFromClaims(User);
            var archives = await _business.GetAllAsync(userId);
            var archivesViewModel = ArchiveViewModel.MapToViewModel(archives);

            return Ok(archivesViewModel);
        }

        [Authorize(AuthenticationSchemes = "Bearer", Roles = "admin")]
        [HttpGet("listall")]
        public async Task<IActionResult> ListAll()
        {
            var archives = await _business.GetAllAsync();
            var archivesViewModel = ArchiveViewModel.MapToViewModel(archives);

            return Ok(archivesViewModel);
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpGet("search/{name}")]
        public async Task<IActionResult> Search(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest("name is empty");
            }

            var userId = _business.GetUserIdFromClaims(User);
            var archives = await _business.GetByNameAsync(name, userId);
            var archivesViewModel = ArchiveViewModel.MapToViewModel(archives);

            return Ok(archivesViewModel);
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpGet("download/{id}")]
        public async Task<IActionResult> Download(Guid id)
        {
            var userId = _business.GetUserIdFromClaims(User);
            var archive = await _business.GetByIdAsync(id, userId);
            if (archive == null) 
                return NotFound();

            var stream = await _business.DownloadAsync(archive);
            if (stream == null)
                return NotFound("File is Missing");

            return File(stream, archive.ContentType, archive.FileName);
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpGet("download/zip")]
        public async Task<IActionResult> DownloadZip([FromQuery] Guid[] guids)
        {
            var userId = _business.GetUserIdFromClaims(User);

            var (archives, missingArchives) = await _business.GetByIdsAsync(guids, userId);

            if (!missingArchives.IsNullOrEmpty())
                return NotFound($"Archive not found. Id: {missingArchives}");

            var (data, missingStreams) = await _business.DownloadMultipleAsync(archives);

            if (!missingStreams.IsNullOrEmpty())
                return NotFound($"File is Missing. Id: {missingStreams}");

            return File(data, "application/zip", "download.zip");
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IEnumerable<IFormFile> files)
        {
            if (files == null || !files.Any())
                return BadRequest();

            var userId = _business.GetUserIdFromClaims(User);
            var archives = await _business.UploadAsync(files, userId);

            var viewModel = ArchiveViewModel.MapToViewModel(archives);
            return Ok(viewModel);
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = _business.GetUserIdFromClaims(User);
            var archive = await _business.GetByIdAsync(id, userId);
            if (archive == null)
                return NotFound();

            await _business.DeleteAsync(archive);

            return NoContent();
        }
    }
}

