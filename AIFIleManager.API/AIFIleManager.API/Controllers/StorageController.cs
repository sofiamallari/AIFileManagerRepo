using AIFileManager.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AIFileManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StorageController : ControllerBase
    {
        private readonly IFileStorageService _service;

        public StorageController(IFileStorageService service)
        {
            _service = service;
        }

        [HttpGet("getDrive")]
        public async Task<IActionResult> GetDrives() =>
            Ok(await _service.GetDrivesAsync());

        [HttpGet("getFolderList")]
        public async Task<IActionResult> GetFolders([FromQuery] string path) =>
            Ok(await _service.GetFoldersAsync(path));

        [HttpGet("getFileMetadata")]
        public async Task<IActionResult> GetFileMetadata([FromQuery] string path) =>
            Ok(await _service.GetFileMetadataAsync(path));

        [HttpDelete("deleteFile")]
        public async Task<IActionResult> DeleteFile([FromQuery] string path) =>
            Ok(new { status = await _service.DeleteFileAsync(path) });

        [HttpDelete("deleteFolder")]
        public async Task<IActionResult> DeleteFolder([FromQuery] string path) =>
            Ok(new { status = await _service.DeleteFolderAsync(path) });

        [HttpPost("moveFile")]
        public async Task<IActionResult> MoveFile([FromQuery] string source, [FromQuery] string destination) =>
            Ok(new { status = await _service.MoveFileAsync(source, destination) });

        [HttpPost("moveFolder")]
        public async Task<IActionResult> MoveFolder([FromQuery] string source, [FromQuery] string destination) =>
            Ok(new { status = await _service.MoveFolderAsync(source, destination) });
    }
}
