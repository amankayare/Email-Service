using System.ComponentModel.DataAnnotations;

namespace EmailService.API.Models
{
    public class EmailRequestDto
    {
        [Required]
        [EmailAddress]
        public string To { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string Body { get; set; } = string.Empty;

        public bool IsHtml { get; set; } = true;

        [EmailAddress]
        public string? From { get; set; }
    }
}
