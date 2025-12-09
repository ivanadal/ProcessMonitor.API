namespace ProcessMonitor.API.Models
{
    public class HuggingFaceResponseItem
    {
        public string Label { get; set; } = string.Empty;
        public double Score { get; set; }
    }
}
