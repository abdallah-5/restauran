using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AliEns.Models
{
    public class ShoppingCart
    {
        public int Id { get; set; }

        [Required]
        public string ApplicationUserId { get; set; }
        [NotMapped]
        [ForeignKey("ApplicationUserId")]
        public virtual ApplicationUser ApplicationUser { get; set; }

        public int MenuItemId { get; set; }
        [NotMapped]
        [ForeignKey("MenuItemId")]
        public virtual MenuItem MenuItem { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Plese Enter Value Grater Than Or Equel 1.")]
        public int Count { get; set; }
    }
}
