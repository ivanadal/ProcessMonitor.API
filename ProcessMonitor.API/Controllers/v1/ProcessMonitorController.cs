using Microsoft.AspNetCore.Mvc;
using ProcessMonitor.API.DTOs;
using ProcessMonitor.API.Models;
using ProcessMonitor.Domain.Entities;
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

        // GET /processmonitor/history?page=1&pageSize=10
        [HttpGet("history")]
        public async Task<ActionResult> GetHistory([FromQuery] HistoryQuery query)
        {
            var result = await _repository.GetPagedHistoryAsync(query.Page, query.PageSize);
            var response = new HistoryQueryResponse<Analysis>
            {
                Page = query.Page,
                PageSize = query.PageSize,
                TotalPages = result.TotalPages,
                TotalItems = result.TotalItems,
                Items = (List<Analysis>)result.Items
            };

            return Ok(response);
        }

        // GET /summary
        [HttpGet("summary")]
        public async Task<ActionResult> GetSummary()
        {
            var result = await _repository.GetSummaryAsync();          

            return Ok(result);
        }
    }
}
