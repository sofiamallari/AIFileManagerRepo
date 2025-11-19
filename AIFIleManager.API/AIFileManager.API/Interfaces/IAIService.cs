using AIFileManager.API.DTO;
using AIFileManager.DTO;

namespace AIFileManager.Interfaces
{
    public interface IAIService
    {
        // BATCH → 1 decision
        Task<DecisionDto> AnalyzeFileAsync(FileInfoDto file);           // single file fallback
        Task<AIAnalysisResultDto?> AnalyzeFolderAsync(List<FolderInfoDto> folders);
        Task<List<DecisionDto>?> AnalyzeBatchAsync(List<FileInfoDto> files);
        Task<DecisionDto?> CheckLargeFileAsync(List<FileInfoDto> files);
        Task<ThresholdRespDto?> ThresholdLimitAsync(ThresholdReqDto req);
        Task<DecisionDto?> AnalyzeSingleFileAsync(FileInfoDto file);
        Task<DecisionDto?> AnalyzeFolderAsync(FolderAnalysisReqDto req);

    }
}
