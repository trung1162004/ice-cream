using Microsoft.AspNetCore.Identity;

namespace Project.Models.Identity
{
    public class AppUser : IdentityUser
    {
        public TwoFactorType TwoFactorType { get; set; }
        public DateTime BirthDay { get; set; }
        public Gender Gender { get; set; }

        public string Address { get; set; } = null!;

        public string FullName { get; set; } = null!;
        public DateTime CreatedOn { get; set; }

        public ICollection<PaymentInfo> PaymentInfos { get; set; }

        public virtual ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();

        public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
        public virtual ICollection<Orders> Orders { get; set; } = new List<Orders>();


    }
}