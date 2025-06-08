using System.ComponentModel.DataAnnotations;

namespace WebApi.Dto
{
    public class CreateCommentDto
    {
        [Required(ErrorMessage = "LegoSetId is required")]
        public string LegoSetId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Content is required")]
        public string Content { get; set; } = string.Empty;
    }
} 