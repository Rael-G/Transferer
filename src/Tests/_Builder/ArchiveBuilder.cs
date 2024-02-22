using Api.Models;
using Application.Dtos;
using Bogus;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System.Reflection;

namespace Tests._Builder
{
    public class ArchiveBuilder
    {
        private int? _id;
        private string _fileName;
        private string _contentType;
        private long _length;
        private string _path;
        private User _user;

        private readonly Faker _faker =  new();

        public ArchiveBuilder() 
        {
            _id = null;
            _fileName = _faker.System.FileName();
            _contentType = _faker.System.MimeType();
            _length = _faker.Random.Long(1, 1024);
            _path = _faker.System.FilePath();
            _user = UserBuilder.BuildUser();
        }

        public ArchiveBuilder SetId(int id)
        {
            _id = id;
            return this;
        }

        public ArchiveBuilder SetFileName(string fileName)
        {
            _fileName = fileName;
            return this;
        }

        public ArchiveBuilder SetContentType(string contentType)
        {
            _contentType = contentType;
            return this;
        }

        public ArchiveBuilder SetLength(long length)
        {
            _length = length;
            return this;
        }

        public ArchiveBuilder SetPath(string path)
        {
            _path = path;
            return this;
        }

        public ArchiveBuilder SetUser(User user)
        {
            _user = user;
            return this;
        }

        public Archive Build()
        {
            var archive = new Archive(_fileName, _contentType, _length, _path, _user);
            if (_id != null)
            {
                archive = SetIdField(archive);
            }
            return archive;
        }

        public static List<Archive> BuildArchives(int num)
        {
            List<Archive> archives = new();
            for (int i = 0; i < num; i++)
            {
                archives.Add(new ArchiveBuilder().Build());
            }

            return archives;
        }

        public static List<ArchiveDto> BuildArchivesDto(int num)
        {
            List<ArchiveDto> archives = new();
            for (int i = 0; i < num; i++)
            {
                archives.Add(new ArchiveBuilder().BuildDto());
            }

            return archives;
        }

        private Archive SetIdField(Archive archive)
        {
            
            FieldInfo? field = typeof(Archive).GetField("id");

            field?.SetValue(archive, _id);
            return archive;
        }

        private ArchiveDto SetIdField(ArchiveDto archive)
        {

            FieldInfo? field = typeof(ArchiveDto).GetField("id");

            field?.SetValue(archive, _id);
            return archive;
        }

        public ArchiveDto BuildDto()
        {
            var archive = new ArchiveDto { FileName = _fileName, ContentType =_contentType, Length = _length, Path = _path, User = _user };
            if (_id != null)
            {
                archive = SetIdField(archive);
            }
            return archive;
        }

    }
}
