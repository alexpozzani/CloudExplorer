using CloudExplorer.Services;
using CloudExplorer.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CloudExplorer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DataServiceController : ControllerBase
    {
        private readonly StorageService _storageService;

        public DataServiceController()
        {
            // Configure o caminho raiz conforme necessário
            var rootPath = Path.Combine(Directory.GetCurrentDirectory(), "StorageRoot");
            if (!Directory.Exists(rootPath))
                Directory.CreateDirectory(rootPath);
            _storageService = new StorageService(rootPath);
        }

        [HttpPost("content")]
        public async Task<IActionResult> GetContent([FromBody] Data target)
        {
            // Supondo que target["path"] contenha o caminho relativo
            var relativePath = target.ContainsKey("path") && target["path"] != null ? target["path"].ToString() : string.Empty;
            (List<string> files, List<string> dirs) = await _storageService.GetContentAsync(relativePath ?? string.Empty);

            // Retornar como lista de Data para compatibilidade com o frontend
            var fileList = files.Select(f => new Data { { "name", f }, { "type", "file" }, { "path", Path.Combine(relativePath ?? string.Empty, f) } }).ToList();
            var dirList = dirs.Select(d => new Data { { "name", d }, { "type", "dir" }, { "path", Path.Combine(relativePath ?? string.Empty, d) } }).ToList();

            return Ok(new { files = fileList, dirs = dirList });
        }


        [HttpPost("createdir")]
        public async Task<IActionResult> CreateDir([FromBody] CreateDirRequest request)
        {
            if (request?.Parent == null || string.IsNullOrWhiteSpace(request.Name))
                return BadRequest("Parâmetros inválidos");
            var parentPath = request.Parent.ContainsKey("path") && request.Parent["path"] != null ? request.Parent["path"].ToString() : string.Empty;
            var newDirPath = await _storageService.CreateDirAsync(parentPath ?? string.Empty, request.Name);
            return Ok(new Data { { "path", newDirPath }, { "name", request.Name }, { "type", "dir" } });
        }

        [HttpPost("rename")]
        public async Task<IActionResult> Rename([FromBody] RenameRequest request)
        {
            if (request?.Target == null || string.IsNullOrWhiteSpace(request.NewName))
                return BadRequest("Parâmetros inválidos");
            var targetPath = request.Target.ContainsKey("path") && request.Target["path"] != null ? request.Target["path"].ToString() : string.Empty;
            var newPath = await _storageService.RenameAsync(targetPath ?? string.Empty, request.NewName);
            return Ok(new Data { { "path", newPath }, { "name", request.NewName } });
        }

        [HttpPost("delete")]
        public async Task<IActionResult> Delete([FromBody] List<Data> targets)
        {
            if (targets == null || !targets.Any())
                return BadRequest("Nenhum alvo informado");
            var paths = targets.Select(t => t.ContainsKey("path") ? t["path"]?.ToString() ?? string.Empty : string.Empty).ToList();
            await _storageService.DeleteAsync(paths);
            return Ok(new { deleted = paths });
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFiles([FromForm] UploadRequest request)
        {
            if (request?.Parent == null || request.Files == null || request.Files.Count == 0)
                return BadRequest("Parâmetros inválidos");

            //deserialize property Parent from request as Dictionary<string, object>
            var parent = JsonConvert.DeserializeObject<Dictionary<string, object>>(request.Parent ?? "{}");

            var parentPath = parent != null && parent.ContainsKey("path") && parent["path"] != null ? parent["path"].ToString() : string.Empty;
            var uploaded = new List<string>();
            foreach (var file in request.Files)
            {
                using var stream = file.OpenReadStream();
                var filePath = await _storageService.UploadFileAsync(parentPath ?? string.Empty, file.FileName, stream);
                uploaded.Add(filePath);
            }
            return Ok(new { uploaded });
        }

        [HttpPost("download")]
        public async Task<IActionResult> DownloadFile([FromBody] Data target)
        {
            if (target == null || !target.ContainsKey("path"))
                return BadRequest("Parâmetros inválidos");
            var relativePath = target["path"]?.ToString() ?? string.Empty;
            var (stream, fileName) = await _storageService.DownloadFileAsync(relativePath);
            return File(stream, "application/octet-stream", fileName);
        }

        [HttpPost("opentree")]
        public IActionResult OpenTree([FromBody] Data data)
        {
            // TODO: Implementar integração com FluentStorage
            return Ok(new List<DataNode<Data>>());
        }
    }

    public class CreateDirRequest { public Data? Parent { get; set; } public string? Name { get; set; } }
    public class RenameRequest { public Data? Target { get; set; } public string? NewName { get; set; } }
    public class UploadRequest { public string? Parent { get; set; } public Microsoft.AspNetCore.Http.IFormFileCollection? Files { get; set; } }
}
