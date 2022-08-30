using AliEns.Data;
using AliEns.Models;
using AliEns.Utility;
using AliEns.ViewModels;
using Microsoft.AspNetCore.Hosting;
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
    public class MenuItemsController : Controller
    {
        [BindProperty]
        public MenuItemViewModel MenuItemVM { get; set; }

        [TempData]
        public string StatusMessageTemp { get; set; }

        private readonly ApplicationDbContext _db;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public MenuItemsController(ApplicationDbContext db,
            IToastNotification toastNotification,
            IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _toastNotification = toastNotification;
            _webHostEnvironment = webHostEnvironment;

            MenuItemVM = new MenuItemViewModel()
            {
                MenuItem = new MenuItem(),
                CategoriesList = _db.Categories.ToList()
            };
        }

        // GET
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var menuItemList = await _db.MenuItems.Include(e => e.Category).Include(e => e.SubCategory).OrderBy(e => e.Name).ToListAsync();

            return View(menuItemList);
        }

        // GET
        [HttpGet]
        public IActionResult Create()
        {
            return View("MenuItemView", MenuItemVM);
        }

        // POST
        [HttpPost]
        [ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost()
        {
            if (ModelState.IsValid)
            {
                var files = HttpContext.Request.Form.Files;
                string imagePath = @"\img\default-product-image.png";

                if (files.Any())
                {
                    string webRootPath = _webHostEnvironment.WebRootPath;
                    string imageName = DateTime.Now.ToFileTime().ToString() + Path.GetExtension(files[0].FileName);

                    FileStream fileStream = new FileStream(Path.Combine(webRootPath, "img", imageName), FileMode.Create);
                    files[0].CopyTo(fileStream);

                    imagePath = @"\img\" + imageName;
                }

                MenuItemVM.MenuItem.Image = imagePath;

                var isExistMenuItem = await _db.MenuItems
                    .Where(e => e.CategoryId == MenuItemVM.MenuItem.CategoryId &&
                                e.SubCategoryId == MenuItemVM.MenuItem.SubCategoryId &&
                                e.Name == MenuItemVM.MenuItem.Name).FirstOrDefaultAsync();

                if (isExistMenuItem != null)
                {
                    StatusMessageTemp = "Error : This Menu Item Is Already Exist Under These Category And Sub Category";
                }
                else
                {
                    _db.Add(MenuItemVM.MenuItem);
                    await _db.SaveChangesAsync();

                    _toastNotification.AddSuccessToastMessage("Menu Item Created Successfly.");

                    return RedirectToAction(nameof(Index));
                }
            }

            MenuItemVM.StatusMessage = StatusMessageTemp;
            MenuItemVM.CategoriesList = await _db.Categories.ToListAsync();

            return View("MenuItemView", MenuItemVM);
        }

        // GET
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var menuItem = await _db.MenuItems.Include(e => e.Category).Include(e => e.SubCategory).FirstOrDefaultAsync(e => e.Id == id);

            if (menuItem == null)
            {
                return NotFound();
            }

            menuItem.Description = SD.ConvertToRawHtml(menuItem.Description);
            MenuItemVM.MenuItem = menuItem;

            return View(MenuItemVM);
        }

        // GET
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var menuItem = await _db.MenuItems.Include(e => e.Category).Include(e => e.SubCategory).FirstOrDefaultAsync(e => e.Id == id);

            if (menuItem == null)
            {
                return NotFound();
            }

            MenuItemVM.MenuItem = menuItem;

            return View("MenuItemView", MenuItemVM);
        }

        // POST
        [HttpPost]
        [ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost()
        {
            if (ModelState.IsValid)
            {
                var files = HttpContext.Request.Form.Files;
                string imagePath = MenuItemVM.MenuItem.Image;

                if (files.Any())
                {
                    string webRootPath = _webHostEnvironment.WebRootPath;
                    string imageName = DateTime.Now.ToFileTime().ToString() + Path.GetExtension(files[0].FileName);

                    FileStream fileStream = new FileStream(Path.Combine(webRootPath, "img", imageName), FileMode.Create);
                    files[0].CopyTo(fileStream);

                    imagePath = @"\img\" + imageName;
                }

                MenuItemVM.MenuItem.Image = imagePath;

                var isExistMenuItem = await _db.MenuItems
                    .Where(e => e.CategoryId == MenuItemVM.MenuItem.CategoryId &&
                                e.SubCategoryId == MenuItemVM.MenuItem.SubCategoryId &&
                                e.Name == MenuItemVM.MenuItem.Name &&
                                e.Id != MenuItemVM.MenuItem.Id).FirstOrDefaultAsync();

                if (isExistMenuItem != null)
                {
                    StatusMessageTemp = "Error : This Menu Item Is Already Exist Under These Category And Sub Category";
                }
                else
                {
                    _db.Update(MenuItemVM.MenuItem);
                    await _db.SaveChangesAsync();

                    _toastNotification.AddSuccessToastMessage("Menu Item Updated Successfly.");

                    return RedirectToAction(nameof(Index));
                }
            }

            MenuItemVM.StatusMessage = StatusMessageTemp;
            MenuItemVM.CategoriesList = await _db.Categories.ToListAsync();

            return View(MenuItemVM);
        }

        // POST
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var menuItem = await _db.MenuItems.Include(e => e.Category).Include(e => e.SubCategory).FirstOrDefaultAsync(e => e.Id == id);

            if (menuItem == null)
            {
                return NotFound();
            }

            _db.Remove(menuItem);
            await _db.SaveChangesAsync();

            _toastNotification.AddSuccessToastMessage("Menu Item Deleted Successfly.");

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
                            var menuItem = _db.MenuItems.First(e => e.Id == deleteId);
                            if (menuItem != null)
                            {
                                _db.MenuItems.Remove(menuItem);
                                _db.SaveChanges();
                            }
                        }
                    }
                }
                else
                {
                    ViewBag.msg = "No Menu Item Selected !";
                }
            }
            catch (Exception ex)
            {
                ViewBag.msg = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
