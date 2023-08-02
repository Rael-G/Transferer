using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Api.Models;

namespace Tests.Archives
{
    public class ArchiveTests
    {
        [Fact]
        public void MustCreateArchive() 
        {
            string name = "arquivo.txt";
            string type = "txt";
            long length = 16;
            string path = ".\\documents\\arquivo.txt";

            Archive archive = new(name, type, length, path);

            Assert.Equal(name, archive.FileName);
            Assert.Equal(type, archive.ContentType);
            Assert.Equal(length, archive.Length);
            Assert.Equal(path, archive.Path);
        }
    }
}
