using AiLogAnalyzer.API.Configuration;
using AiLogAnalyzer.API.Services;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// ── Configuration ──────────────────────────────────────────────────────────
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection(MongoDbSettings.SectionName));

builder.Services.Configure<OpenAiSettings>(
    builder.Configuration.GetSection(OpenAiSettings.SectionName));

// ── MongoDB ────────────────────────────────────────────────────────────────
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = builder.Configuration
        .GetSection(MongoDbSettings.SectionName)
        .Get<MongoDbSettings>()!;
    return new MongoClient(settings.ConnectionString);
});

// ── HttpClient ─────────────────────────────────────────────────────────────
builder.Services.AddHttpClient<IOpenAiService, OpenAiService>();

// ── Application Services ───────────────────────────────────────────────────
builder.Services.AddScoped<ILogService, LogService>();

// ── Controllers & API ──────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "AiLogAnalyzer API",
        Version = "v1",
        Description = "AI-powered application log analysis API"
    });

});

// ── CORS ───────────────────────────────────────────────────────────────────
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? ["http://localhost:4200"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// ── Build ──────────────────────────────────────────────────────────────────
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "AiLogAnalyzer API v1"));
}

app.UseHttpsRedirection();
app.UseCors("AllowAngular");
app.UseAuthorization();
app.MapControllers();

app.Run();