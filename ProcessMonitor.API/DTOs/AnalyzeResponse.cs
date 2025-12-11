namespace ProcessMonitor.API.DTOs
{
    public record AnalyzeResponse(
     string Action,
     string Guideline,
     string Result,
     double Confidence,
     DateTime Timestamp
 );
}
