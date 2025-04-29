using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using YARDK.Models;
using System.Collections.Generic;
using System.IO; // Required for file operations
using Microsoft.AspNetCore.Hosting; // Required for IWebHostEnvironment

namespace YARDK.Controllers
{
    public class UserController : Controller
    {
        private readonly MyDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment; // Inject IWebHostEnvironment

        public UserController(MyDbContext context, IWebHostEnvironment hostEnvironment) // Modify constructor
        {
            _context = context;
            _hostEnvironment = hostEnvironment; // Assign injected dependency
        }

        // التحقق من صلاحيات المستخدم كمسؤول
        private bool IsAdmin()
        {
            return Request.Cookies["UserRole"]?.ToString() == "Admin";
        }

        // دالة للتحقق من صلاحيات المستخدم قبل الوصول إلى صفحات الإدارة
        public IActionResult CheckAdminAccess()
        {
            // التحقق من تسجيل الدخول
            if (Request.Cookies["UserId"] == null)
            {
                return RedirectToAction("Login");
            }

            // التحقق من صلاحيات المسؤول
            if (!IsAdmin())
            {
                return RedirectToAction("AccessDenied");
            }

            return null; // السماح بالوصول
        }

        // صفحة رفض الوصول
        public IActionResult AccessDenied()
        {
            return View();
        }

        // GET: UserController
        public ActionResult Regester()
        {
            return View();
        }

        // POST: UserController/Regester
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Regester(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Check if email already exists
                    var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                    if (existingUser != null)
                    {
                        ModelState.AddModelError("Email", "This email is already registered");
                        return View(model);
                    }

                    // Create new user
                    var user = new User
                    {
                        Name = model.Name,
                        Email = model.Email,
                        Phone = model.Phone,
                        Password = model.Password, 
                        // نترك Role ليأخذ القيمة الافتراضية من قاعدة البيانات
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    };

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();

                    // بعد الحفظ، نحصل على قيمة Role المحددة من قاعدة البيانات
                    string userRole = user.Role ?? "User";

                    // Store user information in cookies instead of session
                    var cookieOptions = new CookieOptions
                    {
                        Expires = DateTime.Now.AddDays(30),
                        HttpOnly = true,
                        IsEssential = true,
                        SameSite = SameSiteMode.Lax
                    };

                    Response.Cookies.Append("UserId", user.Id.ToString(), cookieOptions);
                    Response.Cookies.Append("UserName", user.Name, cookieOptions);
                    Response.Cookies.Append("UserRole", userRole, cookieOptions);

                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    // Log the error details
                    var innerEx = ex.InnerException != null ? ex.InnerException.Message : "No inner exception";
                    ModelState.AddModelError("", $"Registration failed: {ex.Message}. Details: {innerEx}");
                }
            }

            return View(model);
        }

        // GET: UserController/Login
        public ActionResult Login()
        {
            return View();
        }

        // POST: UserController/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Find user by email and password
                    var user = await _context.Users.FirstOrDefaultAsync(
                        u => u.Email == model.Email && u.Password == model.Password);

                    if (user == null)
                    {
                        ModelState.AddModelError("", "Invalid email or password");
                        return View(model);
                    }

                    if (user.IsActive != true)
                    {
                        ModelState.AddModelError("", "This account is inactive");
                        return View(model);
                    }

                    // الحصول على قيمة Role، وإذا كانت فارغة استخدم "User" كقيمة افتراضية
                    string userRole = user.Role ?? "User";

                    // Store user information in cookies instead of session
                    var cookieOptions = new CookieOptions
                    {
                        HttpOnly = true,
                        IsEssential = true,
                        SameSite = SameSiteMode.Lax
                    };
                    
                    // Set cookie expiration based on RememberMe option
                    if (model.RememberMe)
                    {
                        cookieOptions.Expires = DateTime.Now.AddDays(30);
                    }
                    
                    Response.Cookies.Append("UserId", user.Id.ToString(), cookieOptions);
                    Response.Cookies.Append("UserName", user.Name, cookieOptions);
                    Response.Cookies.Append("UserRole", userRole, cookieOptions);

                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Login failed: {ex.Message}");
                }
            }

            return View(model);
        }

        // GET: UserController/Logout
        public IActionResult Logout()
        {
            // Clear cookies instead of session
            Response.Cookies.Delete("UserId");
            Response.Cookies.Delete("UserName");
            Response.Cookies.Delete("UserRole");

            return RedirectToAction("Index", "Home");
        }

        public async Task<ActionResult> Profile()
        {
            // Check if user is logged in using cookies
            if (!int.TryParse(Request.Cookies["UserId"], out int userId)) // Safer parsing
            {
                return RedirectToAction("Login");
            }

            // Fetch user data from the database
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                // Handle case where user ID from cookie doesn't exist in DB
                Response.Cookies.Delete("UserId"); // Clear invalid cookie
                Response.Cookies.Delete("UserName");
                Response.Cookies.Delete("UserRole");
                return RedirectToAction("Login");
            }

            // Calculate Counts
            int adsCount = await _context.Products.CountAsync(p => p.SellerId == userId && p.Quantity > 0); // Count only active ads
            int ordersCount = await _context.Orders.CountAsync(o => o.UserId == userId);

            // Pass user data and counts to the view
            ViewBag.UserEmail = user.Email;
            ViewBag.UserPhone = user.Phone;
            ViewBag.UserName = user.Name;
            ViewBag.ProfileImageUrl = user.ProfileImageUrl; // Pass profile image URL
            ViewBag.AdsCount = adsCount;
            ViewBag.OrdersCount = ordersCount;
            // Pass AccountType from cookie as it seems not stored in DB model based on usage
            ViewBag.AccountType = Request.Cookies["AccountType"] ?? "Personal Account";

            return View(); // Return the view with data
        }

        public async Task<ActionResult> MyAds()
        {
            // Check if user is logged in using cookies
            if (Request.Cookies["UserId"] == null)
            {
                return RedirectToAction("Login");
            }

            // Get user id from cookies
            int userId = int.Parse(Request.Cookies["UserId"]);

            // Get all of the user's products, including those with quantity = 0
            var userProducts = await _context.Products
                .Where(p => p.SellerId == userId)
                .Include(p => p.ProductImages)
                .OrderByDescending(p => p.Quantity > 0) // الإعلانات الفعالة أولاً
                .ThenByDescending(p => p.CreatedAt) // ثم حسب تاريخ الإضافة
                .ToListAsync();

            return View(userProducts);
        }
        
        public ActionResult PaymentMethod()
        {
            // Check if user is logged in using cookies
            if (Request.Cookies["UserId"] == null)
            {
                return RedirectToAction("Login");
            }

            return View();
        }

        public async Task<ActionResult> MyOrders()
        {
            // التحقق من تسجيل دخول المستخدم
            if (Request.Cookies["UserId"] == null)
            {
                return RedirectToAction("Login");
            }

            // الحصول على معرف المستخدم من الكوكيز
            int userId = int.Parse(Request.Cookies["UserId"]);

            // الحصول على طلبات المستخدم
            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return View(orders);
        }

        public async Task<ActionResult> OrderDetails(int id)
        {
            // التحقق من تسجيل دخول المستخدم
            if (Request.Cookies["UserId"] == null)
            {
                return RedirectToAction("Login");
            }

            // الحصول على معرف المستخدم من الكوكيز
            int userId = int.Parse(Request.Cookies["UserId"]);

            // الحصول على تفاصيل الطلب
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: User/UpdateProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Add IFormFile parameter for the image
        public async Task<IActionResult> UpdateProfile(string UserName, string UserEmail, string UserPhone, string Password, string ConfirmPassword, string AccountType, IFormFile ProfileImage)
        {
            // Check if user is logged in
            if (!int.TryParse(Request.Cookies["UserId"], out int userId))
            {
                return RedirectToAction("Login");
            }

            // Fetch the current user from the database
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                // Should not happen if profile page loaded, but handle defensively
                return RedirectToAction("Login");
            }

            // --- Handle Profile Image Upload ---
            string oldImagePath = user.ProfileImageUrl; // Store old path for potential deletion
            if (ProfileImage != null && ProfileImage.Length > 0)
            {
                // Basic validation (you might want more robust validation)
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(ProfileImage.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension))
                {
                    TempData["ErrorMessage"] = "Invalid image file type. Allowed types: jpg, jpeg, png, gif.";
                    return RedirectToAction("Profile");
                }

                // Define target path
                string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads", "profile_imgs");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Generate unique filename
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(ProfileImage.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Save new image
                try
                {
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await ProfileImage.CopyToAsync(fileStream);
                    }
                    // Update user's ProfileImageUrl with the relative path
                    user.ProfileImageUrl = "/uploads/profile_imgs/" + uniqueFileName;
                }
                catch (Exception ex)
                {
                     // Log error
                    TempData["ErrorMessage"] = "Error uploading profile image.";
                    // Optionally, decide if you want to proceed without the image update or return
                     return RedirectToAction("Profile"); // Stop processing if image upload fails
                }
            }
            // --- End Handle Profile Image Upload ---


            // Validate other input
            bool dataChanged = (user.ProfileImageUrl != oldImagePath); // Check if image changed

            if (!string.IsNullOrWhiteSpace(Password))
            {
                if (Password != ConfirmPassword)
                {
                    TempData["ErrorMessage"] = "Password and Confirmation Password do not match.";
                    // If image was uploaded, delete it before returning
                    if (user.ProfileImageUrl != oldImagePath && !string.IsNullOrEmpty(user.ProfileImageUrl))
                    {
                         string newFilePath = Path.Combine(_hostEnvironment.WebRootPath, user.ProfileImageUrl.TrimStart('/'));
                         if (System.IO.File.Exists(newFilePath)) System.IO.File.Delete(newFilePath);
                         user.ProfileImageUrl = oldImagePath; // Revert path
                    }
                    return RedirectToAction("Profile");
                }
                user.Password = Password; // WARNING: Plain text password
                dataChanged = true;
            }

            if (user.Name != UserName) { user.Name = UserName; dataChanged = true; }
            if (user.Email != UserEmail)
            {
                 var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == UserEmail && u.Id != userId);
                 if (existingUser != null)
                 {
                     TempData["ErrorMessage"] = "This email address is already in use by another account.";
                      // Revert image if changed
                     if (user.ProfileImageUrl != oldImagePath && !string.IsNullOrEmpty(user.ProfileImageUrl)) {
                          string newFilePath = Path.Combine(_hostEnvironment.WebRootPath, user.ProfileImageUrl.TrimStart('/'));
                          if (System.IO.File.Exists(newFilePath)) System.IO.File.Delete(newFilePath);
                          user.ProfileImageUrl = oldImagePath;
                     }
                     return RedirectToAction("Profile");
                 }
                user.Email = UserEmail;
                dataChanged = true;
            }
            if (user.Phone != UserPhone) { user.Phone = UserPhone; dataChanged = true; }


            // Only save changes if something actually changed
            if (dataChanged)
            {
                 try
                 {
                    _context.Users.Update(user); // Explicitly mark as updated
                    await _context.SaveChangesAsync();

                    // Delete old image *after* successful save
                    if (user.ProfileImageUrl != oldImagePath && !string.IsNullOrEmpty(oldImagePath))
                    {
                        string oldFilePath = Path.Combine(_hostEnvironment.WebRootPath, oldImagePath.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                             try { System.IO.File.Delete(oldFilePath); } catch { /* Log deletion error? */ }
                        }
                    }

                    TempData["SuccessMessage"] = "Profile updated successfully.";
                 }
                 catch (DbUpdateException ex)
                 {
                    // Log error
                     TempData["ErrorMessage"] = "An error occurred while updating your profile. Please try again.";
                      // Revert image if changed and save failed
                     if (user.ProfileImageUrl != oldImagePath && !string.IsNullOrEmpty(user.ProfileImageUrl)) {
                          string newFilePath = Path.Combine(_hostEnvironment.WebRootPath, user.ProfileImageUrl.TrimStart('/'));
                          if (System.IO.File.Exists(newFilePath)) System.IO.File.Delete(newFilePath);
                          // Don't revert user.ProfileImageUrl here as the DB change failed
                     }
                    return RedirectToAction("Profile");
                 }
            }
            else
            {
                 TempData["InfoMessage"] = "No changes were detected.";
            }

            // Update cookies
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                IsEssential = true,
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.Now.AddDays(30) // Reset expiration
            };

            Response.Cookies.Append("UserName", user.Name, cookieOptions);
            Response.Cookies.Append("AccountType", AccountType ?? "Personal Account", cookieOptions);

            return RedirectToAction("Profile");
        }
    }
}