using AliEns.Data;
using AliEns.Models;
using AliEns.Utility;
using AliEns.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AliEns.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class OrdersController : Controller
    {
        // Max size of items in page
        private int pageSize = 5;

        private readonly ApplicationDbContext _db;

        public OrdersController(ApplicationDbContext db)
        {
            _db = db;
        }

        [Authorize]
        public async Task<IActionResult> Confirm(int id)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            OrderDetailsViewModel orderDetailsVM = new OrderDetailsViewModel()
            {
                Order = await _db.Orders.FirstOrDefaultAsync(e => e.UserId == claim.Value && e.Id == id),
                OrderDetails = await _db.OrderDetails.Where(e => e.OrderId == id).ToListAsync()
            };

            return View(orderDetailsVM);
        }
        
        [Authorize]
        public async Task<IActionResult> OrderHistory(int pageNumber = 1)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            // List<OrderDetailsViewModel> orderDetailsVMList = new List<OrderDetailsViewModel>();

            PagingOrderDetailsViewModel PagingOrderDetailsVM = new PagingOrderDetailsViewModel()
            {
                Orders = new List<OrderDetailsViewModel>()
            };

            List<Order> ordersList = await _db.Orders.Include(e => e.ApplicationUser).Where(e => e.UserId == claim.Value).ToListAsync();

            foreach (var order in ordersList)
            {
                OrderDetailsViewModel OrderDetailsVM = new OrderDetailsViewModel()
                {
                    Order = order,
                    OrderDetails = await _db.OrderDetails.Where(e => e.OrderId == order.Id).ToListAsync()
                };

                PagingOrderDetailsVM.Orders.Add(OrderDetailsVM);
            }

            var count = PagingOrderDetailsVM.Orders.Count;

            PagingOrderDetailsVM.Orders = PagingOrderDetailsVM.Orders
                .OrderByDescending(e => e.Order.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            PagingOrderDetailsVM.PagingInfo = new PagingInfo()
            {
                CurrentPage = pageNumber,
                RecoredsPerPage = pageSize,
                TotalRecoreds = count,
                urlParam = "/Customer/Orders/OrderHistory?pageNumber=:"
            };

            return View(PagingOrderDetailsVM);
        }

        // GET
        public async Task<IActionResult> GetOrderDetails(int id)
        {
            OrderDetailsViewModel orderDetailsVM = new OrderDetailsViewModel()
            {
                Order = await _db.Orders.Include(e => e.ApplicationUser).FirstOrDefaultAsync(e => e.Id == id),
                OrderDetails = await _db.OrderDetails.Where(e => e.OrderId == id).ToListAsync()
            };

            return PartialView("_IndividualOrderDetails", orderDetailsVM);
        }

        // GET
        public async Task<IActionResult> GetOrderStatus(int id)
        {
            Order order = await _db.Orders.FindAsync(id);

            return PartialView("_OrderStatus", order.Status);
        }

        // GET
        [Authorize(Roles = SD.Admin + "," + SD.ManagerUser + "," + SD.KitchenUser)]
        public async Task<IActionResult> ManageOrder()
        {
            List<OrderDetailsViewModel> orderDetailsVMList = new List<OrderDetailsViewModel>();

            List<Order> ordersList = await _db.Orders.Where(e => e.Status == SD.StatusInProcess || e.Status == SD.StatusSubmitted).ToListAsync();

            foreach (var order in ordersList)
            {
                OrderDetailsViewModel OrderDetailsVM = new OrderDetailsViewModel()
                {
                    Order = order,
                    OrderDetails = await _db.OrderDetails.Where(e => e.OrderId == order.Id).ToListAsync()
                };

                orderDetailsVMList.Add(OrderDetailsVM);
            }

            return View(orderDetailsVMList.OrderBy(e => e.Order.PickUpTime).ToList());
        }

        [Authorize(Roles = SD.Admin + ", " + SD.ManagerUser + ", " + SD.KitchenUser)]
        public async Task<IActionResult> OrderPrepare(int orderId)
        {
            var orderHeader = await _db.Orders.FindAsync(orderId);
            orderHeader.Status = SD.StatusInProcess;

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(ManageOrder));
        }

        [Authorize(Roles = SD.Admin + ", " + SD.ManagerUser + ", " + SD.KitchenUser)]
        public async Task<IActionResult> OrderReady(int orderId)
        {
            var orderHeader = await _db.Orders.FindAsync(orderId);
            orderHeader.Status = SD.StatusReady;

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(ManageOrder));
        }

        [Authorize(Roles = SD.Admin + ", " + SD.ManagerUser + ", " + SD.KitchenUser)]
        public async Task<IActionResult> OrderCancel(int orderId)
        {
            var orderHeader = await _db.Orders.FindAsync(orderId);
            orderHeader.Status = SD.StatusCancelled;

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(ManageOrder));
        }

        // GET
        [Authorize(Roles = SD.Admin + ", " + SD.ManagerUser + "," + SD.FrontDeskUser)]
        public async Task<IActionResult> OrderPickup(int pageNumber = 1, string searchName = null, string searchPhone = null, string searchEmail = null)
        {
            PagingOrderDetailsViewModel PagingOrderDetailsVM = new PagingOrderDetailsViewModel()
            {
                Orders = new List<OrderDetailsViewModel>()
            };

            StringBuilder param = new StringBuilder();
            param.Append("/Customer/Orders/OrderPickup?pageNumber=:");

            param.Append("&searchName");
            if (searchName != null)
            {
                param.Append(searchName);
            }
            else
            {
                searchName = "";
            }

            param.Append("&searchPhone");
            if (searchPhone != null)
            {
                param.Append(searchPhone);
            }
            else
            {
                searchPhone = "";
            }

            param.Append("&searchEmail");
            if (searchEmail != null)
            {
                param.Append(searchEmail);
            }
            else
            {
                searchEmail = "";
            }

            List<Order> ordersList = await _db.Orders
                .Include(e => e.ApplicationUser)
                .OrderByDescending(e => e.OrderDate)
                .Where(e => 
                    e.Status == SD.StatusReady &&
                    e.PersonPickUpName.Contains(searchName) &&
                    e.PhoneNumber.Contains(searchPhone) &&
                    e.ApplicationUser.Email.Contains(searchEmail)
                )
                .ToListAsync();

            foreach (var order in ordersList)
            {
                OrderDetailsViewModel OrderDetailsVM = new OrderDetailsViewModel()
                {
                    Order = order,
                    OrderDetails = await _db.OrderDetails.Where(e => e.OrderId == order.Id).ToListAsync()
                };

                PagingOrderDetailsVM.Orders.Add(OrderDetailsVM);
            }

            var count = PagingOrderDetailsVM.Orders.Count;

            PagingOrderDetailsVM.Orders = PagingOrderDetailsVM.Orders
                .OrderByDescending(e => e.Order.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            PagingOrderDetailsVM.PagingInfo = new PagingInfo()
            {
                CurrentPage = pageNumber,
                RecoredsPerPage = pageSize,
                TotalRecoreds = count,
                urlParam = param.ToString()
            };

            return View(PagingOrderDetailsVM);
        }

        // POST
        [Authorize(Roles = SD.Admin + ", " + SD.ManagerUser + "," + SD.FrontDeskUser)]
        [HttpPost]
        public async Task<IActionResult> OrderPickup(int orderId)
        {
            var orderHeader = await _db.Orders.FindAsync(orderId);
            orderHeader.Status = SD.StatusCompleted;

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(OrderPickup));
        }
    }
}
