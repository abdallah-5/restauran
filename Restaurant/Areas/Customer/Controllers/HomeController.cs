using AliEns.Data;
using AliEns.Models;
using AliEns.Utility;
using AliEns.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AliEns.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IToastNotification _toastNotification;

        public HomeController(ApplicationDbContext db, IToastNotification toastNotification)
        {
            _db = db;
            _toastNotification = toastNotification;
        }

        // GET
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if (claim != null)
            {
                var shoppingCart = await _db.ShoppingCarts.Where(e => e.ApplicationUserId == claim.Value).ToListAsync();

                HttpContext.Session.SetInt32(SD.ShoppingCartCount, shoppingCart.Count);
            }
            

            IndexViewModel model = new IndexViewModel
            {
                Coupons = await _db.Coupons.Where(e => e.IsActive == true).ToListAsync(),
                Categories = await _db.Categories.ToListAsync(),
                MenuItems = await _db.MenuItems.Include(e => e.Category).Include(e => e.SubCategory).ToListAsync()
            };

            return View(model);
        }

        // GET
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Details(int? itemid)
        {
            if (itemid == null)
            {
                return BadRequest();
            }

            var menuItem = await _db.MenuItems.Include(e => e.Category).Include(e => e.SubCategory).Where(e => e.Id == itemid).FirstOrDefaultAsync();

            if (menuItem == null)
            {
                return NotFound();
            }
            
            var shoppingCartVM = new ShoppingCartViewModel()
            {
                MenuItem = menuItem,
                MenuItemId = menuItem.Id
            };

            return View(shoppingCartVM);
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details(ShoppingCartViewModel shoppingCartVM)
        {
            //if (!ModelState.IsValid)
            //{
            //    var menuItem = await _db.MenuItems.Include(e => e.Category).Include(e => e.SubCategory).Where(e => e.Id == shoppingCartVM.MenuItemId).FirstOrDefaultAsync();

            //    var shoppingCartVMBack = new ShoppingCartViewModel()
            //    {
            //        MenuItem = menuItem,
            //        MenuItemId = menuItem.Id
            //    };

            //    return View(shoppingCartVMBack);
            //}

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            shoppingCartVM.ApplicationUserId = claim.Value;

            var isShopingCartExist = await _db.ShoppingCarts.Where(e => e.ApplicationUserId == shoppingCartVM.ApplicationUserId && e.MenuItemId == shoppingCartVM.MenuItemId).FirstOrDefaultAsync();
            
            if (isShopingCartExist == null)
            {
                var shopingCart = new ShoppingCart()
                {
                    MenuItemId = shoppingCartVM.MenuItemId,
                    ApplicationUserId = shoppingCartVM.ApplicationUserId,
                    Count = shoppingCartVM.Count
                };

                _db.ShoppingCarts.Add(shopingCart);
            }
            else
            {
                isShopingCartExist.Count += shoppingCartVM.Count;
            }

            await _db.SaveChangesAsync();

            var countItemsInShoppingCart = _db.ShoppingCarts.Where(e => e.ApplicationUserId == shoppingCartVM.ApplicationUserId).ToList().Count;

            HttpContext.Session.SetInt32(SD.ShoppingCartCount, countItemsInShoppingCart);

            _toastNotification.AddSuccessToastMessage("Item(s) Added To The Shopping Cart.");

            return RedirectToAction(nameof(Index));
        }
    }
}
