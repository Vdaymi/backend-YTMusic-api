using YTMusicApi.Data;
using YTMusicApi.Data.Playlist;
using YTMusicApi.Data.User;
using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using YTMusicApi.Data.PlaylistTrack;
using YTMusicApi.Data.Track;
using YTMusicApi.Data.UserPlaylist;
using YTMusicApi.Model.Auth;
using YTMusicApi.Model.Playlist;
using YTMusicApi.Model.Track;
using YTMusicApi.Shared.Optimization;

namespace YTMusicApi.IntegrationTest;

public class BaseTest : IClassFixture<YTMusicApiApplicationFactory>, IDisposable
{
    protected readonly HttpClient HttpClient;
    protected readonly YTMusicApiApplicationFactory Factory;
    private readonly Guid _testUserId;
    private readonly IServiceScope _scope;
    private readonly SqlDbContext _sqlDbContext;

    protected BaseTest(YTMusicApiApplicationFactory factory)
    {
        Factory = factory;
        
        _testUserId = Guid.NewGuid();
        
        HttpClient = factory.CreateClient();
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TestScheme");
        HttpClient.DefaultRequestHeaders.Add("X-Test-User-Id", _testUserId.ToString());
        
        _scope = factory.Services.CreateScope();
        _sqlDbContext = _scope.ServiceProvider.GetRequiredService<SqlDbContext>();

        _sqlDbContext.Database.EnsureDeleted();
        _sqlDbContext.Database.EnsureCreated();
    }
    
    public void Dispose()
    {
        _scope?.Dispose();
    }

    protected async Task<TEntity?> FindAsyncInNewContext<TDbContext, TEntity>(params object[] keyValues) 
        where TDbContext : DbContext 
        where TEntity : class
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TDbContext>();
        return await context.Set<TEntity>().FindAsync(keyValues);
    }

    protected void MockYouTubePlaylist(string playlistId, PlaylistDto? expectedResult)
    {
        Factory.YouTubeRepositoryMock
            .Setup(x => x.GetPlaylistAsync(playlistId))
            .ReturnsAsync(expectedResult);
    }
 
    protected void MockYouTubePlaylistTracks(string playlistId, List<string> trackIds)
    {
        Factory.YouTubeRepositoryMock
            .Setup(x => x.GetTrackIdsFromPlaylistAsync(playlistId))
            .ReturnsAsync(trackIds);
    }
    
    protected void MockYouTubeGetTracks(List<TrackDto> expectedTracks)
    {
        Factory.YouTubeRepositoryMock
            .Setup(x => x.GetTracksAsync(It.IsAny<List<string>>()))
            .ReturnsAsync(expectedTracks);
    }
    
    protected void MockOptimizerClient(OptimizationResponse expectedResponse)
    {
        Factory.OptimizerClientMock
            .Setup(x => x.OptimizePlaylistAsync(It.IsAny<OptimizationSettingsDto>()))
            .ReturnsAsync(expectedResponse);
    }

    protected async Task<PlaylistDao> SeedPlaylistAsync(Action<PlaylistDao>? configure = null)
    {
        var playlist = new PlaylistDao
        {
            PlaylistId = "PL" + Guid.NewGuid().ToString("N"),
            Title = "Default Test Title",
            ChannelTitle = "Default Test Channel",
            ItemCount = 10
        };

        configure?.Invoke(playlist);

        await _sqlDbContext.Playlists.AddAsync(playlist);
        await _sqlDbContext.SaveChangesAsync();

        return playlist;
    }

    protected async Task<UserDao> SeedUserAsync(Action<UserDao>? configure = null)
    {
        var passwordHasher = _scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var user = new UserDao
        {
            Id = _testUserId, 
            UserName = "Integration Test User",
            Email = $"{_testUserId}@integration.com",
            PasswordHash = passwordHasher.Generate("TestPassword123!"),
            IsEmailVerified = true
        };

        configure?.Invoke(user);
        await _sqlDbContext.Users.AddAsync(user);
        await _sqlDbContext.SaveChangesAsync();
        return user;
    }
    
    protected async Task<TrackDao> SeedTrackAsync(Action<TrackDao>? configure = null)
    {
        var track = new TrackDao
        {
            TrackId = Guid.NewGuid().ToString("N").Substring(0, 11), 
            Title = "Default Test Track",
            ChannelTitle = "Default Test Channel",
            Duration = TimeSpan.FromMinutes(3),
            ImageUrl = "default_image_url"
        };
 
        configure?.Invoke(track);
        await _sqlDbContext.Tracks.AddAsync(track);
        await _sqlDbContext.SaveChangesAsync();
        return track;
    }
 
    protected async Task<PlaylistTrackDao> SeedPlaylistTrackAsync(string playlistId, string trackId)
    {
        var playlistTrack = new PlaylistTrackDao
        {
            PlaylistId = playlistId,
            TrackId = trackId
        };
        await _sqlDbContext.PlaylistTracks.AddAsync(playlistTrack);
        await _sqlDbContext.SaveChangesAsync();
        return playlistTrack;
    }
    
    protected async Task<UserPlaylistDao> SeedUserPlaylistAsync(Guid userId, string playlistId)
    {
        var userPlaylist = new UserPlaylistDao
        {
            UserId = userId,
            PlaylistId = playlistId
        };
        await _sqlDbContext.UserPlaylists.AddAsync(userPlaylist);
        await _sqlDbContext.SaveChangesAsync();
        return userPlaylist;
    }

}