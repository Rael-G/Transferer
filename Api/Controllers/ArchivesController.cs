using Api.Data.Interfaces;
using Api.Models;
using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;
using System.Xml.Linq;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ArchivesController : ControllerBase
    {
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
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest(string.Empty);

            var archives = await _archiveRepository.GetByNameAsync(name);

            return Ok(archives);
        }

        [HttpGet("download/{id}")]
        public async Task<IActionResult> Download(int id)
        {
            if (id <= 0)
                return BadRequest();

            var archive = await _archiveRepository.GetByIdAsync(id);
            if (archive == null) 
                return NotFound();

            var stream = _fileStorage.GetByPath(archive.Path);
            if (stream == null)
                return StatusCode(500, "File is missing in Storage.");

            return File(stream, archive.ContentType, archive.FileName);
        }

        [HttpGet("download/zip/{id}")]
        public async Task<IActionResult> DownloadZip(string id)
        {
            int[] idArray;
            try
            {
                idArray = id.Split(',').Select(int.Parse).ToArray();
            }
            catch (Exception)
            {
                return BadRequest(id);
            }

            (var archives, var notFoundIds) = await _archiveRepository.GetByIdsAsync(idArray);

            if (archives == null || !archives.Any())
                return NotFound();

            byte[] zipData = CreateZipData(archives, ref notFoundIds);

            
            if (notFoundIds.Any())
            {
                //TODO
                //informar de alguma forma o usuario sobre os arquivos que não foram encontrados
            }
            if (zipData == null)
                return StatusCode(500, "Files are missing in Storage.");
            return File(zipData, "application/zip", "archives.zip");
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IEnumerable<IFormFile> files)
        {
            List<Archive> archives = new();

            if (files == null || !files.Any())
                return BadRequest();

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
            if (id < 0)
                return BadRequest();

            var archive = await _archiveRepository.GetByIdAsync(id);
            if (archive == null)
                return NotFound();

            _fileStorage.Delete(archive.Path);
            await _archiveRepository.DeleteAsync(id);

            return NoContent();
        }

        //Auxiliary Methods

        private byte[]? CreateZipData(List<Archive> archives, ref List<int> notFoundIds)
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
                        notFoundIds.Add(item.Id ?? 0);
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

