using Project.Models.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Models
{
    [Table("Payment")]
    public class PaymentInfo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
        
        public int PaymentInfoId { get; set; }
        public string PaymentId { get; set; }
        public string PackageType { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string? UserId { get; set; }
        public AppUser? User { get; set; }
    }

}
