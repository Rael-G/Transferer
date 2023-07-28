using Api.Data;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.Reflection.Metadata;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class ArchivesController : ControllerBase
    {
        private readonly IArchiveRepository _archiveRepository;
        private readonly string _path;

        public ArchivesController(IArchiveRepository repository)
        {
            _archiveRepository = repository;
            _path = $"{AppDomain.CurrentDomain.BaseDirectory}\\Storage\\Files\\";
        }

        [HttpPost]
        public IActionResult Upload(IFormFile file)
        {
            return Ok();
        }

        [HttpGet("{id}")]
        public IActionResult Download(int id) 
        {
            return new FileStreamResult(new MemoryStream(), "");
        }
    }
}
