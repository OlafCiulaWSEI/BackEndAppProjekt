using ApplicationCore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApi.Services;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentController : ControllerBase
    {
        private readonly CommentService _commentService;

        public CommentController(CommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpGet("legoSet/{legoSetId}")]
        public ActionResult<List<Comment>> GetByLegoSetId(string legoSetId)
        {
            return _commentService.GetByLegoSetId(legoSetId);
        }

        [HttpPost]
        public IActionResult Add([FromBody] Comment comment)
        {
            if (string.IsNullOrWhiteSpace(comment.Content))
                return BadRequest("Treść komentarza nie może być pusta.");
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                comment.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            }
            else
            {
                comment.UserId = null; // anonymous
            }
            comment.CreatedAt = DateTime.UtcNow;
            _commentService.Add(comment);
            return Ok(comment);
        }

        [Authorize]
        [HttpPut("{id}")]
        public IActionResult Update(string id, [FromBody] string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return BadRequest("Treść komentarza nie może być pusta.");
            if (id == null || id.Length != 24)
                return BadRequest("Nieprawidłowy format id (musi być 24-znakowy ObjectId).");
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var success = _commentService.Update(id, userId, content);
            if (!success) return Forbid();
            return Ok();
        }

        [Authorize]
        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            if (id == null || id.Length != 24)
                return BadRequest("Nieprawidłowy format id (musi być 24-znakowy ObjectId).");
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var success = _commentService.Delete(id, userId);
            if (!success) return Forbid();
            return Ok();
        }
    }
} 