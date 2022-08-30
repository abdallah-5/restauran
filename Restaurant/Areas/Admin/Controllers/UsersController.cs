using AliEns.Data;
using AliEns.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AliEns.Areas.Admin.Controllers
{
    [Authorize(Roles = SD.Admin + "," + SD.ManagerUser)]
    [Area("Admin")]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IToastNotification _toastNotification;

        public UsersController(ApplicationDbContext db, IToastNotification toastNotification)
        {
            _db = db;
            _toastNotification = toastNotification;
        }

        
        // GET
        public async Task<IActionResult> Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            string UserId = claim.Value;

            var adminRole = _db.Roles.Where(e => e.Name == SD.Admin).FirstOrDefault().Id;
            var admin = _db.UserRoles.Where(e => e.RoleId == adminRole).FirstOrDefault();

            if (User.IsInRole(SD.ManagerUser))
            {
                return View(await _db.ApplicationUsers.Where(e => e.Id != UserId && e.Id != admin.UserId).ToListAsync());
            }

            return View(await _db.ApplicationUsers.Where(e => e.Id != UserId).ToListAsync());
        }

        // POST
        public async Task<IActionResult> LockUnLock(string id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var user = await _db.ApplicationUsers.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            if (user.LockoutEnd == null || user.LockoutEnd < DateTime.Now)
            {
                user.LockoutEnd = DateTime.Now.AddYears(1000);
            }
            else
            {
                user.LockoutEnd = DateTime.Now;
            }

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
