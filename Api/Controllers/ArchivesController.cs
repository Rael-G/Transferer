using Api.Data.Interfaces;
using Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.Extensions.FileProviders;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.IO.Pipes;
using System.Net.Mime;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Xml.Linq;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ArchivesController : ControllerBase
    {
        //TODO EXTERNO
        //Testes unitarios e integração
        //Implementar autorização e criptografia
        //Cocumentação swagger e readme.md
        //Verificação de virus nos arquivos
        //Integração com Armazenamento em Nuvem?
        //Organização em Pastas?

        private readonly IArchiveRepository _archiveRepository;
        private readonly IFileStorage _fileStorage;

        public ArchivesController(IArchiveRepository repository, IFileStorage storage)
        {
            _archiveRepository = repository;
            _fileStorage = storage;
        }

        [HttpGet("list")]
        public async Task<IActionResult> ListAll()
        {
            var archives = await _archiveRepository.GetAllAsync();

            return Ok(archives);
        }

        [HttpGet("search/{name}")]
        public async Task<IActionResult> Search(string name)
        {
            var archives = await _archiveRepository.GetByNameAsync(name);

            return Ok(archives);
        }

        [HttpGet("download/{id}")]
        public async Task<IActionResult> Download(int id)
        {
            var archive = await _archiveRepository.GetByIdAsync(id);
            if (archive == null) 
                return NotFound();

            var stream = _fileStorage.GetByPath(archive.Path);
            if (stream == null)
                return NotFound();

            return File(stream, archive.ContentType, archive.FileName);
        }

        //TODO
        //Avisar quais arquivos não foram encontrados
        //Verificar se o Zip está vazio
        [HttpGet("downloadzip/{ids}")]
        public async Task<IActionResult> DownloadZip(string ids)
        {
            if (string.IsNullOrEmpty(ids))
                return NotFound();

            int[] idArray = ids.Split(',').Select(int.Parse).ToArray();
            var archives = await _archiveRepository.GetByIdsAsync(idArray);

            if (archives == null)
                return NotFound();

            byte[] zipData = await CreateZipData(archives);
            return File(zipData, "application/zip", "archives.zip");
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IEnumerable<IFormFile> files)
        {
            List<Archive> archives = new();
            foreach (var file in files)
            {
                Stream stream = file.OpenReadStream();
                string filePath = _fileStorage.Store(stream);
                var archive = new Archive(file.FileName, file.ContentType, file.Length, filePath);
                archives.Add(await _archiveRepository.SaveAsync(archive));
            }
            return Ok(archives);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var archive = await _archiveRepository.GetByIdAsync(id);
            if (archive == null) { return NotFound(); };

            if (!_fileStorage.Delete(archive.Path))
                return NotFound();

            await _archiveRepository.DeleteAsync(id);

            return NoContent();
        }

        //Auxiliary Methods

        private async Task<byte[]> CreateZipData(List<Archive> archives)
        {
            using var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                foreach (var item in archives)
                {
                    var stream = _fileStorage.GetByPath(item.Path);
                    if (stream == null)
                        continue;

                    var zipEntry = archive.CreateEntry(item.FileName, CompressionLevel.Optimal);

                    using var zipStream = zipEntry.Open();
                    await stream.CopyToAsync(zipStream);
                }
            }
            return memoryStream.ToArray();
        }

    }

}

