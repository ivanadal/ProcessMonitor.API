using Microsoft.AspNetCore.Mvc;
using ProcessMonitor.API.Models;
using ProcessMonitor.Domain.Interfaces;


namespace ProcessMonitor.API.Controllers.v1
{
    [ApiController]
    [Route("v1/[controller]")]
    public class ProcessMonitorController : ControllerBase
    {
        private readonly IAnalysisService _analysisService;

        public ProcessMonitorController(IAnalysisService analysisService)
        {
            _analysisService = analysisService;
        }

        // POST /processmonitor/analyze
        [HttpPost("analyze")] 
        public async Task<IActionResult> Analyze([FromBody] AnalyzeRequest request)
        {
            var analysisResult = await _analysisService.AnalyzeAsync(request.Action, request.Guideline);

            return Ok(analysisResult);
        }


    }
}
