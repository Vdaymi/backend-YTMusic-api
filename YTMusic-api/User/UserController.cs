using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using YTMusicApi.Model.User;
using YTMusicApi.User.Contracts;

namespace YTMusicApi.User
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserOrchestrator _orchestrator;
        public UserController(
            IUserOrchestrator orchestrator) 
        {
            _orchestrator = orchestrator;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync(RegisterUser registerUser)
        {
            await _orchestrator.RegisterAsync(registerUser.UserName, registerUser.Email, registerUser.Password);
            return Ok();
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync(LoginUser loginUser)
        {
            var result = await _orchestrator.LoginAsync(loginUser.Email, loginUser.Password);

            HttpContext.Response.Cookies.Append("cookies-play", result.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true, 
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });

            return Ok(result.UserName);
        }
        
        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerificationRequest request)
        {
            await _orchestrator.VerifyEmailAsync(request.Token);
            return Ok(new { Message = "Email successfully verified. You can now log in." });
        }

        [HttpPost("resend-verification"), EnableRateLimiting("PerUserResendVerificationPolicy")]
        public async Task<IActionResult> ResendVerificationEmailAsync([FromBody] ResendVerificationRequest request)
        {
            await _orchestrator.ResendVerificationEmailAsync(request.Email);

            return Ok(new { Message = "If your email is registered and not verified, a new verification link has been sent." });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Response.Cookies.Delete("cookies-play", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None
            });

            return Ok();
        }
    }
}
