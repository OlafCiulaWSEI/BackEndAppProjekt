using Microsoft.AspNetCore.Mvc;
using WebApi.Services;
using ApplicationCore.Models;
using WebApi.Helpers;

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
            var urlBuilder = new UrlBuilder(Request);
            response.Metadata.Links.Add("self", urlBuilder.BuildUrl(pageNumber, pageSize, "legoSetId", legoSetId));
            
            if (response.Metadata.HasPrevious)
                response.Metadata.Links.Add("previous", urlBuilder.BuildUrl(pageNumber - 1, pageSize, "legoSetId", legoSetId));
            
            if (response.Metadata.HasNext)
                response.Metadata.Links.Add("next", urlBuilder.BuildUrl(pageNumber + 1, pageSize, "legoSetId", legoSetId));
            
            response.Metadata.Links.Add("first", urlBuilder.BuildUrl(1, pageSize, "legoSetId", legoSetId));
            response.Metadata.Links.Add("last", urlBuilder.BuildUrl(response.Metadata.TotalPages, pageSize, "legoSetId", legoSetId));

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
        public ActionResult<Comment> Create(Comment comment)
        {
            _commentService.Create(comment);
            return CreatedAtAction(nameof(GetById), new { id = comment.Id }, comment);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            var comment = _commentService.GetById(id);
            if (comment == null)
                return NotFound();

            _commentService.Delete(id);
            return NoContent();
        }
    }
} 