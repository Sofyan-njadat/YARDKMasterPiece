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
            
            // Ensure PaymentMethod table exists
            EnsurePaymentMethodTableExists();
        }

        // التحقق من صلاحيات المستخدم كمسؤول
        private bool IsAdmin()
        {
            return Request.Cookies["UserRole"]?.ToString() == "Admin";
        }

        private void EnsurePaymentMethodTableExists()
        {
            try
            {
                // Check if the PaymentMethod table exists
                bool tableExists = false;
                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = @"
                        SELECT COUNT(*) 
                        FROM INFORMATION_SCHEMA.TABLES 
                        WHERE TABLE_SCHEMA = 'dbo' 
                        AND TABLE_NAME = 'PaymentMethods'";
                    
                    _context.Database.OpenConnection();
                    var result = command.ExecuteScalar();
                    tableExists = Convert.ToInt32(result) > 0;
                }
                
                // If the table doesn't exist, create it
                if (!tableExists)
                {
                    using (var command = _context.Database.GetDbConnection().CreateCommand())
                    {
                        command.CommandText = @"
                            CREATE TABLE [dbo].[PaymentMethods] (
                                [ID] INT IDENTITY(1,1) NOT NULL,
                                [UserID] INT NOT NULL,
                                [CardNumber] NVARCHAR(19) NOT NULL,
                                [MaskedCardNumber] NVARCHAR(255) NOT NULL,
                                [ExpiryDate] NVARCHAR(5) NOT NULL,
                                [CVV] NVARCHAR(3) NOT NULL,
                                [CardHolderName] NVARCHAR(100) NULL,
                                [CardType] NVARCHAR(20) NULL,
                                [IsDefault] BIT DEFAULT 1,
                                [CreatedAt] DATETIME DEFAULT GETDATE(),
                                [UpdatedAt] DATETIME NULL,
                                CONSTRAINT [PK__PaymentMethods__3214EC274F9B1DFE] PRIMARY KEY CLUSTERED ([ID] ASC),
                                CONSTRAINT [UK_PaymentMethods_UserID] UNIQUE ([UserID]),
                                CONSTRAINT [FK__PaymentMethods__UserID__123456] FOREIGN KEY ([UserID]) REFERENCES [dbo].[Users] ([ID])
                            )";
                        
                        _context.Database.OpenConnection();
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but continue
                Console.WriteLine($"Error ensuring PaymentMethod table exists: {ex.Message}");
            }
            finally
            {
                _context.Database.CloseConnection();
            }
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
        public IActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // استخراج بيانات المستخدم من النموذج
                    string email = model.Email?.Trim();
                    string password = model.Password?.Trim();

                    // البحث عن المستخدم بواسطة البريد الإلكتروني
                    var user = _context.Users.FirstOrDefault(u => u.Email == email);

                    if (user == null)
                    {
                        ModelState.AddModelError("Email", "البريد الإلكتروني غير مسجل");
                        return View(model);
                    }

                    // التحقق من حالة المستخدم - معالجة القيمة القابلة للإلغاء
                    if (user.IsActive == false)
                    {
                        ModelState.AddModelError("", "تم حظر حسابك. يرجى التواصل مع الإدارة للمزيد من المعلومات.");
                        return View(model);
                    }

                    // التحقق من كلمة المرور
                    if (user.Password != password)
                    {
                        ModelState.AddModelError("Password", "كلمة المرور غير صحيحة");
                        return View(model);
                    }

                    // تم التحقق بنجاح، قم بتخزين معلومات المستخدم في الكوكيز
                    Response.Cookies.Append("UserId", user.Id.ToString(), new CookieOptions { Expires = DateTime.Now.AddDays(30) });
                    Response.Cookies.Append("UserName", user.Name ?? "User", new CookieOptions { Expires = DateTime.Now.AddDays(30) });
                    Response.Cookies.Append("UserEmail", user.Email ?? "", new CookieOptions { Expires = DateTime.Now.AddDays(30) });
                    Response.Cookies.Append("UserRole", user.Role ?? "User", new CookieOptions { Expires = DateTime.Now.AddDays(30) });
                    Response.Cookies.Append("AccountType", "Personal Account", new CookieOptions { Expires = DateTime.Now.AddDays(30) });
                    
                    // التحقق من وجود تاريخ الإنشاء
                    string joinDate = DateTime.Now.ToString("d");
                    if (user.CreatedAt.HasValue)
                    {
                        joinDate = user.CreatedAt.Value.ToString("d");
                    }
                    Response.Cookies.Append("JoinDate", joinDate, new CookieOptions { Expires = DateTime.Now.AddDays(30) });

                    // إذا كان المستخدم مسؤولاً، قم بتوجيهه إلى لوحة التحكم
                    if (user.Role == "Admin")
                    {
                        return RedirectToAction("Dashboard", "Admin");
                    }

                    // توجيه المستخدم العادي إلى الصفحة الرئيسية
                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Email or Password is invalid !: {ex.Message}");
                    return View(model);
                }
            }

            return View(model);
        }

        private List<CartItemViewModel> GetCartFromSession()
        {
            var cart = HttpContext.Session.GetString("Cart");
            if (string.IsNullOrEmpty(cart))
            {
                return new List<CartItemViewModel>();
            }
            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<List<CartItemViewModel>>(cart);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deserializing cart: {ex.Message}");
                return new List<CartItemViewModel>();
            }
        }

        private void MergeSessionCartToDatabase(int userId, List<CartItemViewModel> sessionCart)
        {
            try
            {
                if (sessionCart == null || !sessionCart.Any())
                {
                    return; // No session items to merge
                }
                
                // Check if user already has a cart
                var userCart = _context.Carts.FirstOrDefault(c => c.UserId == userId);
                
                // If no cart exists, create one
                if (userCart == null)
                {
                    userCart = new Cart
                    {
                        UserId = userId,
                        CreatedAt = DateTime.Now
                    };
                    
                    _context.Carts.Add(userCart);
                    _context.SaveChanges(); // Save to get the cart ID
                }
                
                // Add session items to database cart
                foreach (var item in sessionCart)
                {
                    // Check if product already in cart
                    var existingItem = _context.CartItems
                        .FirstOrDefault(ci => ci.CartId == userCart.Id && ci.ProductId == item.ProductId);
                        
                    if (existingItem != null)
                    {
                        // Update quantity if item already exists
                        existingItem.Quantity += item.Quantity;
                        existingItem.UpdatedAt = DateTime.Now;
                    }
                    else
                    {
                        // Add new item
                        var cartItem = new CartItem
                        {
                            CartId = userCart.Id,
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            CreatedAt = DateTime.Now
                        };
                        
                        _context.CartItems.Add(cartItem);
                    }
                }
                
                _context.SaveChanges();
                
                // Clear session cart after merging
                HttpContext.Session.Remove("Cart");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error merging session cart to database: {ex.Message}");
                // Log the error
            }
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

            int userId = int.Parse(Request.Cookies["UserId"]);
            
            // Get the user's payment method if it exists
            var paymentMethod = _context.PaymentMethods
                .FirstOrDefault(p => p.UserId == userId);
            
            // Pass the payment method to the view
            return View(paymentMethod);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPaymentMethod(PaymentMethodViewModel model)
        {
            // Check if user is logged in
            if (Request.Cookies["UserId"] == null)
            {
                return RedirectToAction("Login");
            }

            int userId = int.Parse(Request.Cookies["UserId"]);
            
            if (ModelState.IsValid)
            {
                try
                {
                    // Check if the user already has a payment method
                    var existingPaymentMethod = await _context.PaymentMethods
                        .FirstOrDefaultAsync(p => p.UserId == userId);
                    
                    if (existingPaymentMethod != null)
                    {
                        // Update existing payment method
                        existingPaymentMethod.CardNumber = model.CardNumber;
                        existingPaymentMethod.MaskedCardNumber = MaskCardNumber(model.CardNumber);
                        existingPaymentMethod.ExpiryDate = model.ExpiryDate;
                        existingPaymentMethod.CVV = model.CVV;
                        existingPaymentMethod.CardHolderName = model.CardHolderName;
                        existingPaymentMethod.CardType = DetectCardType(model.CardNumber);
                        existingPaymentMethod.UpdatedAt = DateTime.Now;
                        
                        _context.Update(existingPaymentMethod);
                        TempData["SuccessMessage"] = "Payment method updated successfully.";
                    }
                    else
                    {
                        // Create new payment method
                        var newPaymentMethod = new PaymentMethod
                        {
                            UserId = userId,
                            CardNumber = model.CardNumber,
                            MaskedCardNumber = MaskCardNumber(model.CardNumber),
                            ExpiryDate = model.ExpiryDate,
                            CVV = model.CVV,
                            CardHolderName = model.CardHolderName,
                            CardType = DetectCardType(model.CardNumber),
                            IsDefault = true,
                            CreatedAt = DateTime.Now
                        };
                        
                        _context.PaymentMethods.Add(newPaymentMethod);
                        TempData["SuccessMessage"] = "Payment method added successfully.";
                    }
                    
                    await _context.SaveChangesAsync();
                    return RedirectToAction("PaymentMethod");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error: {ex.Message}");
                }
            }
            
            // If we reach here, there was an error, return to view with model
            return RedirectToAction("PaymentMethod");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePaymentMethod()
        {
            // Check if user is logged in
            if (Request.Cookies["UserId"] == null)
            {
                return RedirectToAction("Login");
            }

            int userId = int.Parse(Request.Cookies["UserId"]);
            
            try
            {
                // Find the user's payment method
                var paymentMethod = await _context.PaymentMethods
                    .FirstOrDefaultAsync(p => p.UserId == userId);
                
                if (paymentMethod != null)
                {
                    _context.PaymentMethods.Remove(paymentMethod);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Payment method deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "No payment method found to delete.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
            }
            
            return RedirectToAction("PaymentMethod");
        }

        // Helper method to mask the card number
        private string MaskCardNumber(string cardNumber)
        {
            // Keep only the last 4 digits visible
            if (cardNumber.Length >= 4)
            {
                return "xxxx xxxx xxxx " + cardNumber.Substring(cardNumber.Length - 4);
            }
            
            return cardNumber;
        }

        // Helper method to detect card type based on first digit(s)
        private string DetectCardType(string cardNumber)
        {
            if (string.IsNullOrEmpty(cardNumber) || cardNumber.Length < 1)
            {
                return "Unknown";
            }
            
            // Simple detection based on first digits
            switch (cardNumber[0])
            {
                case '4':
                    return "Visa";
                case '5':
                    return "MasterCard";
                case '3':
                    if (cardNumber.Length > 1 && (cardNumber[1] == '4' || cardNumber[1] == '7'))
                    {
                        return "AmericanExpress";
                    }
                    return "Unknown";
                case '6':
                    return "Discover";
                default:
                    return "Unknown";
            }
        }

        public async Task<ActionResult> MyOrders()
        {
            // التحقق من تسجيل دخول المستخدم
            if (Request.Cookies["UserId"] == null)
            {
                return RedirectToAction("Login");
            }

            try
            {
                // الحصول على معرف المستخدم من الكوكيز
                int userId = int.Parse(Request.Cookies["UserId"]);

                // الحصول على طلبات المستخدم مع تضمين بيانات الدفع
                var orders = await _context.Orders
                    .Include(o => o.Payments)  // تضمين بيانات الدفع
                    .Where(o => o.UserId == userId)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();

                // إذا كان هناك رسالة نجاح من صفحة تأكيد الطلب، حافظ عليها
                if (TempData["SuccessMessage"] != null)
                {
                    TempData["SuccessMessage"] = TempData["SuccessMessage"].ToString();
                }

                return View(orders);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading orders: {ex.Message}";
                return View(new List<Order>());
            }
        }

        public async Task<ActionResult> OrderDetails(int id)
        {
            // التحقق من تسجيل دخول المستخدم
            if (Request.Cookies["UserId"] == null)
            {
                return RedirectToAction("Login");
            }

            try
            {
                // الحصول على معرف المستخدم من الكوكيز
                int userId = int.Parse(Request.Cookies["UserId"]);

                // الحصول على تفاصيل الطلب مع تضمين بيانات الدفع
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                    .Include(o => o.Payments)  // تضمين بيانات الدفع
                    .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

                if (order == null)
                {
                    TempData["ErrorMessage"] = "Order not found.";
                    return RedirectToAction("MyOrders");
                }

                return View(order);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading order details: {ex.Message}";
                return RedirectToAction("MyOrders");
            }
        }
        
        // API endpoint to get order details for AJAX calls
        public async Task<JsonResult> GetOrderDetails(int id)
        {
            try
            {
                // التحقق من وجود المستخدم المسجل
                if (Request.Cookies["UserId"] == null && !IsAdmin())
                {
                    return Json(new { success = false, message = "Unauthorized" });
                }

                // Get the order with related items and products
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null)
                {
                    return Json(new { success = false, message = "Order not found" });
                }

                // التحقق من أن المستخدم مسؤول أو مالك الطلب
                bool isAdmin = IsAdmin();
                bool isOrderOwner = false;
                
                if (Request.Cookies["UserId"] != null)
                {
                    int userId = int.Parse(Request.Cookies["UserId"]);
                    isOrderOwner = order.UserId == userId;
                }

                if (!isAdmin && !isOrderOwner)
                {
                    return Json(new { success = false, message = "Unauthorized" });
                }

                // Map to simplified object to avoid circular references in JSON
                var result = new
                {
                    id = order.Id,
                    orderNumber = order.OrderNumber,
                    totalAmount = order.TotalAmount,
                    status = order.Status,
                    shippingAddress = order.ShippingAddress,
                    billingAddress = order.BillingAddress,
                    paymentMethod = order.PaymentMethod,
                    paymentStatus = order.PaymentStatus,
                    createdAt = order.CreatedAt,
                    orderItems = order.OrderItems.Select(item => new
                    {
                        id = item.Id,
                        quantity = item.Quantity,
                        unitPrice = item.UnitPrice,
                        totalPrice = item.TotalPrice,
                        product = item.Product == null ? null : new
                        {
                            id = item.Product.Id,
                            productName = item.Product.ProductName,
                            imageUrl = item.Product.ImageUrl
                        }
                    }).ToList()
                };

                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
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