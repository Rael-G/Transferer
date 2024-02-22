using Domain.Entities;

namespace Application.Dtos
{
    public class ArchiveDto
    {
        public Guid Id { get; private set; }

        public string? FileName { get; private set; }

        public string? ContentType { get; private set; }

        public long? Length { get; private set; }

        public string? Path { get; private set; }

        public DateTime? UploadDate { get; private set; }

        public string? UserId { get; private set; }

        public User? User { get; private set; }

        public ArchiveDto()
        {

        }
    }
}
