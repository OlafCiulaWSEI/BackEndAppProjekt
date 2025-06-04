using Microsoft.AspNetCore.Mvc;
using WebApi.Services;
using ApplicationCore.Models;

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
        public ActionResult<List<LegoSet>> GetAll() => _legoSetService.GetAll();

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
