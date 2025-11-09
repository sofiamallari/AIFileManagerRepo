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

        public AnalysisController(IAIService aiService)
        {
            _aiService = aiService;
        }

        [HttpPost("analyzeFiles")]
        public async Task<IActionResult> AnalyzeFiles([FromBody] List<FileInfoDto> files) =>
            Ok(await _aiService.AnalyzeFilesAsync(files));

        [HttpPost("analyzeFolder")]
        public async Task<IActionResult> AnalyzeFolder([FromBody] List<FolderInfoDto> folders) =>
            Ok(await _aiService.AnalyzeFolderAsync(folders));

        [HttpPost("optimize")]
        public async Task<IActionResult> Optimize([FromBody] object metadata) =>
            Ok(await _aiService.OptimizeAsync(metadata));
    }
}
