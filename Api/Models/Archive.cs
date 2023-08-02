using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class Archive
    {
        public int? Id { get; private set; }

        [Required]
        public string FileName { get; private set; }

        [Required]
        public string ContentType { get; private set; }

        [Range(1, 29999999)]
        public long Length { get; private set; }

        [Required]
        public string Path { get; private set; }

        public DateTime UploadDate { get; private set; }

        public Archive(string fileName, string contentType, long length, string path)
        {
            FileName = fileName;
            ContentType = contentType;
            Length = length;
            Path = path;
            UploadDate = DateTime.Now;
        }
    }
}
