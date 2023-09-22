﻿using Api.Data.Interfaces;

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

        public Stream? GetByPath(string fullPath)
        {
            if (File.Exists(fullPath))
            {
                byte[] file = File.ReadAllBytes(fullPath);
                return new MemoryStream(file);
            }
            return null;
        }

        public string Store(Stream file)
        {
            Guid fileName = Guid.NewGuid();
            string fullPath = Path.Combine(_path, fileName.ToString());
            using (FileStream fileStream = new FileStream(fullPath, FileMode.Create))
            {
                file.CopyTo(fileStream);
            }

            return fullPath;
        }

        public bool Delete(string path)
        {
            bool result;
            try
            {
                File.Delete(path);
            }
            finally 
            { 
                result = !File.Exists(path);
            }

            return result;
        }
    }
}
