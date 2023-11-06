﻿namespace Api.Models.ViewModels
{
    public record ArchiveViewModel
    {
        public string Id { get; set; }
        public string FileName { get; set; }
        public DateTime? UploadDate { get; set; }
        public string Owner { get; set; }   

        public ArchiveViewModel (string id, string fileName, DateTime? uploadDate, string owner)
        {
            Id = id;
            FileName = fileName;
            UploadDate = uploadDate;
            Owner = owner;
        }

        public static List<ArchiveViewModel> MapToViewModel(IEnumerable<Archive> archives)
        {
            List<ArchiveViewModel> viewModels = new();
            foreach (var archive in archives)
            {
                viewModels.Add(MapToViewModel(archive));
            }

            return viewModels;
        }

        public static ArchiveViewModel MapToViewModel(Archive archive)
        {
            if (archive.User != null)
                return new(archive.Id.ToString(), archive.FileName, archive.UploadDate, archive.User.UserName);

            return new(archive.Id.ToString(), archive.FileName, archive.UploadDate, "aaaa");
        }
    }
}
