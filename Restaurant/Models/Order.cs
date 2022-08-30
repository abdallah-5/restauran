using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AliEns.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual ApplicationUser ApplicationUser { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        [Required]
        public decimal OrderOrginalTotal { get; set; }

        [Required]
        [DisplayFormat(DataFormatString = "{0:c}")]
        public decimal OrderTotal { get; set; }

        [Required, Display(Name = "Pickup Time")]
        public DateTime PickUpTime { get; set; }

        [Required, NotMapped]
        public DateTime PickUpDate { get; set; }

        [Display(Name = "Coupon Code")]
        public string CouponCode { get; set; }
        public decimal CouponCodeDiscount { get; set; }
        public string Status { get; set; }
        public string PaymentStatus { get; set; }

        [MaxLength(250)]
        public string Comments { get; set; }

        [Display(Name = "Pickup Name")]
        public string PersonPickUpName { get; set; }

        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }
        public string TransactionId { get; set; }
    }
}
