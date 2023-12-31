﻿using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Api.Models
{
    [Index(nameof(FileName), IsUnique = true)]
    public class Archive
    {
        [Key]
        public Guid Id { get; private set; }

        [Required]
        public string? FileName { get; private set; }

        [Required]
        public string? ContentType { get; private set; }

        [Range(1, 29999999)]
        public long? Length { get; private set; }

        [Required]
        public string? Path { get; private set; }

        [Required]
        public DateTime? UploadDate { get; private set; }

        [Required]
        public string? UserId { get; private set; }

        [Required]
        [JsonIgnore]
        public User? User { get; private set; }

        public Archive()
        {

        }

        public Archive(string fileName, string contentType, long length, string path, User user)
        {
            FileName = fileName;
            ContentType = contentType;
            Length = length;
            Path = path;
            UploadDate = DateTime.UtcNow;
            User = user;
            UserId = user.Id;
        }
    }
}
