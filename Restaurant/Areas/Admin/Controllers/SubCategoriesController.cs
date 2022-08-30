using AliEns.Data;
using AliEns.Models;
using AliEns.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AliEns.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SubCategoriesController : Controller
    {
        [TempData]
        public string StatusMessageTemp { get; set; }

        private readonly ApplicationDbContext _db;
        private readonly IToastNotification _toastNotification;

        public SubCategoriesController(ApplicationDbContext db, IToastNotification toastNotification)
        {
            _db = db;
            _toastNotification = toastNotification;
        }

        // GET
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var subCategories = await _db.SubCategories.Include(e => e.Category).OrderBy(e => e.Name).ToListAsync();
            List<SubCategoryViewModel> model = new List<SubCategoryViewModel>();
            
            foreach (var item in subCategories)
            {
                model.Add(new SubCategoryViewModel
                {
                    SubCategory = item
                });
            }

            return View(model);
        }

        // GET
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new SubCategoryViewModel()
            {
                CategoriesList = await _db.Categories.ToListAsync(),
                SubCategory = new SubCategory()
            };

            return View("SubCategoryView", model);
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var isSubCategoryExist = await _db.SubCategories
                    .Include(e => e.Category)
                    .Where(e => e.Category.Id == model.SubCategory.CategoryId && e.Name == model.SubCategory.Name)
                    .FirstOrDefaultAsync();

                if (isSubCategoryExist != null)
                {
                    StatusMessageTemp = "Error : This Sub Category Is Already Exist Unser " + isSubCategoryExist.Category.Name + " Category";
                }
                else
                {
                    var subCategory = new SubCategory()
                    {
                        Name = model.SubCategory.Name,
                        CategoryId = model.SubCategory.CategoryId
                    };

                    _db.SubCategories.Add(subCategory);
                    await _db.SaveChangesAsync();

                    _toastNotification.AddSuccessToastMessage("Sub Category Created Successfly");

                    return RedirectToAction(nameof(Index));
                }
            }
            
            var subCategoryVM = new SubCategoryViewModel()
            {
                CategoriesList = await _db.Categories.ToListAsync(),
                SubCategory = new SubCategory(),
                StatusMessage = StatusMessageTemp
            };

            return View("SubCategoryView", subCategoryVM);
        }

        // GET
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var isExistSubCategory = await _db.SubCategories.FindAsync(id);
            isExistSubCategory.Category = await _db.Categories.FirstAsync(e => e.Id == isExistSubCategory.CategoryId);

            if (isExistSubCategory == null)
            {
                return NotFound();
            }

            var model = new SubCategoryViewModel()
            {
                SubCategory = isExistSubCategory
            };

            return View(model);
        }

        // GET
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var isExistSubCategory = await _db.SubCategories.FindAsync(id);

            if (isExistSubCategory == null)
            {
                return NotFound();
            }

            var model = new SubCategoryViewModel()
            {
                CategoriesList = await _db.Categories.Where(e => e.Id == isExistSubCategory.CategoryId).ToListAsync(),
                SubCategory = isExistSubCategory
            };

            return View("SubCategoryView", model);
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SubCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var isSubCategoryExist = await _db.SubCategories
                    .Include(e => e.Category)
                    .Where(e => e.Category.Id == model.SubCategory.CategoryId && e.Name == model.SubCategory.Name)
                    .FirstOrDefaultAsync();

                if (isSubCategoryExist != null)
                {
                    StatusMessageTemp = "Error : This Sub Category Is Already Exist Unser " + isSubCategoryExist.Category.Name + " Category";
                }
                else
                {
                    var subCategory = new SubCategory()
                    {
                        Id = model.SubCategory.Id,
                        Name = model.SubCategory.Name,
                        CategoryId = model.SubCategory.CategoryId
                    };

                    _db.SubCategories.Update(subCategory);
                    await _db.SaveChangesAsync();

                    _toastNotification.AddSuccessToastMessage("Sub Category Updated Successfly");

                    return RedirectToAction(nameof(Index));
                }
            }

            var subCategoryVM = new SubCategoryViewModel()
            {
                CategoriesList = await _db.Categories.ToListAsync(),
                SubCategory = model.SubCategory,
                StatusMessage = StatusMessageTemp
            };

            return View("SubCategoryView", subCategoryVM);
        }

        // POST
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var subCategory = await _db.SubCategories.FindAsync(id);

            if (subCategory == null)
            {
                return NotFound();
            }

            _db.SubCategories.Remove(subCategory);
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
                        catch {}

                        if (deleteId > 0)
                        {
                            var subCategory = _db.SubCategories.First(e => e.Id == deleteId);
                            if (subCategory != null)
                            {
                                _db.SubCategories.Remove(subCategory);
                                _db.SaveChanges();
                            }
                        }
                    }
                }
                else
                {
                    ViewBag.msg = "No Sub Category Selected !";
                }
            }
            catch (Exception ex)
            {
                ViewBag.msg = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET
        public async Task<IActionResult> GetSubCategories(int id)
        {
            List<SubCategory> subCategories = new List<SubCategory>();
            subCategories = await _db.SubCategories.Where(e => e.CategoryId == id).ToListAsync();

            return Json(new SelectList(subCategories, "Id", "Name"));
        }
    }
}
