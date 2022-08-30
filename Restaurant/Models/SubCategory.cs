using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AliEns.Models
{
    public class SubCategory
    {
        public int Id { get; set; }

        [Display(Name = "Sub Category Name")]
        [Required, MaxLength(100), MinLength(2)]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Category Name")]
        public byte CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public Category Category { get; set; }
    }
}
