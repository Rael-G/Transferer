using Domain.Entities;

namespace Application.Dtos
{
    public class ArchiveDto
    {
        public Guid Id { get; set; }

        public string? FileName { get; set; }

        public string? ContentType { get; set; }

        public long? Length { get; set; }

        public string? Path { get; set; }

        public DateTime? UploadDate { get; set; }

        public string? UserId { get; set; }

        public User? User { get; set; }

        public ArchiveDto()
        {

        }
    }
}
