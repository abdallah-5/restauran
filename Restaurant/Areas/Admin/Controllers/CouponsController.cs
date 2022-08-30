using AliEns.Data;
using AliEns.Models;
using AliEns.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AliEns.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CouponsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IToastNotification _toastNotification;

        public CouponsController(ApplicationDbContext db, IToastNotification toastNotification)
        {
            _db = db;
            _toastNotification = toastNotification;
        }

        // GET
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var couponsList = await _db.Coupons.ToListAsync();
            var model = new List<CouponViewModel>();

            foreach (var coupon in couponsList)
            {
                model.Add(new CouponViewModel
                {
                    Id = coupon.Id,
                    Name = coupon.Name,
                    CouponType = coupon.CouponType,
                    Discount = coupon.Discount,
                    IsActive = coupon.IsActive,
                    MinimumAmount = coupon.MinimumAmount,
                    Picture = coupon.Picture
                });
            }

            return View(model);
        }

        // GET
        [HttpGet]
        public IActionResult Create()
        {
            CouponViewModel model = new CouponViewModel();
            return View("CouponView", model);
        }

        // POST
        [HttpPost]
        public async Task<IActionResult> Create(CouponViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("CouponView", model);
            }

            var isExsistCoupon = await _db.Coupons.Where(e => e.Name == model.Name).FirstOrDefaultAsync();

            if (isExsistCoupon != null)
            {
                ModelState.AddModelError("Name", "This Coupon Is Already Exist");
                return View("CouponView", model);
            }

            var files = Request.Form.Files;

            if (!files.Any())
            {
                ModelState.AddModelError("Picture", "The Image Is Required.");
                return View("CouponView", model);
            }

            var img = files.FirstOrDefault();
            var allowedExtentions = new List<string> { ".png", ".jpg", ".jfif" };

            if (!allowedExtentions.Contains(Path.GetExtension(img.FileName).ToLower()))
            {
                ModelState.AddModelError("Picture", "This Extinsion Is Not Allowed.");
                return View("CouponView", model);
            }

            if (img.Length > 1048576)
            {
                ModelState.AddModelError("Picture", "The Image Size Alloed Is 1MB");
                return View("CouponView", model);
            }

            using var dataStream = new MemoryStream();
            await img.CopyToAsync(dataStream);

            var newCoupon = new Coupon()
            {
                Name = model.Name,
                Picture = dataStream.ToArray(),
                CouponType = model.CouponType,
                Discount = model.Discount,
                MinimumAmount = model.MinimumAmount,
                IsActive = model.IsActive
            };

            _db.Coupons.Add(newCoupon);
            await _db.SaveChangesAsync();

            _toastNotification.AddSuccessToastMessage("Coupon Created Successfly.");

            return RedirectToAction(nameof(Index));
        }

        // GET
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var isCouponExist = await _db.Coupons.FindAsync(id);

            if (isCouponExist == null)
            {
                return NotFound();
            }

            var model = new CouponViewModel()
            {
                Id = isCouponExist.Id,
                Name = isCouponExist.Name,
                Discount = isCouponExist.Discount,
                CouponType = isCouponExist.CouponType,
                IsActive = isCouponExist.IsActive,
                MinimumAmount = isCouponExist.MinimumAmount,
                Picture = isCouponExist.Picture
            };

            return View(model);
        }

        // GET
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var isCouponExist = await _db.Coupons.FindAsync(id);

            if (isCouponExist == null)
            {
                return NotFound();
            }

            var model = new CouponViewModel()
            {
                Id = isCouponExist.Id,
                Name = isCouponExist.Name,
                Discount = isCouponExist.Discount,
                CouponType = isCouponExist.CouponType,
                IsActive = isCouponExist.IsActive,
                MinimumAmount = isCouponExist.MinimumAmount,
                Picture = isCouponExist.Picture
            };

            return View("CouponView", model);
        }

        // POST
        [HttpPost]
        public async Task<IActionResult> Edit(CouponViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("CouponView", model);
            }

            var isExsistCoupon = await _db.Coupons.Where(e => e.Name == model.Name && e.Id != model.Id).FirstOrDefaultAsync();

            if (isExsistCoupon != null)
            {
                ModelState.AddModelError("Name", "This Coupon Is Already Exist");
                return View("CouponView", model);
            }

            var updatedCoupon = await _db.Coupons.Where(e => e.Id == model.Id).FirstAsync();
            var files = Request.Form.Files;

            if (files.Any())
            {
                var img = files.FirstOrDefault();
                var allowedExtentions = new List<string> { ".png", ".jpg" };

                if (!allowedExtentions.Contains(Path.GetExtension(img.FileName).ToLower()))
                {
                    ModelState.AddModelError("Picture", "This Extinsion Is Not Allowed.");
                    return View("CouponView", model);
                }

                if (img.Length > 1048576)
                {
                    ModelState.AddModelError("Picture", "The Image Size Alloed Is 1MB");
                    return View("CouponView", model);
                }

                using var dataStream = new MemoryStream();
                await img.CopyToAsync(dataStream);
                updatedCoupon.Picture = dataStream.ToArray();
            }

            updatedCoupon.Name = model.Name;
            updatedCoupon.CouponType = model.CouponType;
            updatedCoupon.Discount = model.Discount;
            updatedCoupon.MinimumAmount = model.MinimumAmount;
            updatedCoupon.IsActive = model.IsActive;

            _db.Coupons.Update(updatedCoupon);
            await _db.SaveChangesAsync();

            _toastNotification.AddSuccessToastMessage("Coupon Updated Successfly.");

            return RedirectToAction(nameof(Index));
        }

        // POST
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var isCouponExist = await _db.Coupons.FindAsync(id);

            if (isCouponExist == null)
            {
                return NotFound();
            }

            _db.Coupons.Remove(isCouponExist);
            await _db.SaveChangesAsync();

            _toastNotification.AddSuccessToastMessage("Coupon Deleted Successfly.");

            return Ok();
        }

        // POST
        public IActionResult DeleteAll(IEnumerable<string> ID)
        {
            ViewBag.msg = string.Empty;

            try
            {
                List<string> st = ID.ToList();

                if (st.Count > 0)
                {
                    foreach (var id in st)
                    {
                        int deleteId = 0;
                        try
                        {
                            deleteId = int.Parse(id);
                        }
                        catch { }

                        if (deleteId > 0)
                        {
                            var coupon = _db.Coupons.First(e => e.Id == deleteId);
                            if (coupon != null)
                            {
                                _db.Coupons.Remove(coupon);
                                _db.SaveChanges();
                            }
                        }
                    }
                }
                else
                {
                    ViewBag.msg = "This Category Has One Or More Sub Category Under Him You Must Delete It First";
                }
            }
            catch (Exception ex)
            {
                ViewBag.msg = ex.Message;
            }

            _toastNotification.AddSuccessToastMessage("Category(s) Deleted Successfly.");

            return RedirectToAction(nameof(Index));
        }
    }
}
