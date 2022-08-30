using AliEns.Data;
using AliEns.Models;
using AliEns.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AliEns.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IToastNotification _toastNotification;

        public CategoriesController(ApplicationDbContext db, IToastNotification toastNotification)
        {
            _db = db;
            _toastNotification = toastNotification;
        }

        // GET
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var categoriesList = await _db.Categories.OrderBy(e => e.Name).ToListAsync();
            List<CategoryViewModel> model = new List<CategoryViewModel>();

            foreach (var category in categoriesList)
            {
                model.Add(new CategoryViewModel
                {
                    Id = category.Id,
                    Name = category.Name
                });
            }

            return View(model);
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryViewModel model)
        {

            var categoriesList = await _db.Categories.OrderBy(e => e.Name).ToListAsync();
            List<CategoryViewModel> categoryVMList = new List<CategoryViewModel>();

            foreach (var item in categoriesList)
            {
                categoryVMList.Add(new CategoryViewModel
                {
                    Id = item.Id,
                    Name = item.Name
                });
            }

            if (!ModelState.IsValid)
            {
                return View(nameof(Index), categoryVMList);
            }

            var category = await _db.Categories.FirstOrDefaultAsync(e => e.Name == model.Name);

            if (category != null)
            {
                ModelState.AddModelError("Name", "This Category Is Already Exist");
                return View(nameof(Index), categoryVMList);
            }

            Category newCategory = new Category
            {
                Name = model.Name
            };

            _db.Categories.Add(newCategory);
            await _db.SaveChangesAsync();

            _toastNotification.AddSuccessToastMessage("Category Created Successfly.");

            return RedirectToAction(nameof(Index));
        }

        // GET
        [HttpGet]
        public async Task<IActionResult> Details(byte? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var category = await _db.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            CategoryViewModel categoryViewModel = new CategoryViewModel
            {
                Id = category.Id,
                Name = category.Name
            };

            return View(categoryViewModel);
        }

        // GET
        [HttpGet]
        public async Task<IActionResult> Edit(byte? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var category = await _db.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            CategoryViewModel categoryViewModel = new CategoryViewModel
            {
                Id = category.Id,
                Name = category.Name
            };

            return View(categoryViewModel);
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var category = await _db.Categories.FirstOrDefaultAsync(e => e.Name == model.Name && e.Id != model.Id);

            if (category != null)
            {
                ModelState.AddModelError("Name", "This Category Is Already Exist");
                return View(model);
            }

            Category updatedCategory = new Category
            {
                Id = model.Id,
                Name = model.Name
            };

            _db.Categories.Update(updatedCategory);
            await _db.SaveChangesAsync();

            _toastNotification.AddSuccessToastMessage("Category Updated Successfly.");

            return RedirectToAction(nameof(Index));
        }

        // POST
        public async Task<IActionResult> Delete(byte? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var category = await _db.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            _db.Categories.Remove(category);
            await _db.SaveChangesAsync();

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
                            var category = _db.Categories.First(e => e.Id == deleteId);
                            if (category != null)
                            {
                                _db.Categories.Remove(category);
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
