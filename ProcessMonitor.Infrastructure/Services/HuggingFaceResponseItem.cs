namespace ProcessMonitor.Infrastructure.Services 
{ 
    public class HuggingFaceResponseItem
    {
        public string Label { get; set; } = string.Empty;
        public double Score { get; set; }
    }
}
