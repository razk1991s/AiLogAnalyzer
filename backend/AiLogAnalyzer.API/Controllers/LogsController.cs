using AiLogAnalyzer.API.DTOs;
using AiLogAnalyzer.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace AiLogAnalyzer.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class LogsController : ControllerBase
{
    private readonly ILogService _logService;
    private readonly ILogger<LogsController> _logger;

    public LogsController(ILogService logService, ILogger<LogsController> logger)
    {
        _logService = logService;
        _logger = logger;
    }

    /// <summary>
    /// Analyze a log entry using AI and save it to the database.
    /// </summary>
    [HttpPost("analyze")]
    [ProducesResponseType(typeof(AnalyzeLogResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Analyze(
        [FromBody] AnalyzeLogRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.LogText))
            return BadRequest(new { message = "LogText cannot be empty." });

        _logger.LogInformation("Received analyze request. Log length: {Length}", request.LogText.Length);

        var result = await _logService.AnalyzeAndSaveAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Get paginated list of analyzed logs.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AnalyzeLogResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var result = await _logService.GetLogsAsync(page, pageSize, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get a single log entry by ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AnalyzeLogResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
    {
        var result = await _logService.GetLogByIdAsync(id, cancellationToken);
        if (result is null)
            return NotFound(new { message = $"Log with id '{id}' was not found." });

        return Ok(result);
    }

    /// <summary>
    /// Delete a log entry by ID.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        await _logService.DeleteLogAsync(id, cancellationToken);
        return NoContent();
    }
}
