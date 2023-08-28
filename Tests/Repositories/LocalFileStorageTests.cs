using Api.Data.Interfaces;
using Api.Data.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Repositories
{
    public class LocalFileStorageTests
    {
        private string _tempPath;
        private LocalFileStorage _fileStorage;

        public LocalFileStorageTests() 
        {
            _tempPath = Path.GetTempPath();
            _fileStorage = new LocalFileStorage(_tempPath);
        }

        [Fact]
        public void GetByPath_FileDontExists_ReturnsNull()
        {
            var tempFilePath = Path.Combine(_tempPath, "testfile.txt");

            using var resultStream = _fileStorage.GetByPath(tempFilePath);

            Assert.Null(resultStream);
        }

        [Fact]
        public void GetByPath_FileExists_ReturnStream()
        {
            var tempFilePath = Path.Combine(_tempPath, "testfile.txt");

            File.WriteAllText(tempFilePath, "Test content");

            long originalLength;
            using (var stream = File.OpenRead(tempFilePath))
            {
                originalLength = stream.Length;
            }

            using (var resultStream = _fileStorage.GetByPath(tempFilePath))
            {
                Assert.NotNull(resultStream);
                Assert.True(resultStream.Length == originalLength);
            }

            File.Delete(tempFilePath);
        }

        [Fact]
        public void Store_Allways_ReturnsPath()
        {
            Stream stream = new MemoryStream(new byte[] { 255, 0, 255, 0 });

            string resultPath = _fileStorage.Store(stream);

            var filePath = Path.Combine(_tempPath, resultPath);

            Assert.True(File.Exists(filePath));

            using (var fileStream = File.OpenRead(filePath))
            {
                Assert.True(fileStream.Length == stream.Length);
            }

            File.Delete(filePath);
        }

        [Fact]
        public void Delete_FileWasDeleted_ReturnsTrue()
        {
            var tempFilePath = Path.Combine(_tempPath, "testfile.txt");
            File.WriteAllText(tempFilePath, "Test content");

            bool deletionResult;
            bool deletionSuccessful = true;
            try
            {
                deletionResult = _fileStorage.Delete(tempFilePath);
            }
            finally 
            {
                if (File.Exists(tempFilePath))
                {
                    deletionSuccessful = false;
                    File.Delete(tempFilePath);
                }
            }

            Assert.True(deletionResult);
            Assert.True(deletionSuccessful);
        }
    }
}
