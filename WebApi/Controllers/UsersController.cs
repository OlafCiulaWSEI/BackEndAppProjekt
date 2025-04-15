using System.Text;
using Infrasctructure.EF;
using JWT.Algorithms;
using JWT.Builder;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using WebApi.Configuration;
using WebApi.Dto;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController(SignInManager<UserEntity> signInManager, UserManager<UserEntity> userManager, JwtSettings jwtSettings) : ControllerBase
    {
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await userManager.FindByNameAsync(dto.Login);
            if (user is null)
            {
                return BadRequest(new {error = "Invalid user or password!"});
            }

            var result = await signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
            if (result.Succeeded)
            {
                //TODO zwrot tokena
                return Ok(new { token = CreateToken(user)});
            }
            else
            {
                return BadRequest(new {error = "Invalid user or password!"});
            }
        }
        
        private string CreateToken(UserEntity user)
        {
            return new JwtBuilder()
                .WithAlgorithm(new HMACSHA256Algorithm())
                .WithSecret(Encoding.UTF8.GetBytes(jwtSettings.Secret))
                .AddClaim(JwtRegisteredClaimNames.Name, user.UserName)
                .AddClaim(JwtRegisteredClaimNames.Gender, "male")
                .AddClaim(JwtRegisteredClaimNames.Email, user.Email)
                .AddClaim(JwtRegisteredClaimNames.Exp, DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds())
                .AddClaim(JwtRegisteredClaimNames.Jti, Guid.NewGuid())
                .Audience(jwtSettings.Audience)
                .Issuer(jwtSettings.Issuer)
                .Encode();
        }
    }
}
