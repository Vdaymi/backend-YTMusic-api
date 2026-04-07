using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Moq;
using YTMusicApi.Data;
using YTMusicApi.Model.Auth;
using YTMusicApi.Model.YouTube;
using YTMusicApi.Model.Integration;

namespace YTMusicApi.IntegrationTest;

public class YTMusicApiApplicationFactory : WebApplicationFactory<Startup>
{
    public Mock<IYouTubeRepository> YouTubeRepositoryMock { get; } = new();
    public Mock<IOptimizerClient> OptimizerClientMock { get; } = new();
    public Mock<IEmailSender> EmailSenderMock { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        
        builder.ConfigureTestServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<SqlDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<SqlDbContext>(options => options.UseInMemoryDatabase("TestSqlDb"));

            services.AddSingleton(YouTubeRepositoryMock.Object);
            services.AddSingleton(OptimizerClientMock.Object);
            services.AddSingleton(EmailSenderMock.Object);

            services.AddAuthentication(defaultScheme: "TestScheme")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", options => { });
            
        });
    }
}