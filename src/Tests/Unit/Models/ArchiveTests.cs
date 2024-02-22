using Domain.Entities;
using System.ComponentModel.DataAnnotations;
using Tests._Builder;

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

            Archive archive = new Archive(name, type, length, path, user);

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

            Assert.False(archive.IsFileNameValid());
        }

        [Theory]
        [InlineData("archive")]
        [InlineData("archiveName.txt")]
        public void FilledFileName_IsValid(string validName)
        {
            Archive archive = new ArchiveBuilder().SetFileName(validName).Build();

            Assert.True(archive.IsFileNameValid());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void EmptyOrNullContentType_IsInvalid(string invalidType)
        {
            Archive archive = new ArchiveBuilder().SetContentType(invalidType).Build();

            Assert.False(archive.IsContentTypeValid());
        }

        [Fact]
        public void FilledContentType_IsValid()
        {
            Archive archive = new ArchiveBuilder().Build();

            Assert.True(archive.IsContentTypeValid());
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-100)]
        public void LengthLessThanOne_IsInvalid(long invalidLength)
        {
            Archive archive = new ArchiveBuilder().SetLength(invalidLength).Build();

            Assert.False(archive.IsLengthValid());
        }

        [Theory]
        [InlineData(1)]
        [InlineData(100)]
        public void LengthGreaterThanOne_IsValid(long validLength)
        {
            Archive archive = new ArchiveBuilder().SetLength(validLength).Build();

            Assert.True(archive.IsLengthValid());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void EmptyOrNullPath_IsInvalid(string invalidPath)
        {
            Archive archive = new ArchiveBuilder().SetPath(invalidPath).Build();

            Assert.False(archive.IsPathValid());
        }

        [Theory]
        [InlineData(".")]
        [InlineData("/home/user/archive.txt")]
        public void FilledPath_IsInvalid(string validPath)
        {
            Archive archive = new ArchiveBuilder().SetPath(validPath).Build();

            Assert.True(archive.IsPathValid());
        }

        [Fact]
        public void UploadDateWhenConstructorIsCalled()
        {
            var past = DateTime.UtcNow;
            Archive archive = new ArchiveBuilder().Build();
            var future = DateTime.UtcNow;

            Assert.True(past < archive.UploadDate && archive.UploadDate < future);
        }
    }
}