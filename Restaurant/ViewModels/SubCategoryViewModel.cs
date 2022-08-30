using AliEns.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AliEns.ViewModels
{
    public class SubCategoryViewModel
    {
        public SubCategory SubCategory { get; set; }
        public IEnumerable<Category> CategoriesList { get; set; }
        public string StatusMessage { get; set; }
    }
}
