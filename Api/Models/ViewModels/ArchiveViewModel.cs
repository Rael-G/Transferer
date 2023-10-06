using System.ComponentModel.DataAnnotations;

namespace Api.Models.ViewModels
{
    public record ArchiveViewModel
    {
        public string? Id { get; set; }
        public string? FileName { get; set; }
        public DateTime? UploadDate { get; set; }
        public string? Owner { get; set; }

        public ArchiveViewModel (Archive archive)
        {
            Id = archive.Id.ToString();
            FileName = archive.FileName;
            UploadDate = archive.UploadDate;
        }

        public static List<ArchiveViewModel> MapArchivesToViewModel(IEnumerable<Archive> archives)
        {
            List<ArchiveViewModel> viewModels = new();
            foreach (var archive in archives)
            {
                viewModels.Add(new ArchiveViewModel (archive));
            }

            return viewModels;
        }
    }
}
