using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ApplicationCore.Models;
using WebApi.Services;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly UserService _userService;

        public BooksController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetBooks()
        {
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
            {
                return Unauthorized();
            }

            var user = await _userService.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(new { message = "Books endpoint" });
        }
    }
}
