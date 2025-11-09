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
        /// <summary>
        /// Drive Information
        /// </summary>
        /// <returns></returns>
        [HttpGet("getDrive")]
        public async Task<IActionResult> GetDrives()
        {
            try
            {
                var drives = await _service.GetDrivesAsync();
                return Ok(drives);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error retrieving drives: {ex.Message}" });
            }
        }
        /// <summary>
        /// List of files
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        [HttpGet("getFileList")]
        public async Task<IActionResult> GetFileList([FromQuery] string path, [FromQuery] bool deepScan = false)
        {
            if (string.IsNullOrWhiteSpace(path))
                return BadRequest(new { message = "Path parameter is required." });

            path = path.Trim();

            if (!Path.IsPathRooted(path))
                return BadRequest(new { message = "Please provide a valid absolute path (e.g., C:\\\\Users\\\\User\\\\Documents)" });

            try
            {
                var result = await _service.GetFilesAsync(path, deepScan);
                return Ok(result);
            }
            catch (DirectoryNotFoundException)
            {
                return NotFound(new { message = $"Directory not found: {path}" });
            }
            catch (UnauthorizedAccessException)
            {
                return StatusCode(403, new { message = "Access denied. Please check folder permissions." });
            }
            catch (IOException ex)
            {
                return BadRequest(new { message = $"I/O error accessing files: {ex.Message}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Unexpected error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Folder Listing with Validation
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        [HttpGet("getFolderList")]
        public async Task<IActionResult> GetFolders([FromQuery] string path, [FromQuery] bool deepScan = false)
        {
            if (string.IsNullOrWhiteSpace(path))
                return BadRequest(new { message = "Path parameter is required." });

            path = path.Trim();

            if (!Path.IsPathRooted(path))
                return BadRequest(new { message = "Please provide a valid absolute path (e.g., C:\\\\Users\\\\User\\\\Downloads)" });

            try
            {
                var result = await _service.GetFoldersAsync(path, deepScan);
                return Ok(result);
            }
            catch (DirectoryNotFoundException)
            {
                return NotFound(new { message = $"Directory not found: {path}" });
            }
            catch (IOException ex)
            {
                return BadRequest(new { message = $"I/O error accessing folder: {ex.Message}" });
            }
            catch (UnauthorizedAccessException)
            {
                return StatusCode(403, new { message = "Access denied. Please check folder permissions." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Unexpected error: {ex.Message}" });
            }
        }
        /// <summary>
        /// File Metadata
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        [HttpGet("getFileMetadata")]
        public async Task<IActionResult> GetFileMetadata([FromQuery] string path, bool deepScan = false)
        {
            if (string.IsNullOrWhiteSpace(path))
                return BadRequest(new { message = "Path parameter is required." });

            path = path.Trim();

            if (!Path.IsPathRooted(path))
                return BadRequest(new { message = "Please provide a valid absolute path (e.g., C:\\\\Users\\\\User\\\\Documents)" });

            try
            {
                var files = await _service.GetFileMetadataAsync(path, deepScan);
                return Ok(files);
            }
            catch (DirectoryNotFoundException)
            {
                return NotFound(new { message = $"Directory not found: {path}" });
            }
            catch (IOException ex)
            {
                return BadRequest(new { message = $"I/O error reading metadata: {ex.Message}" });
            }
            catch (UnauthorizedAccessException)
            {
                return StatusCode(403, new { message = "Access denied. Please check folder permissions." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Unexpected error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Deletes File
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        [HttpDelete("deleteFile")]
        public async Task<IActionResult> DeleteFile([FromQuery] string path, [FromQuery] bool permanent = false)
        {
            if (string.IsNullOrWhiteSpace(path))
                return BadRequest(new { message = "Path parameter is required." });

            try
            {
                var result = await _service.DeleteFileAsync(path, permanent);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error deleting file: {ex.Message}" });
            }
        }

        /// <summary>
        /// Deletes Folder
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>

        [HttpDelete("deleteFolder")]
        public async Task<IActionResult> DeleteFolder([FromQuery] string path, [FromQuery] bool permanent = false)
        {
            if (string.IsNullOrWhiteSpace(path))
                return BadRequest(new { message = "Path parameter is required." });

            try
            {
                var result = await _service.DeleteFolderAsync(path, permanent);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error deleting folder: {ex.Message}" });
            }
        }

        /// <summary>
        /// Moves File
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        [HttpPost("moveFile")]
        public async Task<IActionResult> MoveFile([FromQuery] string source, [FromQuery] string destination)
        {
            if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(destination))
                return BadRequest(new { message = "Both source and destination paths are required." });

            try
            {
                var result = await _service.MoveFileAsync(source.Trim(), destination.Trim());
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error moving file: {ex.Message}" });
            }
        }

        /// <summary>
        /// Moves Folder
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        [HttpPost("moveFolder")]
        public async Task<IActionResult> MoveFolder([FromQuery] string source, [FromQuery] string destination)
        {
            if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(destination))
                return BadRequest(new { message = "Both source and destination paths are required." });

            try
            {
                var result = await _service.MoveFolderAsync(source.Trim(), destination.Trim());
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error moving folder: {ex.Message}" });
            }
        }

    }
}
