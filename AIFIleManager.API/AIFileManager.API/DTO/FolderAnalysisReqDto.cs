using AIFileManager.DTO;

namespace AIFileManager.API.DTO
{
    public class FolderAnalysisReqDto
    {
        public List<FileInfoDto> Files { get; set; }
        public double? TotalDiskMB { get; set; }
        public double? FreeDiskMB { get; set; }
    }
}
