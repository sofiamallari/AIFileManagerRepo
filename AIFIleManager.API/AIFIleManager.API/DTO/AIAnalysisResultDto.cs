namespace AIFileManager.DTO
{
    public class AIAnalysisResultDto
    {
        public List<string>? LargeFiles { get; set; }
        public List<string>? DuplicateFiles { get; set; }
        public List<string>? OldFiles { get; set; }
        public string Threshold { get; set; } = "";
        public List<string>? Suggestions { get; set; }
    }
}
