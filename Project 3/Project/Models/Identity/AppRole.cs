    using Microsoft.AspNetCore.Identity;

namespace Project.Models.Identity
{
    public class AppRole : IdentityRole
    {
        public AppRole() : base()
        {
            // Constructor không tham số
        }
        public AppRole(string roleName) : base(roleName)
        {
            // Constructor accepting role name
        }
        public DateTime CreatedOn { get; set; }
    }
}