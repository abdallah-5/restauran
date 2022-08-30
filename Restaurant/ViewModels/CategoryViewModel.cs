using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AliEns.ViewModels
{
    public class CategoryViewModel
    {
        public byte Id { get; set; }

        [Display(Name = "Category Name")]
        [Required, StringLength(maximumLength: 100, MinimumLength = 2)]
        public string Name { get; set; }
    }
}
