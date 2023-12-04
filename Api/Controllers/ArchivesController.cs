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

        /// <summary>
        /// Get a list of archives for the authenticated user.
        /// </summary>
        /// <returns>Returns a list of archives belonging to the authenticated user.</returns>
        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpGet("list")]
        [ProducesResponseType(typeof(IEnumerable<ArchiveViewModel>), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> List()
        {
            var userId = _business.GetUserIdFromClaims(User);
            var archives = await _business.GetAllAsync(userId);
            var archivesViewModel = ArchiveViewModel.MapToViewModel(archives);

            return Ok(archivesViewModel);
        }

        /// <summary>
        /// Get a list of all archives (admin role required).
        /// </summary>
        /// <returns>Returns a list of all archives in the system.</returns>
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "admin")]
        [HttpGet("listall")]
        [ProducesResponseType(typeof(IEnumerable<ArchiveViewModel>), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> ListAll()
        {
            var archives = await _business.GetAllAsync();
            var archivesViewModel = ArchiveViewModel.MapToViewModel(archives);

            return Ok(archivesViewModel);
        }

        /// <summary>
        /// Search for archives by name for the authenticated user.
        /// </summary>
        /// <param name="name">The name to search for.</param>
        /// <returns>Returns a list of archives matching the specified name for the authenticated user.</returns>
        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpGet("search/{name}")]
        [ProducesResponseType(typeof(IEnumerable<ArchiveViewModel>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
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

        /// <summary>
        /// Download a specific archive by its ID for the authenticated user.
        /// </summary>
        /// <param name="id">The ID of the archive to download.</param>
        /// <returns>Returns the file content of the specified archive for the authenticated user.</returns>
        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpGet("download/{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
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

        /// <summary>
        /// Download multiple archives as a zip file for the authenticated user.
        /// </summary>
        /// <param name="guids">Array of archive IDs to download.</param>
        /// <returns>Returns a zip file containing the specified archives for the authenticated user.</returns>
        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpGet("download/zip")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
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

        /// <summary>
        /// Upload one or more files for the authenticated user.
        /// </summary>
        /// <param name="files">The files to upload.</param>
        /// <returns>Returns information about the uploaded archives for the authenticated user.</returns>
        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPost("upload")]
        [ProducesResponseType(typeof(IEnumerable<ArchiveViewModel>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Upload(IEnumerable<IFormFile> files)
        {
            if (files == null || !files.Any())
                return BadRequest();

            var userId = _business.GetUserIdFromClaims(User);
            var archives = await _business.UploadAsync(files, userId);

            var viewModel = ArchiveViewModel.MapToViewModel(archives);
            return Ok(viewModel);
        }

        /// <summary>
        /// Delete a specific archive by its ID for the authenticated user.
        /// </summary>
        /// <param name="id">The ID of the archive to delete.</param>
        /// <returns>Returns a success response if the deletion is successful for the authenticated user.</returns>
        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpDelete("delete/{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
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

