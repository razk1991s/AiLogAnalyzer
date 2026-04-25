using AiLogAnalyzer.API.DTOs;

namespace AiLogAnalyzer.API.Services;

public interface IOpenAiService
{
    Task<AiAnalysisResult> AnalyzeLogAsync(string logText, CancellationToken cancellationToken = default);
}
