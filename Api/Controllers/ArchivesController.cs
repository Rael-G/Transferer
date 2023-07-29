using Api.Data.Interfaces;
using Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using System.ComponentModel;
using System.Reflection.Metadata;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class ArchivesController : ControllerBase
    {
        //TODO
        //Implementar autorização e criptografia
        private readonly IArchiveRepository _archiveRepository;
        private readonly IFileStorage _fileStorage;

        public ArchivesController(IArchiveRepository repository, IFileStorage storage)
        {
            _archiveRepository = repository;
            _fileStorage = storage;
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            
            Stream stream = file.OpenReadStream();
            string filePath = _fileStorage.Store(stream);

            var archive = new Archive { 
                Name = file.Name, FileName = file.FileName, ContentType = file.ContentType, 
                Length = file.Length, Path = filePath };

            var archived = await _archiveRepository.SaveAsync(archive);

            return CreatedAtAction(nameof(Download), new { archived.Id }, archived);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Download(int id) 
        {
            var archive = await _archiveRepository.GetByIdAsync(id);
            var stream = _fileStorage.GetByPath(archive.Path);
            return File(stream, archive.ContentType, archive.FileName);
        }
    }
}
