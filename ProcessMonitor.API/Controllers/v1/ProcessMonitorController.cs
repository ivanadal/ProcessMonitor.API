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
        private readonly IAnalysisRepository _repository;

        public ProcessMonitorController(IAnalysisService analysisService, IAnalysisRepository repository)
        {
            _analysisService = analysisService;
            _repository = repository;
        }

        // POST /processmonitor/analyze
        [HttpPost("analyze")] 
        public async Task<IActionResult> Analyze([FromBody] AnalyzeRequest request)
        {
            var analysisResult = await _analysisService.AnalyzeAsync(request.Action, request.Guideline);

            return Ok(analysisResult);
        }

        // GET /processmonitor/history
        [HttpGet("history")]
        public async Task<ActionResult> GetHistory()
        {
            var history = await _repository.GetHistoryAsync();

            if (history == null || !history.Any())
                return NoContent(); 

            return Ok(history);
        }
    }
}
