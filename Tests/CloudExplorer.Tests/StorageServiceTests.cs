using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Xunit;
using CloudExplorer.Services;

namespace CloudExplorer.Tests
{
    public class StorageServiceTests
    {
        private readonly string _testRoot = Path.Combine(Path.GetTempPath(), "CloudExplorerTestRoot");
        private StorageService CreateService() => new StorageService(_testRoot);

        public StorageServiceTests()
        {
            if (!Directory.Exists(_testRoot))
                Directory.CreateDirectory(_testRoot);
        }

        [Fact]
        public async Task CreateDirAsync_CreatesDirectory()
        {
            var service = CreateService();
            var dirName = "TestDir";
            var path = await service.CreateDirAsync(string.Empty, dirName);
            Assert.True(Directory.Exists(path));
            Directory.Delete(path);
        }

        [Fact]
        public async Task GetContentAsync_ReturnsFilesAndDirs()
        {
            var service = CreateService();
            var dirName = "ContentDir";
            var fileName = "file.txt";
            var dirPath = await service.CreateDirAsync(string.Empty, dirName);
            var filePath = Path.Combine(dirPath, fileName);
            File.WriteAllText(filePath, "test");
            var (files, dirs) = await service.GetContentAsync(dirName);
            Assert.Contains(fileName, files);
            Directory.Delete(dirPath, true);
        }

        [Fact]
        public async Task RenameAsync_RenamesDirectory()
        {
            var service = CreateService();
            var dirName = "OldDir";
            var newName = "NewDir";
            var dirPath = await service.CreateDirAsync(string.Empty, dirName);
            var newPath = await service.RenameAsync(dirName, newName);
            Assert.True(Directory.Exists(newPath));
            Directory.Delete(newPath);
        }

        [Fact]
        public async Task DeleteAsync_DeletesFileAndDirectory()
        {
            var service = CreateService();
            var dirName = "DeleteDir";
            var fileName = "delete.txt";
            var dirPath = await service.CreateDirAsync(string.Empty, dirName);
            var filePath = Path.Combine(dirPath, fileName);
            File.WriteAllText(filePath, "test");
            await service.DeleteAsync(new List<string> { Path.Combine(dirName, fileName), dirName });
            Assert.False(File.Exists(filePath));
            Assert.False(Directory.Exists(dirPath));
        }
    }
}
