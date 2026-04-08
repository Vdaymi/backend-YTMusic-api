using YTMusicApi.Optimizer.Optimization;
using YTMusicApi.Optimizer.Optimization.Algorithm;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IOptimizationAlgorithm, GreedyOptimizationAlgorithm>();
builder.Services.AddScoped<IOptimizationAlgorithm, AntColonyOptimizationAlgorithm>();

builder.Services.AddScoped<IScoreEvaluator, ScoreEvaluator>();
builder.Services.AddScoped<IOptimizationOrchestrator, OptimizationOrchestrator>();

builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();