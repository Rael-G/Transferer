using Api.Data.Interfaces;

namespace Api.Data.Repositories
{
    public class LocalFileStorage : IFileStorage
    {
        private readonly string _path;

        //TODO
        //É o local file storage que vai criar o caminho /Storage/Files caso não exista?
        //Injetar o path a partir de program.cs

        public LocalFileStorage()
        {
            _path = @"C:\Users\israe\OneDrive\Documentos\Projetos\C#\Transfero\Api\Storage\Files";
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
    }
}
