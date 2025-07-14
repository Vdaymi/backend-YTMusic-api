using Microsoft.AspNetCore.Mvc;
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
            var token = await _orchestrator.LoginAsync(loginUser.Email, loginUser.Password);

            HttpContext.Response.Cookies.Append("cookies-play", token);

            return Ok();
        }
    }
}
