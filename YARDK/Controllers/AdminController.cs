using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using YARDK.Models;
using Microsoft.Extensions.Logging;

namespace YARDK.Controllers
{
    public class AdminController : Controller
    {
        private readonly MyDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<AdminController> _logger;

        public AdminController(MyDbContext context, IWebHostEnvironment webHostEnvironment, ILogger<AdminController> logger)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
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

            // جمع الإحصائيات للوحة التحكم
            try
            {
                // إحصائيات المستخدمين
                ViewBag.TotalUsers = _context.Users.Count();
                ViewBag.NewUsers = _context.Users.Count(u => u.CreatedAt >= DateTime.Now.AddMonths(-1));

                // إحصائيات المنتجات
                ViewBag.TotalProducts = _context.Products.Count();
                ViewBag.NewProducts = _context.Products.Count(p => p.CreatedAt >= DateTime.Now.AddDays(-7));

                // إحصائيات الطلبات
                ViewBag.TotalOrders = _context.Orders.Count();
                ViewBag.PendingOrders = _context.Orders.Count(o => o.Status == "Pending");

                // إحصائيات الإيرادات
                ViewBag.TotalRevenue = Math.Round(_context.Orders.Sum(o => o.TotalAmount), 2);
                ViewBag.MonthlyRevenue = Math.Round(_context.Orders
                    .Where(o => o.CreatedAt >= DateTime.Now.AddMonths(-1))
                    .Sum(o => o.TotalAmount), 2);
                    
                // إحصائيات الفئات
                ViewBag.TotalCategories = _context.Categories.Count();
                
                // إحصائيات التعليقات
                ViewBag.TotalFeedbacks = _context.Feedbacks.Count();
                ViewBag.NewFeedbacks = _context.Feedbacks.Count(f => f.CreatedAt >= DateTime.Now.AddDays(-7));
                
                // بيانات إحصائيات المبيعات للرسم البياني
                var lastSixMonths = Enumerable.Range(0, 6)
                    .Select(i => DateTime.Now.AddMonths(-i))
                    .Select(date => new
                    {
                        Month = date.ToString("MMM"),
                        Year = date.Year,
                        MonthNumber = date.Month,
                        YearNumber = date.Year
                    })
                    .ToList();
                    
                var monthlySales = new List<object>();
                
                foreach (var month in lastSixMonths)
                {
                    var salesAmount = _context.Orders
                        .Where(o => o.CreatedAt.HasValue && 
                               o.CreatedAt.Value.Month == month.MonthNumber && 
                               o.CreatedAt.Value.Year == month.YearNumber)
                        .Sum(o => o.TotalAmount);
                        
                    monthlySales.Add(new
                    {
                        Month = month.Month,
                        Year = month.Year,
                        Sales = Math.Round(salesAmount, 2)
                    });
                }
                
                ViewBag.MonthlySales = monthlySales.OrderBy(m => ((dynamic)m).Year).ThenBy(m => ((dynamic)m).Month).ToList();
                
                // إحصائيات أعداد الطلبات خلال آخر أسبوع
                var lastWeekDays = Enumerable.Range(0, 7)
                    .Select(i => DateTime.Now.Date.AddDays(-i))
                    .ToList();
                    
                var weeklyOrdersData = new List<object>();
                
                foreach (var day in lastWeekDays)
                {
                    var ordersCount = _context.Orders
                        .Count(o => o.CreatedAt.HasValue && o.CreatedAt.Value.Date == day);
                        
                    weeklyOrdersData.Add(new
                    {
                        Date = day.ToString("dd MMM"),
                        Day = day.ToString("ddd"),
                        Count = ordersCount
                    });
                }
                
                ViewBag.WeeklyOrdersData = weeklyOrdersData.OrderBy(d => ((dynamic)d).Date).ToList();
                
                // آخر النشاطات - الطلبات والتسجيلات الجديدة
                var recentOrders = _context.Orders
                    .OrderByDescending(o => o.CreatedAt)
                    .Take(3)
                    .Select(o => new
                    {
                        OrderId = o.Id,
                        TotalAmount = o.TotalAmount,
                        Status = o.Status,
                        CreatedAt = o.CreatedAt,
                        UserName = _context.Users.Where(u => u.Id == o.UserId).Select(u => u.Name).FirstOrDefault()
                    })
                    .ToList();
                    
                var recentUsers = _context.Users
                    .OrderByDescending(u => u.CreatedAt)
                    .Take(2)
                    .Select(u => new
                    {
                        UserId = u.Id,
                        Name = u.Name,
                        CreatedAt = u.CreatedAt
                    })
                    .ToList();
                    
                ViewBag.RecentOrders = recentOrders;
                ViewBag.RecentUsers = recentUsers;
            }
            catch (Exception ex)
            {
                // في حالة وجود مشكلة في استخراج البيانات، عرض قيم افتراضية
                ViewBag.TotalUsers = 0;
                ViewBag.NewUsers = 0;
                ViewBag.TotalProducts = 0;
                ViewBag.NewProducts = 0;
                ViewBag.TotalOrders = 0;
                ViewBag.PendingOrders = 0;
                ViewBag.TotalRevenue = 0;
                ViewBag.MonthlyRevenue = 0;
                ViewBag.TotalCategories = 0;
                ViewBag.TotalFeedbacks = 0;
                ViewBag.NewFeedbacks = 0;
                ViewBag.MonthlySales = new List<object>();
                ViewBag.WeeklyOrdersData = new List<object>();
                ViewBag.RecentOrders = new List<object>();
                ViewBag.RecentUsers = new List<object>();
                
                // يمكن تسجيل الخطأ هنا
                Console.WriteLine($"Error loading dashboard stats: {ex.Message}");
            }

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

        // GET: Admin/Users
        public async Task<IActionResult> Users()
        {
            // التحقق من صلاحيات المستخدم
            var accessCheck = CheckAdminAccess();
            if (accessCheck != null)
                return accessCheck;

            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        // POST: Admin/ToggleUserStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserStatus(int id)
        {
            // التحقق من صلاحيات المستخدم
            var accessCheck = CheckAdminAccess();
            if (accessCheck != null)
                return accessCheck;

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return RedirectToAction(nameof(Users));
            }

            // تغيير حالة المستخدم من نشط إلى معطل أو العكس
            user.IsActive = !user.IsActive;
            
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Users));
        }

        // POST: Admin/AssignAdmin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignAdmin(int id)
        {
            // التحقق من صلاحيات المستخدم
            var accessCheck = CheckAdminAccess();
            if (accessCheck != null)
                return accessCheck;

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return RedirectToAction(nameof(Users));
            }

            // تغيير دور المستخدم إلى مسؤول
            user.Role = "Admin";
            
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Users));
        }

        // GET: Admin/Admins
        public async Task<IActionResult> Admins()
        {
            // التحقق من صلاحيات المستخدم
            var accessCheck = CheckAdminAccess();
            if (accessCheck != null)
                return accessCheck;

            var admins = await _context.Users.Where(u => u.Role == "Admin").ToListAsync();
            return View(admins);
        }

        // POST: Admin/RemoveAdmin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveAdmin(int id)
        {
            // التحقق من صلاحيات المستخدم
            var accessCheck = CheckAdminAccess();
            if (accessCheck != null)
                return accessCheck;

            var admin = await _context.Users.FindAsync(id);
            if (admin == null)
            {
                return RedirectToAction(nameof(Admins));
            }

            // التحقق من أن المستخدم الحالي ليس نفسه الذي يتم حذفه
            if (admin.Id.ToString() == Request.Cookies["UserId"])
            {
                TempData["Error"] = Request.Cookies["Lang"] == "ar" 
                    ? "لا يمكنك إزالة نفسك من قائمة المسؤولين" 
                    : "You cannot remove yourself from admin list";
                return RedirectToAction(nameof(Admins));
            }

            // تغيير دور المسؤول إلى مستخدم عادي
            admin.Role = "User";
            
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Admins));
        }

        // GET: Admin/UserDetails/5
        public async Task<IActionResult> UserDetails(int? id)
        {
            // التحقق من صلاحيات المستخدم
            var accessCheck = CheckAdminAccess();
            if (accessCheck != null)
                return accessCheck;

            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // الحصول على منتجات المستخدم
            var userProducts = await _context.Products
                .Where(p => p.SellerId == id)
                .ToListAsync();

            // الحصول على إعلانات المستخدم المميزة
            var userFeaturedAds = await _context.FeaturedAds
                .Include(f => f.Product)
                .Where(f => f.SellerId == id)
                .ToListAsync();

            // الحصول على طلبات المستخدم
            var userOrders = await _context.Orders
                .Where(o => o.UserId == id)
                .ToListAsync();

            // إرسال البيانات إلى العرض
            ViewBag.UserProducts = userProducts;
            ViewBag.UserFeaturedAds = userFeaturedAds;
            ViewBag.UserOrders = userOrders;

            return View(user);
        }

        // GET: Admin/BlockedUsers
        public async Task<IActionResult> BlockedUsers()
        {
            // التحقق من صلاحيات المستخدم
            var accessCheck = CheckAdminAccess();
            if (accessCheck != null)
                return accessCheck;

            // الحصول على المستخدمين المحظورين فقط (حالة IsActive = false)
            var blockedUsers = await _context.Users
                .Where(u => u.IsActive == false)
                .ToListAsync();

            return View(blockedUsers);
        }

        // Orders Management
        public async Task<IActionResult> Orders(string status = "all")
        {
            try
            {
                var query = _context.Orders
                    .Include(o => o.User)
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                    .Include(o => o.Payments)
                    .AsQueryable();

                // Filter by status if not "all"
                if (status != "all")
                {
                    query = query.Where(o => o.Status == status);
                }

                // Get orders and map to view model
                var orders = await query
                    .OrderByDescending(o => o.CreatedAt)
                    .Select(o => new OrderViewModel
                    {
                        Id = o.Id,
                        OrderNumber = o.OrderNumber,
                        CustomerName = o.User.Name,
                        CustomerEmail = o.User.Email,
                        CustomerPhone = o.User.Phone,
                        TotalAmount = o.TotalAmount,
                        Status = o.Status,
                        ShippingAddress = o.ShippingAddress,
                        BillingAddress = o.BillingAddress,
                        PaymentMethod = o.PaymentMethod,
                        PaymentStatus = o.PaymentStatus,
                        CreatedAt = o.CreatedAt ?? DateTime.Now,
                        UpdatedAt = o.UpdatedAt,
                        OrderItems = o.OrderItems.Select(oi => new OrderItemViewModel
                        {
                            Id = oi.Id,
                            ProductId = oi.ProductId ?? 0,
                            ProductName = oi.Product.ProductName,
                            ProductImage = oi.Product.ImageUrl,
                            Quantity = oi.Quantity,
                            UnitPrice = oi.UnitPrice,
                            TotalPrice = oi.TotalPrice
                        }).ToList(),
                        Payments = o.Payments.Select(p => new PaymentViewModel
                        {
                            Id = p.Id,
                            Amount = p.Amount,
                            PaymentMethod = p.PaymentMethod,
                            PaymentStatus = p.PaymentStatus,
                            TransactionId = p.TransactionId,
                            PaymentDate = p.PaymentDate ?? DateTime.Now,
                            LastUpdated = p.LastUpdated
                        }).ToList()
                    })
                    .ToListAsync();

                ViewBag.CurrentStatus = status;
                return View(orders);
            }
            catch (Exception ex)
            {
                // Log the error
                _logger.LogError(ex, "Error retrieving orders");
                TempData["ErrorMessage"] = "An error occurred while retrieving orders.";
                return View(new List<OrderViewModel>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetOrderDetails(int id)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.User)
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                    .Include(o => o.Payments)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null)
                {
                    return Json(new { success = false, message = "Order not found" });
                }

                var orderViewModel = new OrderViewModel
                {
                    Id = order.Id,
                    OrderNumber = order.OrderNumber,
                    CustomerName = order.User.Name,
                    CustomerEmail = order.User.Email,
                    CustomerPhone = order.User.Phone,
                    TotalAmount = order.TotalAmount,
                    Status = order.Status,
                    ShippingAddress = order.ShippingAddress,
                    BillingAddress = order.BillingAddress,
                    PaymentMethod = order.PaymentMethod,
                    PaymentStatus = order.PaymentStatus,
                    CreatedAt = order.CreatedAt ?? DateTime.Now,
                    UpdatedAt = order.UpdatedAt,
                    OrderItems = order.OrderItems.Select(oi => new OrderItemViewModel
                    {
                        Id = oi.Id,
                        ProductId = oi.ProductId ?? 0,
                        ProductName = oi.Product.ProductName,
                        ProductImage = oi.Product.ImageUrl,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        TotalPrice = oi.TotalPrice
                    }).ToList(),
                    Payments = order.Payments.Select(p => new PaymentViewModel
                    {
                        Id = p.Id,
                        Amount = p.Amount,
                        PaymentMethod = p.PaymentMethod,
                        PaymentStatus = p.PaymentStatus,
                        TransactionId = p.TransactionId,
                        PaymentDate = p.PaymentDate ?? DateTime.Now,
                        LastUpdated = p.LastUpdated
                    }).ToList()
                };

                return Json(new { success = true, data = orderViewModel });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order details for order {OrderId}", id);
                return Json(new { success = false, message = "An error occurred while retrieving order details" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, string status)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.Payments)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    return Json(new { success = false, message = "Order not found" });
                }

                // Check if payment is completed
                var isPaymentCompleted = order.Payments.Any(p => p.PaymentStatus == "Paid");
                if (!isPaymentCompleted)
                {
                    return Json(new { 
                        success = false, 
                        message = "Cannot update order status. Payment has not been completed." 
                    });
                }

                // Update order status
                order.Status = status;
                order.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status for order {OrderId}", orderId);
                return Json(new { 
                    success = false, 
                    message = "An error occurred while updating the order status" 
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePaymentStatus(int paymentId, string status)
        {
            try
            {
                var payment = await _context.Payments
                    .Include(p => p.Order)
                    .FirstOrDefaultAsync(p => p.Id == paymentId);

                if (payment == null)
                {
                    return Json(new { 
                        success = false, 
                        message = "Payment not found",
                        messageAr = "لم يتم العثور على الدفعة"
                    });
                }

                // Update payment status
                payment.PaymentStatus = status;
                payment.LastUpdated = DateTime.Now;

                // If payment is marked as paid, update order payment status
                if (status == "Paid")
                {
                    payment.Order.PaymentStatus = "Paid";
                    payment.Order.UpdatedAt = DateTime.Now;
                }

                await _context.SaveChangesAsync();

                return Json(new { 
                    success = true,
                    message = "Payment status updated successfully",
                    messageAr = "تم تحديث حالة الدفع بنجاح"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment status for payment {PaymentId}", paymentId);
                return Json(new { 
                    success = false, 
                    message = "An error occurred while updating the payment status",
                    messageAr = "حدث خطأ أثناء تحديث حالة الدفع"
                });
            }
        }

        [HttpGet]
        public IActionResult Feedbacks()
        {
            var feedbacks = _context.Feedbacks.ToList();
            return View(feedbacks);
        }

        [HttpGet]
        public IActionResult GetFeedback(int id)
        {
            try
            {
                var feedback = _context.Feedbacks.Find(id);
                if (feedback == null)
                {
                    return Json(new { success = false, message = "Feedback not found" });
                }

                return Json(new { 
                    success = true, 
                    data = new { 
                        name = feedback.Name,
                        email = feedback.Email,
                        subject = feedback.Subject,
                        message = feedback.Message,
                        createdAt = feedback.CreatedAt
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving feedback");
                return Json(new { success = false, message = "An error occurred while retrieving the feedback" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteFeedback(int id)
        {
            try
            {
                var feedback = _context.Feedbacks.Find(id);
                if (feedback == null)
                {
                    return Json(new { success = false, message = "Feedback not found" });
                }

                _context.Feedbacks.Remove(feedback);
                _context.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting feedback");
                return Json(new { success = false, message = "An error occurred while deleting the feedback" });
            }
        }
    }
} 