namespace AIFileManager.DTO
{
    public class AnalysisFileInfoDto
    {
        public string Name { get; set; }
        public string Reason { get; set; }
        public string Hash { get; set; }
    }

    public class AIAnalysisResultDto
    {
        public List<AnalysisFileInfoDto> LargeFiles { get; set; }
        public List<AnalysisFileInfoDto> DuplicateFiles { get; set; }
        public List<AnalysisFileInfoDto> OldFiles { get; set; }
        public List<string> Suggestions { get; set; }
    }

}
