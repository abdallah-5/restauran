using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AliEns.Models
{
    public class MenuItem
    {
        public int Id { get; set; }

        [Required, MaxLength(100), MinLength(5)]
        public string Name { get; set; }

        [MaxLength(250), MinLength(5)]
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Image { get; set; }
        public string Spicyness { get; set; }

        [Required, Display(Name = "Category")]
        public byte CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public Category Category { get; set; }

        [Required, Display(Name = "Sub Category")]
        public int SubCategoryId { get; set; }
        [ForeignKey("SubCategoryId")]
        public SubCategory SubCategory { get; set; }
        public enum ESpicy
        {
            NA          = 0,
            NotSpicy    = 1,
            Spicy       = 2,
            VerySpicy   = 3
        }
    }
}
