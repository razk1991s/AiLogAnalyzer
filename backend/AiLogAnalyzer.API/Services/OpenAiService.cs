using System.Text;
using System.Text.Json;
using AiLogAnalyzer.API.Configuration;
using AiLogAnalyzer.API.DTOs;
using Microsoft.Extensions.Options;

namespace AiLogAnalyzer.API.Services;

public class OpenAiService : IOpenAiService
{
    private readonly HttpClient _httpClient;
    private readonly OpenAiSettings _settings;
    private readonly ILogger<OpenAiService> _logger;

    private const string OpenAiChatEndpoint = "https://api.openai.com/v1/chat/completions";

    public OpenAiService(
        HttpClient httpClient,
        IOptions<OpenAiSettings> settings,
        ILogger<OpenAiService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;

        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.ApiKey}");
    }

    public async Task<AiAnalysisResult> AnalyzeLogAsync(string logText, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Sending log to OpenAI for analysis. Log length: {Length}", logText.Length);

        var systemPrompt = """
            You are an expert software engineer specializing in log analysis and debugging.
            Analyze the provided application log and return a structured JSON response with exactly these fields:
            - issue: A short title describing the issue (max 10 words)
            - severity: One of: "critical", "high", "medium", "low", "info"
            - explanation: A clear explanation of what the log means and what caused it (2-4 sentences)
            - solution: A single string with concrete steps a developer should take, using newlines to separate steps. Do NOT return an array.
            
            Respond ONLY with a valid JSON object. No markdown, no code blocks, no extra text.
            """;

        var requestBody = new
        {
            model = _settings.Model,
            max_tokens = _settings.MaxTokens,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = $"Analyze this log:\n\n{logText}" }
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(OpenAiChatEndpoint, content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("OpenAI API error: {StatusCode} - {Body}", response.StatusCode, errorBody);
            throw new InvalidOperationException($"OpenAI API returned {response.StatusCode}");
        }

        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(responseJson);

        var aiText = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? string.Empty;

        _logger.LogDebug("OpenAI raw response: {Response}", aiText);

        return ParseAiResponse(aiText);
    }

    private AiAnalysisResult ParseAiResponse(string aiText)
    {
        try
        {
            using var doc = JsonDocument.Parse(aiText);
            var root = doc.RootElement;

            return new AiAnalysisResult(
                Issue: root.GetProperty("issue").GetString() ?? "Unknown Issue",
                Severity: root.GetProperty("severity").GetString() ?? "medium",
                Explanation: root.GetProperty("explanation").GetString() ?? string.Empty,
                Solution: ReadStringOrArray(root.GetProperty("solution"))
            );
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse AI response as JSON. Raw: {Raw}", aiText);
            return new AiAnalysisResult(
                Issue: "Parse Error",
                Severity: "low",
                Explanation: "Could not parse AI response. Raw output returned.",
                Solution: aiText
            );
        }
    }

    /// <summary>
    /// OpenAI sometimes returns solution as a string, sometimes as a JSON array.
    /// This handles both cases gracefully.
    /// </summary>
    private static string ReadStringOrArray(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.String)
            return element.GetString() ?? string.Empty;

        if (element.ValueKind == JsonValueKind.Array)
            return string.Join("\n", element.EnumerateArray()
                .Select(e => e.GetString() ?? string.Empty)
                .Where(s => !string.IsNullOrWhiteSpace(s)));

        return element.ToString();
    }
}