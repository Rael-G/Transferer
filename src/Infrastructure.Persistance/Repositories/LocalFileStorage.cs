using Domain.Interfaces.Repositories;

namespace Api.Data.Repositories
{
    public class LocalFileStorage : IFileStorage
    {
        private readonly string _path;

        public LocalFileStorage(string path)
        {
            _path = path;

            if (!Directory.Exists(_path))
            {
                Directory.CreateDirectory(_path);
            }
        }

        public async Task<Stream?> GetByPathAsync(string fullPath)
        {
            if (File.Exists(fullPath))
            {
                byte[] file = await File.ReadAllBytesAsync(fullPath);
                return new MemoryStream(file);
            }
            return null;
        }

        public async Task<string> StoreAsync(Stream file)
        {
            Guid fileName = Guid.NewGuid();
            string fullPath = Path.Combine(_path, fileName.ToString());
            using (FileStream fileStream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return fullPath;
        }

        public async Task<bool> DeleteAsync(string path)
        {
            bool result;
            try
            {
                await Task.Run(() => File.Delete(path));
            }
            finally 
            { 
                result = !File.Exists(path);
            }

            return result;
        }
    }
}
