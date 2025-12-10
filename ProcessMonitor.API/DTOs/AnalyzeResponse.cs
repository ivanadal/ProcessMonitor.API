namespace ProcessMonitor.API.Models
{
    public record AnalyzeResponse(
     string Action,
     string Guideline,
     string Result,
     double Confidence,
     DateTime Timestamp
 );
}
