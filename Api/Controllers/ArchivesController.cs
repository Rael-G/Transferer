using Api.Data.Interfaces;
using Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using System.ComponentModel;
using System.Net.Mime;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Xml.Linq;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class ArchivesController : ControllerBase
    {
        //TODO
        //Implementar autorização e criptografia
        //Upload de multiplus arquivos
        //Download de multiplus arquivos devolvendo um .zip
        //deletar arquivos
        //Organização em Pastas?
        //verificar virus?
        //Integração com Armazenamento em Nuvem?
        //documentação swagger e readme.md
        //testes unitarios e integração

        private readonly IArchiveRepository _archiveRepository;
        private readonly IFileStorage _fileStorage;

        public ArchivesController(IArchiveRepository repository, IFileStorage storage)
        {
            _archiveRepository = repository;
            _fileStorage = storage;
        }

        [HttpGet]
        public async Task<IActionResult> ListAll()
        {
            var archives = await _archiveRepository.GetAllAsync();

            return Ok(archives);
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            
            Stream stream = file.OpenReadStream();
            string filePath = _fileStorage.Store(stream);

            var archive = new Archive(file.FileName, file.ContentType, file.Length, filePath);
            var archived = await _archiveRepository.SaveAsync(archive);

            return CreatedAtAction(nameof(Download), new { archived.Id }, archived);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Download(int id) 
        {
            var archive = await _archiveRepository.GetByIdAsync(id);
            if (archive == null) { return NotFound(); }
            
            var stream = _fileStorage.GetByPath(archive.Path);
            if (stream == null) { return NotFound(); }

            return File(stream, archive.ContentType, archive.FileName);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var archive = await _archiveRepository.GetByIdAsync(id);
            if (archive == null) { return NotFound(); };

            if (!_fileStorage.Delete(archive.Path))
            {
                return NotFound();
            }

            await _archiveRepository.DeleteAsync(id);

            return NoContent();
        }
    }
}
