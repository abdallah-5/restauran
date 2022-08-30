using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AliEns.ViewModels
{
    public class CouponViewModel
    {
        public int Id { get; set; }

        [Required, StringLength(maximumLength: 100, MinimumLength = 5)]
        public string Name { get; set; }

        [Required]
        public string CouponType { get; set; }

        [Required]
        public decimal Discount { get; set; }

        [Required, Display(Name = "Minimum Amount")]
        public decimal MinimumAmount { get; set; }
        public byte[] Picture { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; }
    }
}
