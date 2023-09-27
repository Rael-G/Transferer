using System.ComponentModel.DataAnnotations;

namespace Api.Models.ViewModels
{
    public record ArchiveViewModel
    {
        public string FileName { get; set; }
        public DateTime UploadDate { get; set; }
        public string Owner { get; set; }
    }
}
