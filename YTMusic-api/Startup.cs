using AutoMapper;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using YTMusicApi.Data;
using YTMusicApi.Data.MessageBroker;
using YTMusicApi.Data.Optimization;
using YTMusicApi.Data.Playlist;
using YTMusicApi.Data.PlaylistTrack;
using YTMusicApi.Data.Track;
using YTMusicApi.Data.User;
using YTMusicApi.Data.UserPlaylist;
using YTMusicApi.Data.YouTube;
using YTMusicApi.Extensions;
using YTMusicApi.Middleware;
using YTMusicApi.Model.Auth;
using YTMusicApi.Model.Playlist;
using YTMusicApi.Model.PlaylistTrack;
using YTMusicApi.Model.Track;
using YTMusicApi.Model.User;
using YTMusicApi.Model.UserPlaylist;
using YTMusicApi.Model.YouTube;
using YTMusicApi.Model.MessageBroker;
using YTMusicApi.Model.Optimization;
using YTMusicApi.Orchestrator.Integration;
using YTMusicApi.Orchestrator.Optimization;
using YTMusicApi.Orchestrator.Playlist;
using YTMusicApi.Orchestrator.PlaylistTrack;
using YTMusicApi.Orchestrator.Track;
using YTMusicApi.Orchestrator.User;
using YTMusicApi.Orchestrator.UserPlaylist;
using YTMusicApi.Platform;
using YTMusicApi.Platform.Client;
using YTMusicApi.Platform.Email;
using YTMusicApi.Platform.MessageBroker;
using YTMusicApi.Platform.Jwt;

namespace YTMusicApi
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddPerUserRateLimiting();
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            services.AddHttpContextAccessor();

            services.Configure<JwtOptions>(_configuration.GetSection("JwtOptions"));
            services.Configure<ClientSettings>(_configuration.GetSection(ClientSettings.SectionName));
            services.Configure<EmailSettings>(_configuration.GetSection(EmailSettings.SectionName));
            services.Configure<RabbitMqSettings>(_configuration.GetSection("RabbitMq"));
            services.AddApiAuthentication(_configuration);
            
            var clientSettings = _configuration.GetSection(ClientSettings.SectionName).Get<ClientSettings>();
            
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.WithOrigins(clientSettings?.ClientBaseUrl ?? "http://localhost:5173");
                    policy.AllowAnyHeader();
                    policy.AllowAnyMethod();
                    policy.AllowCredentials();
                    policy.WithExposedHeaders("x-ratelimit-reset");
                });
            });


            services.AddScoped<IYouTubeRepository, YouTubeRepository>();

            services.AddScoped<ITrackOrchestrator, TrackOrchestrator>();
            services.AddScoped<ITrackRepository, TrackRepository>();

            services.AddScoped<IPlaylistOrchestrator, PlaylistOrchestrator>();
            services.AddScoped<IPlaylistRepository, PlaylistRepository>();

          
            services.AddScoped<IPlaylistTrackRepository, PlaylistTrackRepository>();
            services.AddScoped<IPlaylistTrackOrchestrator, PlaylistTrackOrchestrator>();

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserOrchestrator, UserOrchestrator>();
            
            services.AddScoped<IUserPlaylistRepository, UserPlaylistRepository>();
            services.AddScoped<IUserPlaylistOrchestrator, UserPlaylistOrchestrator>();
            
            services.AddScoped<IOptimizationTaskOrchestrator, OptimizationTaskOrchestrator>();
            services.AddScoped<IOptimizationRepository, OptimizationRepository>();

            services.AddScoped<IJwtProvider, JwtProvider>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IEmailSender, SmtpEmailSender>();

            services.AddSingleton<IMessagePublisher, RabbitMqPublisher>();
            
            services.AddStackExchangeRedisCache(options => {
                options.Configuration = _configuration["Redis:ConnectionString"];
                options.InstanceName = "YTMusicApi_";
            });

            services.AddAutoMapper(config => config.AddProfiles(new List<Profile> 
            {   
                new TrackDaoProfile(), 
                new PlaylistDaoProfile(),
                new PlaylistTrackDaoProfile(),
                new UserDaoProfile(),
                new UserPlaylistDaoProfile(),
                new TrackOptimizationDtoProfile(),
                new PlaylistSettingDaoProfile(),
                new OptimizationTaskDaoProfile(),
                new OutboxMessageDaoProfile()
            }));
            
            services.AddHostedService<OutboxMessageRelayService>();
            services.AddHostedService<OptimizationResultConsumer>();
            
            ConfigureDb(services);
            ConfigureYouTubeService(services);
            
            services.AddHealthChecks().AddDbContextCheck<SqlDbContext>("Database", tags: new[] { "db" });
            
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseMiddleware<ExceptionHandlingMiddleware>();

            app.UseForwardedHeaders();

            app.UseSwagger();
            app.UseSwaggerUI();
            
            //app.UseHttpsRedirection();

            app.UseRouting();
            app.UseCors();

            app.UseCookiePolicy(new CookiePolicyOptions
            {
                MinimumSameSitePolicy = SameSiteMode.None,
                Secure = CookieSecurePolicy.Always,
                HttpOnly = HttpOnlyPolicy.Always,
            });
            app.UseAuthentication();
            app.UseAuthorization();

            using (var scope = app.ApplicationServices.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<SqlDbContext>();
            //    db.Database.Migrate();
            }

            app.UsePerUserRateLimiting();
            app.UseMiddleware<RateLimitResetMiddleware>();

            app.UseEndpoints(endpoints => 
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health", new HealthCheckOptions
                {
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
            });
        }

        protected virtual void ConfigureDb(IServiceCollection services)
        {
            services.AddDbContext<SqlDbContext>(config => config.UseNpgsql(
                _configuration.GetConnectionString("DefaultConnection")));
        }

        protected virtual void ConfigureYouTubeService(IServiceCollection services)
        {
            services.Configure<YouTubeSettings>(
            _configuration.GetSection("YouTube"));

            services.AddSingleton<YouTubeService>(sp =>
            {
                var ytSettings = sp.GetRequiredService<IOptions<YouTubeSettings>>().Value;
                return new YouTubeService(new BaseClientService.Initializer
                {
                    ApiKey = ytSettings.ApiKey,
                    ApplicationName = ytSettings.ApplicationName
                });
            });
        }
    }
}
