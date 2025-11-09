namespace AIFileManager.DTO
{
    public class FileInfoDto
    {
        public string Name { get; set; } = "";
        public double SizeMB { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string Hash { get; set; } = "";
    }
}
