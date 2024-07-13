using Microsoft.AspNetCore.Identity;

namespace Project.IdentitySettings
{
    public class ErrorDescriber : IdentityErrorDescriber
    {
        public override IdentityError InvalidUserName(string userName) => new() { Code = "InvalidUserName", Description = $"\"{userName}\" username is invalid." };

        public override IdentityError DuplicateEmail(string email) => new() { Code = "DuplicateEmail", Description = $"\"{email}\" email is in use." };

        public override IdentityError PasswordTooShort(int length) => new() { Code = "PasswordTooShort", Description = $"Password must be at least  {length} characters." };

        // Custom Errors
        public static IdentityError PasswordContainsUsername() => new() { Code = "PasswordContainsUsername", Description = "Password can not contains user name." };
        public static IdentityError UserNameLength() => new() { Code = "UserNameLength", Description = "User name must be at least 6 characters." };
        public static IdentityError UserNameContainsEmail() => new() { Code = "UserNameContainsEmail", Description = "User name can not contains email name." };
    }
}