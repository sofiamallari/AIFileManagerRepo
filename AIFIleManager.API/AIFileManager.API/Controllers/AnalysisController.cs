using AIFileManager.API.DTO;
using AIFileManager.DTO;
using AIFileManager.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AIFileManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalysisController : ControllerBase
    {
        private readonly IAIService _aiService;
        private readonly IFileStorageService _fileStorageService;

        public AnalysisController(IAIService aiService, IFileStorageService fileStorageService)
        {
            _aiService = aiService;
            _fileStorageService = fileStorageService;
        }
        //PARALLEL BATCH FILE ANALYSIS
        [HttpPost("analyze-batch-parallel")]
        public async Task<IActionResult> AnalyzeBatchParallel(
            [FromBody] List<FileInfoDto> files,
            [FromQuery] int batchSize = 10)
        {
            if (files == null || !files.Any())
                return BadRequest("File list is empty.");

            var result = await _fileStorageService.AnalyzeFilesParallelBatchesAsync(
                files,
                batchSize,
                onBatchCompleted: async (batchNum) =>
                {
                    Console.WriteLine($"Batch {batchNum} completed");
                });

            return Ok(result);
        }

        [HttpPost("check-large-file")]
        public async Task<IActionResult> CheckLargeFile([FromBody] List<FileInfoDto> files)
        {
            var result = await _aiService.CheckLargeFileAsync(files);
            return Ok(result);
        }

        [HttpPost("threshold-limit")]
        public async Task<IActionResult> ThresholdLimit([FromBody] ThresholdReqDto req)
        {
            var result = await _aiService.ThresholdLimitAsync(req);
            return Ok(result);
        }

        [HttpPost("analyze-file")]
        public async Task<IActionResult> AnalyzeSingleFile([FromBody] FileInfoDto file)
        {
            var result = await _aiService.AnalyzeSingleFileAsync(file);
            return Ok(result);
        }

        [HttpPost("analyze-folder")]
        public async Task<IActionResult> AnalyzeFolder([FromBody] FolderAnalysisReqDto req)
        {
            var result = await _aiService.AnalyzeFolderAsync(req);
            return Ok(result);
        }

    }
}
