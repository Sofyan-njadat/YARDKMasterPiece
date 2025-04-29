using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using YARDK.Models;

namespace YARDK.Controllers
{
    public class AdminController : Controller
    {
        private readonly MyDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AdminController(MyDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // التحقق من صلاحيات المستخدم كمسؤول
        private bool IsAdmin()
        {
            return Request.Cookies["UserRole"]?.ToString() == "Admin";
        }

        // التحقق من صلاحيات المستخدم قبل الوصول إلى صفحات الإدارة
        private IActionResult CheckAdminAccess()
        {
            // التحقق من تسجيل الدخول
            if (Request.Cookies["UserId"] == null)
            {
                return RedirectToAction("Login", "User");
            }

            // التحقق من صلاحيات المسؤول
            if (!IsAdmin())
            {
                return RedirectToAction("AccessDenied", "User");
            }

            return null; // السماح بالوصول
        }

        // GET: Admin/Dashboard
        public IActionResult Dashboard()
        {
            // التحقق من صلاحيات المستخدم
            var accessCheck = CheckAdminAccess();
            if (accessCheck != null)
                return accessCheck;

            return View();
        }

        // GET: Admin/Categories
        public async Task<IActionResult> Categories()
        {
            // التحقق من صلاحيات المستخدم
            var accessCheck = CheckAdminAccess();
            if (accessCheck != null)
                return accessCheck;

            var categories = await _context.Categories.ToListAsync();
            return View(categories);
        }

        // GET: Admin/CreateCategory
        public IActionResult CreateCategory()
        {
            // التحقق من صلاحيات المستخدم
            var accessCheck = CheckAdminAccess();
            if (accessCheck != null)
                return accessCheck;

            return View();
        }

        // POST: Admin/CreateCategory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(Category category, IFormFile categoryImage)
        {
            // التحقق من صلاحيات المستخدم
            var accessCheck = CheckAdminAccess();
            if (accessCheck != null)
                return accessCheck;

            if (ModelState.IsValid)
            {
                // معالجة الصورة إذا تم تحميلها
                if (categoryImage != null && categoryImage.Length > 0)
                {
                    // إنشاء اسم فريد للصورة
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(categoryImage.FileName);
                    
                    // تحديد مسار المجلد للصور
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "categories");
                    
                    // التأكد من وجود المجلد
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }
                    
                    // حفظ الصورة
                    string filePath = Path.Combine(uploadsFolder, fileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await categoryImage.CopyToAsync(fileStream);
                    }
                    
                    // تعيين مسار الصورة
                    category.ImageUrl = "/uploads/categories/" + fileName;
                }

                // إضافة الفئة إلى قاعدة البيانات
                _context.Add(category);
                await _context.SaveChangesAsync();
                
                return RedirectToAction(nameof(Categories));
            }
            return View(category);
        }

        // GET: Admin/EditCategory/5
        public async Task<IActionResult> EditCategory(int? id)
        {
            // التحقق من صلاحيات المستخدم
            var accessCheck = CheckAdminAccess();
            if (accessCheck != null)
                return accessCheck;

            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: Admin/EditCategory/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(int id, Category category, IFormFile categoryImage)
        {
            // التحقق من صلاحيات المستخدم
            var accessCheck = CheckAdminAccess();
            if (accessCheck != null)
                return accessCheck;

            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // الحصول على الفئة الحالية من قاعدة البيانات
                    var existingCategory = await _context.Categories.FindAsync(id);
                    if (existingCategory == null)
                    {
                        return NotFound();
                    }

                    // تحديث البيانات
                    existingCategory.CategoryName = category.CategoryName;
                    existingCategory.Description = category.Description;

                    // معالجة الصورة إذا تم تحميلها
                    if (categoryImage != null && categoryImage.Length > 0)
                    {
                        // إنشاء اسم فريد للصورة
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(categoryImage.FileName);
                        
                        // تحديد مسار المجلد للصور
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "categories");
                        
                        // التأكد من وجود المجلد
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }
                        
                        // حفظ الصورة
                        string filePath = Path.Combine(uploadsFolder, fileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await categoryImage.CopyToAsync(fileStream);
                        }
                        
                        // حذف الصورة القديمة إذا كانت موجودة
                        if (!string.IsNullOrEmpty(existingCategory.ImageUrl) && existingCategory.ImageUrl.StartsWith("/uploads/"))
                        {
                            string oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, existingCategory.ImageUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }
                        
                        // تعيين مسار الصورة الجديدة
                        existingCategory.ImageUrl = "/uploads/categories/" + fileName;
                    }

                    // تحديث قاعدة البيانات
                    _context.Update(existingCategory);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Categories));
            }
            return View(category);
        }

        // GET: Admin/DeleteCategory/5
        public async Task<IActionResult> DeleteCategory(int? id)
        {
            // التحقق من صلاحيات المستخدم
            var accessCheck = CheckAdminAccess();
            if (accessCheck != null)
                return accessCheck;

            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Admin/DeleteCategory/5
        [HttpPost, ActionName("DeleteCategory")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategoryConfirmed(int id)
        {
            // التحقق من صلاحيات المستخدم
            var accessCheck = CheckAdminAccess();
            if (accessCheck != null)
                return accessCheck;

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            // حذف الصورة المرتبطة بالفئة
            if (!string.IsNullOrEmpty(category.ImageUrl) && category.ImageUrl.StartsWith("/uploads/"))
            {
                string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, category.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Categories));
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
} 