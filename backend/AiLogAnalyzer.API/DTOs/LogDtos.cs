namespace AiLogAnalyzer.API.DTOs;

public record AnalyzeLogRequest(string LogText);

public record AnalyzeLogResponse(
    string Id,
    string LogText,
    string Issue,
    string Severity,
    string Explanation,
    string Solution,
    DateTime CreatedAt
);

public record AiAnalysisResult(
    string Issue,
    string Severity,
    string Explanation,
    string Solution
);

public record PagedResult<T>(
    IEnumerable<T> Items,
    int TotalCount,
    int Page,
    int PageSize
);
