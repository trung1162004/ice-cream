using System.ComponentModel.DataAnnotations;

namespace Project.Models.ViewModels
{
    public class ResetPasswordViewModel
    {
        public string UserId { get; set; } = default!;
        public string Token { get; set; } = default!;
        public string Password { get; set; } = default!;
        [Compare("Password", ErrorMessage = "Confirm password doesn't match, Type again !")]
        public string? ConfirmPassword { get; set; }

    }
}