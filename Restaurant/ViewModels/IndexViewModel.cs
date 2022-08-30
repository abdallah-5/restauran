using AliEns.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AliEns.ViewModels
{
    public class IndexViewModel
    {
        public IEnumerable<Coupon> Coupons { get; set; }
        public IEnumerable<Category> Categories { get; set; }
        public IEnumerable<MenuItem> MenuItems { get; set; }
    }
}
