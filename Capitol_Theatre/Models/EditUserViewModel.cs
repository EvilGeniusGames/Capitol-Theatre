using System.ComponentModel.DataAnnotations;

namespace Capitol_Theatre.Models
{
    public class EditUserViewModel
    {
        public string? Id { get; set; } // null or empty = new user

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }

        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        [DataType(DataType.Password)]
        public string? ConfirmPassword { get; set; }
    }
}
