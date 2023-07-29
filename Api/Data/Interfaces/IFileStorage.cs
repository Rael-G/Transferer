namespace Api.Data.Interfaces
{
    public interface IFileStorage
    {
        //Store a file and return its path or url
        string Store(Stream file);
    }
}
