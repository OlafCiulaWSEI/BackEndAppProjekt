using System.ComponentModel.DataAnnotations;

namespace WebApi.Dto
{
    public class UpdateCommentDto
    {
        [Required(ErrorMessage = "Content is required")]
        public string Content { get; set; } = string.Empty;
    }
} 