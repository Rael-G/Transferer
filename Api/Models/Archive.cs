namespace Api.Models
{
    public class Archive
    {
        public int? Id { get; private set; }

        public string FileName { get; private set; }

        public string ContentType { get; private set; }

        public long Length { get; private set; }

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
