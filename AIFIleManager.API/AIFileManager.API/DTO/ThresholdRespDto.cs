namespace AIFileManager.API.DTO
{
    public class ThresholdRespDto
    {
        public string Action { get; set; }
        public string? Destination { get; set; }
        public double ThresholdMB { get; set; }
        public string Reason { get; set; }
    }


}
