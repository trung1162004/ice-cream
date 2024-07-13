using System.ComponentModel.DataAnnotations;

namespace Project.IdentitySettings.Validators
{
    public class MinimumAgeAttribute : ValidationAttribute
    {
        private readonly int _minimumAge;

        public MinimumAgeAttribute(int minimumAge)
        {
            _minimumAge = minimumAge;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is DateTime dateOfBirth)
            {
                var age = DateTime.Now.Year - dateOfBirth.Year;
                if (dateOfBirth.AddYears(_minimumAge) <= DateTime.Now && age >= _minimumAge)
                {
                    return ValidationResult.Success;
                }

                return new ValidationResult(ErrorMessage);
            }

            return new ValidationResult("Invalid date of birth.");
        }
    }
}
