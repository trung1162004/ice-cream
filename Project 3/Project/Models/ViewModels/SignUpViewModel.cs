using Project.IdentitySettings.Validators;
using System.ComponentModel.DataAnnotations;

namespace Project.Models.ViewModels
{
    public class SignUpViewModel
    {

        public string UserName { get; set; } = default!;

        public string FullName { get; set; } = null!;
        public string Email { get; set; } = default!;
        [Required(ErrorMessage = "Phone number is a required field.")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Phone numbers can only contain digits.")]
        public string PhoneNumber { get; set; }
        public Gender Gender { get; set; } = Gender.Unknown;


        [Required(ErrorMessage = "Date of birth is a required field.")]
        [DataType(DataType.Date)]
        [Display(Name = "BirthDay")]
        [MinimumAge(12, ErrorMessage = "You must be 12 years old to register.")]
        public DateTime BirthDay { get; set; }
        public string Password { get; set; } = default!;
        [Compare("Password", ErrorMessage = "Confirm password doesn't match, Type again !")]
        public string? ConfirmPassword { get; set; }

        public string Address { get; set; } = null!;
    }
}