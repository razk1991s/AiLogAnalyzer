using AiLogAnalyzer.API.DTOs;

namespace AiLogAnalyzer.API.Services;

public interface ILogService
{
    Task<AnalyzeLogResponse> AnalyzeAndSaveAsync(AnalyzeLogRequest request, CancellationToken cancellationToken = default);
    Task<PagedResult<AnalyzeLogResponse>> GetLogsAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<AnalyzeLogResponse?> GetLogByIdAsync(string id, CancellationToken cancellationToken = default);
    Task DeleteLogAsync(string id, CancellationToken cancellationToken = default);
}
