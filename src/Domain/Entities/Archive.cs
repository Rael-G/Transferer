namespace Domain.Entities
{
    public class Archive
    {
        public Guid Id { get; private set; } = new Guid();

        public string FileName { get; private set; }

        public string ContentType { get; private set; }

        public long Length { get; private set; }

        public string Path { get; private set; }

        public DateTime UploadDate { get; private set; } = DateTime.UtcNow;

        public string UserId { get; private set; }

        public User User { get; private set; }

        public Archive()
        {
            
        }

        public Archive(string fileName, string contentType, long length, string path, User user)
        {
            FileName = fileName;
            ContentType = contentType;
            Length = length;
            Path = path;
            User = user;
            UserId = user.Id;
        }

        public bool IsFileNameValid()
        {
            return !string.IsNullOrWhiteSpace(FileName);
        }

        public bool IsContentTypeValid()
        {
            return !string.IsNullOrWhiteSpace(ContentType);
        }

        public bool IsLengthValid()
        {
            return Length > 0;
        }

        public bool IsPathValid()
        {
            return !string.IsNullOrWhiteSpace(Path);
        }
    }
}
