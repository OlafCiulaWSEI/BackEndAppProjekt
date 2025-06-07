using Microsoft.AspNetCore.Mvc;
using WebApi.Services;
using ApplicationCore.Models;
using WebApi.Helpers;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LegoSetController : ControllerBase
    {
        private readonly LegoSetService _legoSetService;

        public LegoSetController(LegoSetService legoSetService)
        {
            _legoSetService = legoSetService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResponse<LegoSet>>> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
                return BadRequest("Page number and page size must be greater than 0");

            var response = await _legoSetService.GetAllPaged(pageNumber, pageSize);
            
            // Add HATEOAS links
            var urlBuilder = new UrlBuilder(Request);
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
        public ActionResult<LegoSet> GetById(string id)
        {
            var set = _legoSetService.GetById(id);
            if (set == null)
                return NotFound();
            return set;
        }

        [HttpGet("theme/{theme}")]
        public ActionResult<List<LegoSet>> GetByTheme(string theme) => 
            _legoSetService.GetByTheme(theme);

        [HttpGet("year/{year}")]
        public ActionResult<List<LegoSet>> GetByYear(int year) => 
            _legoSetService.GetByYear(year);

        [HttpGet("pieces")]
        public ActionResult<List<LegoSet>> GetByPiecesRange([FromQuery] int min, [FromQuery] int max) => 
            _legoSetService.GetByPiecesRange(min, max);

        [HttpGet("themes")]
        public ActionResult<List<string>> GetAllThemes() => 
            _legoSetService.GetAllThemes();

        [HttpGet("years")]
        public ActionResult<List<int>> GetAllYears() => 
            _legoSetService.GetAllYears();

        [HttpPost]
        public ActionResult<LegoSet> Create(LegoSet set)
        {
            _legoSetService.Create(set);
            return CreatedAtAction(nameof(GetById), new { id = set.Id }, set);
        }

        [HttpPost("import")]
        public async Task<IActionResult> ImportCsv(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            var filePath = Path.GetTempFileName();
            using (var stream = System.IO.File.Create(filePath))
            {
                await file.CopyToAsync(stream);
            }

            await _legoSetService.ImportFromCsv(filePath);
            System.IO.File.Delete(filePath);

            return Ok("Data imported successfully");
        }
    }
}
