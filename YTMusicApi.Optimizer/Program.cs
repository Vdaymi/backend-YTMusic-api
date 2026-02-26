using YTMusicApi.Optimizer.Optimization;
using YTMusicApi.Optimizer.Optimization.Algorithm;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IScoreEvaluator, ScoreEvaluator>();
builder.Services.AddTransient<IOptimizationAlgorithm, GreedyOptimizationAlgorithm>();
builder.Services.AddScoped<IOptimizationOrchestrator, OptimizationOrchestrator>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
