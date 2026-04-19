using YTMusicApi.Optimizer.Optimization;
using YTMusicApi.Optimizer.Optimization.Algorithm;
using YTMusicApi.Optimizer.MessageBroker;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IOptimizationAlgorithm, GreedyOptimizationAlgorithm>();
builder.Services.AddScoped<IOptimizationAlgorithm, AntColonyOptimizationAlgorithm>();

builder.Services.AddScoped<IScoreEvaluator, ScoreEvaluator>();
builder.Services.AddScoped<IOptimizationOrchestrator, OptimizationOrchestrator>();

builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMq"));
builder.Services.AddHostedService<OptimizationCommandConsumer>();


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