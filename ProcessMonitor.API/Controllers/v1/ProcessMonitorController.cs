using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProcessMonitor.API.DTOs;
using ProcessMonitor.Domain.Entities;
using ProcessMonitor.Domain.Exceptions;
using ProcessMonitor.Domain.Interfaces;


namespace ProcessMonitor.API.Controllers.v1
{
    [ApiController]
    [Route("v1/[controller]")]
    public class ProcessMonitorController : ControllerBase
    {
        private readonly IAnalysisService _analysisService;
        private readonly IAnalysisRepository _repository;
        private readonly ILogger<ProcessMonitorController> _logger;

        public ProcessMonitorController(IAnalysisService analysisService, IAnalysisRepository repository, ILogger<ProcessMonitorController> logger)
        {
            _analysisService = analysisService;
            _repository = repository;
            _logger = logger;
        }

        // POST /processmonitor/analyze
        [HttpPost("analyze")]
        public async Task<IActionResult> Analyze([FromBody] AnalyzeRequest request, [FromServices] IValidator<AnalyzeRequest> validator)
        {
            var result = await validator.ValidateAsync(request);

            if (!result.IsValid)
            {
                var message = string.Join("; ", result.Errors.Select(e => e.ErrorMessage));
                return BadRequest(new { Message = "Validation failed", Details = message });
            }

            var analysisResult = await _analysisService.AnalyzeAsync(request.Action, request.Guideline);
            return Ok(analysisResult);          

        }

        // GET /processmonitor/history?page=1&pageSize=10
        [HttpGet("history")]
        public async Task<ActionResult> GetHistory([FromQuery] HistoryQuery query, [FromServices] IValidator<HistoryQuery> validator)
        {
            var result = await validator.ValidateAsync(query);

            if (!result.IsValid)
            {
                var message = string.Join("; ", result.Errors.Select(e => e.ErrorMessage));
                return BadRequest(new { Message = "Validation failed", Details = message });
            }
         
            var historyResult = await _repository.GetPagedHistoryAsync(query.Page, query.PageSize);
            var response = new HistoryQueryResponse<Analysis>
            {
                Page = query.Page,
                PageSize = query.PageSize,
                TotalPages = historyResult.TotalPages,
                TotalItems = historyResult.TotalItems,
                Items = (List<Analysis>)historyResult.Items
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
