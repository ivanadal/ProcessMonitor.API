using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProcessMonitor.API.Models;
using ProcessMonitor.API.Services;


namespace ProcessMonitor.API.Controllers.v1
{
    [ApiController]
    [Route("[controller]")]
    public class ProcessMonitorController : ControllerBase
    {
        private readonly IAnalysisService _analysisService;

        public ProcessMonitorController(IAnalysisService analysisService)
        {
            _analysisService = analysisService;
        }

       // [Authorize] // Real implementation would use JWT or OAuth    
        [HttpPost("analyze")] // POST /processmonitor/analyze
        public async Task<IActionResult> Analyze([FromBody] AnalyzeRequest request)
        {
            var result = await _analysisService.AnalyzeAsync(request.Action, request.Guideline);

            return Ok(result);
        }
    }
}
