using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;
using FluentStorage;
using FluentStorage.Blobs;

namespace CloudExplorer.Services
{
    public class StorageService
    {
        private readonly IBlobStorage _storage;
        private readonly string _rootPath;

        public StorageService(string rootPath)
        {
            _rootPath = rootPath;
            _storage = StorageFactory.Blobs.DirectoryFiles(rootPath);
        }

        public async Task<(List<string> Files, List<string> Dirs)> GetContentAsync(string relativePath)
        {
            var path = Path.Combine(_rootPath, relativePath ?? string.Empty);
            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException($"Diretório não encontrado: {path}");

            var files = await Task.Run(() => Directory.GetFiles(path).Select(f => Path.GetFileName(f) ?? string.Empty).ToList());
            var dirs = await Task.Run(() => Directory.GetDirectories(path).Select(d => Path.GetFileName(d) ?? string.Empty).ToList());
            return (files, dirs);
        }

        public async Task<string> CreateDirAsync(string parentRelativePath, string name)
        {
            var parentPath = Path.Combine(_rootPath, parentRelativePath ?? string.Empty);
            var newDirPath = Path.Combine(parentPath, name);
            await Task.Run(() => Directory.CreateDirectory(newDirPath));
            return newDirPath;
        }

        public async Task<string> RenameAsync(string targetRelativePath, string newName)
        {
            var targetPath = Path.Combine(_rootPath, targetRelativePath ?? string.Empty);
            var parentDir = Path.GetDirectoryName(targetPath);
            var newPath = Path.Combine(parentDir ?? _rootPath, newName);
            await Task.Run(() => Directory.Move(targetPath, newPath));
            return newPath;
        }

        public async Task DeleteAsync(List<string> relativePaths)
        {
            foreach (var relPath in relativePaths)
            {
                var path = Path.Combine(_rootPath, relPath);
                if (File.Exists(path))
                    await Task.Run(() => File.Delete(path));
                else if (Directory.Exists(path))
                    await Task.Run(() => Directory.Delete(path, true));
            }
        }

        public async Task<string> UploadFileAsync(string parentRelativePath, string fileName, Stream fileStream)
        {
            var parentPath = Path.Combine(_rootPath, parentRelativePath ?? string.Empty);
            var filePath = Path.Combine(parentPath, fileName);
            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                await fileStream.CopyToAsync(fs);
            }
            return filePath;
        }

        public async Task<(Stream FileStream, string FileName)> DownloadFileAsync(string relativePath)
        {
            var filePath = Path.Combine(_rootPath, relativePath ?? string.Empty);
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Arquivo não encontrado: {filePath}");
            var stream = await Task.Run(() => (Stream)new FileStream(filePath, FileMode.Open, FileAccess.Read));
            var fileName = Path.GetFileName(filePath) ?? string.Empty;
            return (stream, fileName);
        }
    }
}
