using AutoMapper;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Microsoft.Extensions.Options;
using YTMusicApi.Data.Playlist;
using YTMusicApi.Data.Track;
using YTMusicApi.Data.YouTube;
using YTMusicApi.Data;
using YTMusicApi.Model.Playlist;
using YTMusicApi.Model.Track;
using YTMusicApi.Model.YouTube;
using YTMusicApi.Orchestrator.Playlist;
using YTMusicApi.Orchestrator.Track;
using Microsoft.EntityFrameworkCore;
using CarsMarket;

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
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            services.AddScoped<ITrackOrchestrator, TrackOrchestrator>();
            services.AddScoped<ITrackRepository, TrackRepository>();
            services.AddScoped<IPlaylistOrchestrator, PlaylistOrchestrator>();
            services.AddScoped<IPlaylistRepository, PlaylistRepository>();
            services.AddScoped<IYouTubeRepository, YouTubeRepository>();

            services.AddAutoMapper(config => config.AddProfiles(new List<Profile> { new TrackDaoProfile(), new PlaylistDaoProfile() }));
            
            services.AddDbContext<SqlDbContext>(config => config.UseSqlServer(
                _configuration.GetConnectionString("DefaultConnection")));
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

        public void Configure(IApplicationBuilder app)
        {
            app.UseMiddleware<ExceptionHandlingMiddleware>();
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();

            app.UseAuthorization();
            app.UseRouting();
            app.UseEndpoints(action => action.MapControllers());
        }
    }
}
