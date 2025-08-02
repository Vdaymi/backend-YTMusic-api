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
using YTMusicApi.Data.PlaylistTrack;
using YTMusicApi.Model.PlaylistTrack;
using YTMusicApi.Data.User;
using YTMusicApi.Orchestrator.PlaylistTrack;
using YTMusicApi.Model.User;
using YTMusicApi.Orchestrator.User;
using YTMusicApi.Model.Auth;
using YTMusicApi.Platform.Jwt;
using YTMusicApi.Platform;
using YTMusicApi.Extensions;
using Microsoft.AspNetCore.CookiePolicy;
using YTMusicApi.Model.UserPlaylist;
using YTMusicApi.Data.UserPlaylist;
using YTMusicApi.Orchestrator.UserPlaylist;
using YTMusicApi.Middleware;

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

            services.Configure<JwtOptions>(_configuration.GetSection("JwtOptions"));
            services.AddApiAuthentication(_configuration);
            
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.WithOrigins("http://localhost:5173", "http://localhost", "http://localhost:80", "https://ytmusicplaylists.vercel.app");
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

            services.AddScoped<IJwtProvider, JwtProvider>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();

            services.AddAutoMapper(config => config.AddProfiles(new List<Profile> 
            {   
                new TrackDaoProfile(), 
                new PlaylistDaoProfile(),
                new PlaylistTrackDaoProfile(),
                new UserDaoProfile(),
                new UserPlaylistDaoProfile()
            }));
            
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


            app.UseCors();
            //app.UseHttpsRedirection();

            app.UseRouting();

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

            app.UseEndpoints(action => action.MapControllers());
        }
    }
}
