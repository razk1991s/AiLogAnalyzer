using AiLogAnalyzer.API.Configuration;
using AiLogAnalyzer.API.DTOs;
using AiLogAnalyzer.API.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace AiLogAnalyzer.API.Services;

public class LogService : ILogService
{
    private readonly IMongoCollection<LogEntry> _logsCollection;
    private readonly IOpenAiService _openAiService;
    private readonly ILogger<LogService> _logger;

    public LogService(
        IMongoClient mongoClient,
        IOptions<MongoDbSettings> mongoSettings,
        IOpenAiService openAiService,
        ILogger<LogService> logger)
    {
        var settings = mongoSettings.Value;
        var database = mongoClient.GetDatabase(settings.DatabaseName);
        _logsCollection = database.GetCollection<LogEntry>(settings.LogsCollectionName);
        _openAiService = openAiService;
        _logger = logger;

        EnsureIndexes();
    }

    public async Task<AnalyzeLogResponse> AnalyzeAndSaveAsync(
        AnalyzeLogRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Analyzing log entry...");

        var analysisResult = await _openAiService.AnalyzeLogAsync(request.LogText, cancellationToken);

        var logEntry = new LogEntry
        {
            LogText = request.LogText,
            Issue = analysisResult.Issue,
            Severity = analysisResult.Severity,
            Explanation = analysisResult.Explanation,
            Solution = analysisResult.Solution,
            CreatedAt = DateTime.UtcNow
        };

        await _logsCollection.InsertOneAsync(logEntry, cancellationToken: cancellationToken);

        _logger.LogInformation("Log entry saved with ID: {Id}", logEntry.Id);

        return MapToResponse(logEntry);
    }

    public async Task<PagedResult<AnalyzeLogResponse>> GetLogsAsync(
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var skip = (page - 1) * pageSize;

        var filter = Builders<LogEntry>.Filter.Empty;
        var sort = Builders<LogEntry>.Sort.Descending(x => x.CreatedAt);

        var totalCount = await _logsCollection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);

        var items = await _logsCollection
            .Find(filter)
            .Sort(sort)
            .Skip(skip)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<AnalyzeLogResponse>(
            Items: items.Select(MapToResponse),
            TotalCount: (int)totalCount,
            Page: page,
            PageSize: pageSize
        );
    }

    public async Task<AnalyzeLogResponse?> GetLogByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<LogEntry>.Filter.Eq(x => x.Id, id);
        var entry = await _logsCollection.Find(filter).FirstOrDefaultAsync(cancellationToken);
        return entry is null ? null : MapToResponse(entry);
    }

    public async Task DeleteLogAsync(string id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<LogEntry>.Filter.Eq(x => x.Id, id);
        await _logsCollection.DeleteOneAsync(filter, cancellationToken);
        _logger.LogInformation("Deleted log entry with ID: {Id}", id);
    }

    private void EnsureIndexes()
    {
        var indexKeys = Builders<LogEntry>.IndexKeys.Descending(x => x.CreatedAt);
        var indexModel = new CreateIndexModel<LogEntry>(indexKeys);
        _logsCollection.Indexes.CreateOne(indexModel);
    }

    private static AnalyzeLogResponse MapToResponse(LogEntry entry) => new(
        Id: entry.Id ?? string.Empty,
        LogText: entry.LogText,
        Issue: entry.Issue,
        Severity: entry.Severity,
        Explanation: entry.Explanation,
        Solution: entry.Solution,
        CreatedAt: entry.CreatedAt
    );
}
