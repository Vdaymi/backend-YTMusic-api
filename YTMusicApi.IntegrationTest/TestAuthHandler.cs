using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace YTMusicApi.IntegrationTest;

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string DefaultUserId = "11111111-1111-1111-1111-111111111111";

    public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
        : base(options, logger, encoder, clock) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var userId = Context.Request.Headers["X-Test-User-Id"].FirstOrDefault() ?? DefaultUserId;
        
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "TestUser"),
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim("userId", userId)
        };
        
        var identity = new ClaimsIdentity(claims, "TestScheme");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "TestScheme");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}