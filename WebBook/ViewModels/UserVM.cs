using System.ComponentModel.DataAnnotations;
namespace WebBook.ViewModels
{
    public class UserVM
    {
        public string? Id { get; set; }
        [Required]
        public string? FullName { get; set; }

        [Required]
        public string? UserName { get; set; }
        [Required]
        public string? Email { get; set; }
        public string? Role { get; set; }
    }
}
