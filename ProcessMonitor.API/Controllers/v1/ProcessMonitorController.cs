using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProcessMonitor.API.Models;

namespace ProcessMonitor.API.Controllers.v1
{
    [ApiController]
    [Route("[controller]")]
    public class ProcessMonitorController : ControllerBase
    {

        [Authorize] // Real implementation would use JWT or OAuth    
        [HttpPost("analyze")] // POST /processmonitor/analyze
        public IActionResult Analyze([FromBody] AnalyzeRequest request)
        {
            var result = new AnalyzeResult(
                request.Action,
                request.Guideline,
                Analysis: $"Analysis performed for action '{request.Action}' using guideline '{request.Guideline}'."
            );

            return Ok(result);
        }
    }
}
