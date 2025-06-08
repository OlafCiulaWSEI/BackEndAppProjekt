using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WebApi.Services;
using ApplicationCore.Models;
using WebApi.Helpers;
using System.Security.Claims;
using WebApi.Dto;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentController : ControllerBase
    {
        private readonly CommentService _commentService;
        private readonly UserService _userService;

        public CommentController(CommentService commentService, UserService userService)
        {
            _commentService = commentService;
            _userService = userService;
        }

        [HttpGet("legoset/{legoSetId}")]
        public async Task<ActionResult<PagedResponse<Comment>>> GetByLegoSetId(
            string legoSetId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
                return BadRequest("Page number and page size must be greater than 0");

            var response = await _commentService.GetCommentsByLegoSetPaged(legoSetId, pageNumber, pageSize);

            // Add HATEOAS links
            var urlBuilder = new UrlBuilder(Request, legoSetId);
            response.Metadata.Links.Add("self", urlBuilder.BuildUrl(pageNumber, pageSize));
            
            if (response.Metadata.HasPrevious)
                response.Metadata.Links.Add("previous", urlBuilder.BuildUrl(pageNumber - 1, pageSize));
            
            if (response.Metadata.HasNext)
                response.Metadata.Links.Add("next", urlBuilder.BuildUrl(pageNumber + 1, pageSize));
            
            response.Metadata.Links.Add("first", urlBuilder.BuildUrl(1, pageSize));
            response.Metadata.Links.Add("last", urlBuilder.BuildUrl(response.Metadata.TotalPages, pageSize));

            return Ok(response);
        }

        [HttpGet("{id}")]
        public ActionResult<Comment> GetById(string id)
        {
            var comment = _commentService.GetById(id);
            if (comment == null)
                return NotFound();
            return comment;
        }

        [HttpPost]
        public async Task<ActionResult<Comment>> Create([FromBody] CreateCommentDto createDto)
        {
            var comment = new Comment
            {
                LegoSetId = createDto.LegoSetId,
                Content = createDto.Content
            };

            // If user is authenticated, add their information
            if (User.Identity?.IsAuthenticated == true)
            {
                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                if (!string.IsNullOrEmpty(email))
                {
                    var user = await _userService.FindByEmailAsync(email);
                    if (user != null)
                    {
                        comment.UserId = user.Id;
                        comment.UserName = user.UserName;
                    }
                }
            }

            var createdComment = _commentService.Create(comment);
            return CreatedAtAction(nameof(GetById), new { id = createdComment.Id }, createdComment);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateCommentDto updateDto)
        {
            if (string.IsNullOrEmpty(updateDto.Content))
                return BadRequest("Content cannot be empty");

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized();

            var user = await _userService.FindByEmailAsync(email);
            if (user == null)
                return Unauthorized();

            // Get the comment first to check if it exists
            var comment = _commentService.GetById(id);
            if (comment == null)
            {
                return NotFound("Comment not found");
            }

            // Update the comment without checking ownership
            var success = _commentService.Update(id, updateDto.Content, comment.UserId); // Keep the original author
            if (!success)
            {
                return StatusCode(500, "Failed to update comment");
            }

            // Get and return the updated comment
            var updatedComment = _commentService.GetById(id);
            return Ok(updatedComment);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized();

            var user = await _userService.FindByEmailAsync(email);
            if (user == null)
                return Unauthorized();

            // Get the comment first to check if it exists
            var comment = _commentService.GetById(id);
            if (comment == null)
            {
                return NotFound("Comment not found");
            }

            // Delete the comment without checking ownership
            var success = _commentService.Delete(id);
            if (!success)
            {
                return StatusCode(500, "Failed to delete comment");
            }

            return NoContent();
        }
    }
} 