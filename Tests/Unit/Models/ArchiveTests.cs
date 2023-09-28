using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Api.Models;
using Tests._Builder;
using Xunit.Sdk;

namespace Tests.Unit.Models
{
    public class ArchiveTests
    {
        [Fact]
        public void IsCreated()
        {
            string name = "arquivo.txt";
            string type = "txt";
            long length = 16;
            string path = ".\\documents\\arquivo.txt";
            User user = new User();

            Archive archive = new(name, type, length, path, user);

            Assert.Equal(name, archive.FileName);
            Assert.Equal(type, archive.ContentType);
            Assert.Equal(length, archive.Length);
            Assert.Equal(path, archive.Path);
            Assert.Equal(user, archive.User);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void EmptyOrNullFileName_IsInvalid(string invalidName)
        {
            Archive archive = new ArchiveBuilder().SetFileName(invalidName).Build();

            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(archive, new ValidationContext(archive), results, true);

            Assert.False(isValid);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void EmptyOrNullContentType_IsInvalid(string invalidType)
        {
            Archive archive = new ArchiveBuilder().SetContentType(invalidType).Build();

            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(archive, new ValidationContext(archive), results, true);

            Assert.False(isValid);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(30000001)]
        public void LengthZeroOrTooBig_IsInvalid(long invalidLength)
        {
            Archive archive = new ArchiveBuilder().SetLength(invalidLength).Build();

            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(archive, new ValidationContext(archive), results, true);

            Assert.False(isValid);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void EmptyOrNullPath_IsInvalid(string invalidPath)
        {
            Archive archive = new ArchiveBuilder().SetPath(invalidPath).Build();

            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(archive, new ValidationContext(archive), results, true);

            Assert.False(isValid);
        }

        [Fact]
        public void UploadDateIsWhenConstructorIsCalled()
        {
            var past = DateTime.Now;
            Archive archive = new ArchiveBuilder().Build();
            var future = DateTime.Now;

            Assert.True(past < archive.UploadDate && archive.UploadDate < future);
        }
    }
}