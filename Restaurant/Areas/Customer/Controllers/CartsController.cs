using AliEns.Data;
using AliEns.Models;
using AliEns.Utility;
using AliEns.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AliEns.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CartsController(ApplicationDbContext db)
        {
            _db = db;
        }

        [BindProperty]
        public OrderDetailsCartViewModel OrderDetailsCartVM { get; set; }

        // GET
        [HttpGet]
        public IActionResult Index()
        {
            OrderDetailsCartVM = new OrderDetailsCartViewModel()
            {
                Order = new Models.Order()
            };

            OrderDetailsCartVM.Order.OrderOrginalTotal = 0;

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var shoppingCarts = _db.ShoppingCarts.Where(e => e.ApplicationUserId == claim.Value);

            if (shoppingCarts != null)
            {
                OrderDetailsCartVM.ShoppingCartsList = shoppingCarts.ToList();
            }

            foreach (var item in OrderDetailsCartVM.ShoppingCartsList)
            {
                item.MenuItem = _db.MenuItems.First(e => e.Id == item.MenuItemId);
                
                item.MenuItem.Description = SD.ConvertToRawHtml(item.MenuItem.Description);
                if (item.MenuItem.Description.Length > 75)
                {
                    item.MenuItem.Description = item.MenuItem.Description.Substring(0, 74);
                }

                OrderDetailsCartVM.Order.OrderTotal += item.MenuItem.Price * item.Count;
            }

            OrderDetailsCartVM.Order.OrderOrginalTotal = OrderDetailsCartVM.Order.OrderTotal;

            if (HttpContext.Session.GetString(SD.ssCouponCode) != null)
            {
                OrderDetailsCartVM.Order.CouponCode = HttpContext.Session.GetString(SD.ssCouponCode);

                var couponFromDB = _db.Coupons.Where(e => e.Name.ToLower() == OrderDetailsCartVM.Order.CouponCode.ToLower()).FirstOrDefault();

                OrderDetailsCartVM.Order.OrderTotal = SD.DiscountPrice(couponFromDB, OrderDetailsCartVM.Order.OrderOrginalTotal);
            }

            return View(OrderDetailsCartVM);
        }

        // POST
        [HttpPost]
        public IActionResult ApplyCoupon()
        {
            if (OrderDetailsCartVM.Order.CouponCode == null)
            {
                OrderDetailsCartVM.Order.CouponCode = "";
            }

            HttpContext.Session.SetString(SD.ssCouponCode, OrderDetailsCartVM.Order.CouponCode);

            return RedirectToAction(nameof(Index));
        }

        // POST
        [HttpPost]
        public IActionResult RemoveCoupon()
        {
            HttpContext.Session.SetString(SD.ssCouponCode, string.Empty);

            return RedirectToAction(nameof(Index));
        }

        // POST
        [HttpPost]
        public async Task<IActionResult> Plus(int cartId)
        {
            var shoppingCarts = await _db.ShoppingCarts.FindAsync(cartId);

            shoppingCarts.Count += 1;
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // POST
        [HttpPost]
        public async Task<IActionResult> Minus(int cartId)
        {
            var shoppingCarts = await _db.ShoppingCarts.FindAsync(cartId);

            if (shoppingCarts.Count > 1)
            {
                shoppingCarts.Count -= 1;
                await _db.SaveChangesAsync();
            }
            
            return RedirectToAction(nameof(Index));
        }

        // POST
        [HttpPost]
        public async Task<IActionResult> Remove(int cartId)
        {
            var shoppingCarts = await _db.ShoppingCarts.FindAsync(cartId);

            _db.ShoppingCarts.Remove(shoppingCarts);
            await _db.SaveChangesAsync();

            var count = _db.ShoppingCarts.Where(e => e.ApplicationUserId == shoppingCarts.ApplicationUserId).ToList().Count;

            HttpContext.Session.SetInt32(SD.ShoppingCartCount, count);

            return RedirectToAction(nameof(Index));
        }

        // GET
        [HttpGet]
        public IActionResult Summary()
        {
            OrderDetailsCartVM = new OrderDetailsCartViewModel()
            {
                Order = new Models.Order()
            };

            OrderDetailsCartVM.Order.OrderOrginalTotal = 0;

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            var appUser = _db.ApplicationUsers.Find(claim.Value);

            OrderDetailsCartVM.Order.PersonPickUpName = appUser.Name;
            OrderDetailsCartVM.Order.PhoneNumber = appUser.PhoneNumber;
            OrderDetailsCartVM.Order.PickUpTime = DateTime.Now;

            var shoppingCarts = _db.ShoppingCarts.Where(e => e.ApplicationUserId == claim.Value);

            if (shoppingCarts != null)
            {
                OrderDetailsCartVM.ShoppingCartsList = shoppingCarts.ToList();
            }

            foreach (var item in OrderDetailsCartVM.ShoppingCartsList)
            {
                item.MenuItem = _db.MenuItems.First(e => e.Id == item.MenuItemId);

                OrderDetailsCartVM.Order.OrderTotal += item.MenuItem.Price * item.Count;
            }

            OrderDetailsCartVM.Order.OrderOrginalTotal = OrderDetailsCartVM.Order.OrderTotal;

            if (HttpContext.Session.GetString(SD.ssCouponCode) != null)
            {
                OrderDetailsCartVM.Order.CouponCode = HttpContext.Session.GetString(SD.ssCouponCode);

                var couponFromDB = _db.Coupons.Where(e => e.Name.ToLower() == OrderDetailsCartVM.Order.CouponCode.ToLower()).FirstOrDefault();

                OrderDetailsCartVM.Order.OrderTotal = SD.DiscountPrice(couponFromDB, OrderDetailsCartVM.Order.OrderOrginalTotal);
            }

            return View(OrderDetailsCartVM);
        }

        // POST
        [HttpPost]
        [ActionName("Summary")]
        public async Task<IActionResult> SummaryPost(string stripeToken)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            
            OrderDetailsCartVM.ShoppingCartsList = await _db.ShoppingCarts.Where(e => e.ApplicationUserId == claim.Value).ToListAsync();

            OrderDetailsCartVM.Order.PaymentStatus = SD.PaymentStatusPending;
            OrderDetailsCartVM.Order.OrderDate = DateTime.Now;
            OrderDetailsCartVM.Order.UserId = claim.Value;
            OrderDetailsCartVM.Order.Status = SD.PaymentStatusPending;
            OrderDetailsCartVM.Order.PickUpTime = Convert.ToDateTime(OrderDetailsCartVM.Order.PickUpDate.ToShortDateString() + " " + OrderDetailsCartVM.Order.PickUpTime.ToShortTimeString());
            OrderDetailsCartVM.Order.OrderOrginalTotal = 0;

            _db.Orders.Add(OrderDetailsCartVM.Order);
            await _db.SaveChangesAsync();

            foreach (var item in OrderDetailsCartVM.ShoppingCartsList)
            {
                item.MenuItem = _db.MenuItems.FirstOrDefault(e => e.Id == item.MenuItemId);
                OrderDetail orderDetail = new OrderDetail()
                {
                    MenuItemId = item.MenuItemId,
                    OrderId = OrderDetailsCartVM.Order.Id,
                    Description = item.MenuItem.Description,
                    Name = item.MenuItem.Name,
                    Price = item.MenuItem.Price,
                    Count = item.Count
                };
                OrderDetailsCartVM.Order.OrderOrginalTotal += item.MenuItem.Price * item.Count;
                _db.OrderDetails.Add(orderDetail);
            }

            if (HttpContext.Session.GetString(SD.ssCouponCode) != null)
            {
                OrderDetailsCartVM.Order.CouponCode = HttpContext.Session.GetString(SD.ssCouponCode);

                var couponFromDB = _db.Coupons.Where(e => e.Name.ToLower() == OrderDetailsCartVM.Order.CouponCode.ToLower()).FirstOrDefault();

                OrderDetailsCartVM.Order.OrderTotal = SD.DiscountPrice(couponFromDB, OrderDetailsCartVM.Order.OrderOrginalTotal);
            }
            else
            {
                OrderDetailsCartVM.Order.OrderTotal = Math.Round(OrderDetailsCartVM.Order.OrderOrginalTotal, 2);
            }

            OrderDetailsCartVM.Order.CouponCodeDiscount = OrderDetailsCartVM.Order.OrderOrginalTotal - OrderDetailsCartVM.Order.OrderTotal;

            _db.ShoppingCarts.RemoveRange(OrderDetailsCartVM.ShoppingCartsList);

            HttpContext.Session.SetInt32(SD.ShoppingCartCount, 0);

            await _db.SaveChangesAsync();

            var options = new ChargeCreateOptions
            {
                Amount = Convert.ToInt32(OrderDetailsCartVM.Order.OrderTotal * 100),
                Currency = "usd",
                Description = "Order ID : " + OrderDetailsCartVM.Order.Id,
                Source = stripeToken
            };

            var service = new ChargeService();
            Charge charge = service.Create(options);

            if (charge.BalanceTransactionId == null)
            {
                OrderDetailsCartVM.Order.PaymentStatus = SD.PaymentStatusRejected;
            }
            else
            {
                OrderDetailsCartVM.Order.TransactionId = charge.BalanceTransactionId;
            }

            if (charge.Status.ToLower() == "succeeded")
            {
                OrderDetailsCartVM.Order.PaymentStatus = SD.PaymentStatusApproved;
                OrderDetailsCartVM.Order.Status = SD.StatusSubmitted;
            }
            else
            {
                OrderDetailsCartVM.Order.PaymentStatus = SD.PaymentStatusRejected;
            }

            await _db.SaveChangesAsync();

            return RedirectToAction("Confirm", "Orders", new { id = OrderDetailsCartVM.Order.Id });
        }
    }
}
