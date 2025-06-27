using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Models;
using WebApi.Services;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly TokenService _tokenService;

        public AuthController(TokenService tokenService)
        {
            _tokenService = tokenService;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (request.ClientUserName == "test" && request.ClientPassword == "password")
            {
                var user = new UserModel { Username = request.ClientUserName };
                var token = _tokenService.BuildToken(user);
                return Ok(new { token });
            }
            return Unauthorized();
        }
    }
}
