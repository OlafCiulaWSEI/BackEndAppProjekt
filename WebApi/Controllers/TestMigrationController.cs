using Microsoft.AspNetCore.Mvc;
using WebApi.Services;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestMigrationController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly Infrasctructure.EF.AppDbContext _sqliteContext;

        public TestMigrationController(UserService userService, Infrasctructure.EF.AppDbContext sqliteContext)
        {
            _userService = userService;
            _sqliteContext = sqliteContext;
        }

        [HttpGet("compare")]
        public IActionResult CompareUsers()
        {
            // Get users from both databases
            var mongoUsers = _userService.GetAll();
            var sqliteUsers = _sqliteContext.Users.ToList();

            // Compare counts
            var result = new
            {
                SqliteUserCount = sqliteUsers.Count,
                MongoUserCount = mongoUsers.Count,
                SqliteUsers = sqliteUsers.Select(u => new { u.Id, u.Email, u.UserName }),
                MongoUsers = mongoUsers.Select(u => new { u.Id, u.Email, u.UserName })
            };

            return Ok(result);
        }

        [HttpGet("mongo-users")]
        public IActionResult GetMongoUsers()
        {
            var users = _userService.GetAll();
            return Ok(users);
        }
    }
} 