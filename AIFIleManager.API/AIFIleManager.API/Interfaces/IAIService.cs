using AIFileManager.DTO;

namespace AIFileManager.Interfaces
{
    public interface IAIService
    {
        Task<AIAnalysisResultDto?> AnalyzeFilesAsync(List<FileInfoDto> files);
        Task<AIAnalysisResultDto?> AnalyzeFolderAsync(List<FolderInfoDto> folders);
        Task<AIAnalysisResultDto?> OptimizeAsync(object metadata);
    }
}
