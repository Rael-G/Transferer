using Api.Models;
using Bogus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests._Builder
{
    public class ArchiveBuilder
    {
        private string _fileName;
        private string _contentType;
        private long _length;
        private string _path;

        private Faker _faker =  new Faker();

        public ArchiveBuilder() 
        {
            _fileName = _faker.System.FileName();
            _contentType = _faker.System.FileType();
            _length = _faker.Random.Long(1, 1024);
            _path = _faker.System.FilePath();
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

        public Archive Build()
        {
            return new Archive(_fileName, _contentType, _length, _path);
        }
    }
}
