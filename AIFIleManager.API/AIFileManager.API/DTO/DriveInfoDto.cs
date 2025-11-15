namespace AIFileManager.DTO
{
    public class DriveInfoDto
    {
        public string DriveName { get; set; } = "";
        public double TotalSizeGB { get; set; }
        public double UsedSizeGB { get; set; }
        public double AvailableSizeGB { get; set; }
    }
}
