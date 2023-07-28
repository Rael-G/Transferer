namespace Api.Models
{
    public class Archive
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string FileName { get; set; }

        public string ContentType { get; set; }

        public long Length { get; set; }

        public string Path { get; set; }
    }
}
