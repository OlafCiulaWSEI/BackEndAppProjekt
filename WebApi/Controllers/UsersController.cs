using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Services;
using WebApi.Dto;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using WebApi.Configuration;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly JwtSettings _jwtSettings;

        public UsersController(UserService userService, JwtSettings jwtSettings)
        {
            _userService = userService;
            _jwtSettings = jwtSettings;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            var user = new ApplicationCore.Models.MongoUser
            {
                UserName = model.UserName,
                Email = model.Email,
                EmailConfirmed = true
            };

            var result = await _userService.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                return Ok(new { message = "User registered successfully" });
            }

            return BadRequest(result.Errors);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto model)
        {
            var user = await _userService.FindByNameAsync(model.UserName);
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }

            var isPasswordValid = await _userService.CheckPasswordAsync(user, model.Password);
            if (!isPasswordValid)
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }

            var claims = await _userService.GetClaimsAsync(user);
            var token = GenerateJwtToken(claims);

            return Ok(new
            {
                token,
                user = new
                {
                    user.Id,
                    user.Email,
                    user.UserName
                }
            });
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
            {
                return Unauthorized();
            }

            var user = await _userService.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(new
            {
                user.Id,
                user.Email,
                user.UserName
            });
        }

        private string GenerateJwtToken(IList<Claim> claims)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.ValidIssuer,
                audience: _jwtSettings.ValidAudience,
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
