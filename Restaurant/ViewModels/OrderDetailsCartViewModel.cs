using AliEns.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AliEns.ViewModels
{
    public class OrderDetailsCartViewModel
    {
        public List<ShoppingCart> ShoppingCartsList { get; set; }
        public Order Order { get; set; }
    }
}
