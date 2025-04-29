using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YARDK.Models;
using System.IO;

namespace YARDK.Controllers
{
    public class ProductController : Controller
    {
        private readonly MyDbContext _context;

        public ProductController(MyDbContext context)
        {
            _context = context;
        }

        // GET: ProductController
        public async Task<ActionResult> Categories()
        {
            List<Category> categories;
            
            try
            {
                // الحصول على جميع الفئات من قاعدة البيانات
                categories = await _context.Categories.ToListAsync();
                
                // إذا لم توجد فئات في قاعدة البيانات، قم بإنشاء فئات افتراضية وإضافتها للقاعدة
                if (categories == null || !categories.Any())
                {
                    // إضافة الفئات الافتراضية إلى قاعدة البيانات
                    categories = GetDefaultCategories();
                    _context.Categories.AddRange(categories);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                // في حالة حدوث خطأ، استخدام فئات افتراضية
                categories = GetDefaultCategories();
            }
            
            return View(categories);
        }

        // دالة مساعدة للحصول على فئات افتراضية
        private List<Category> GetDefaultCategories()
        {
            return new List<Category>
            {
                new Category
                {
                    CategoryName = "Electrical Appliances",
                    Description = "Browse through surplus and used electrical appliances, perfect for resellers, companies, and individuals looking for cost-effective solutions.",
                    ImageUrl = "/img/Electrical Appliances/electricalApplinces01.jpg"
                },
                new Category
                {
                    CategoryName = "Furniture",
                    Description = "Discover quality surplus and second-hand furniture for your home or office at affordable prices.",
                    ImageUrl = "/img/Furnituer/Furnituer01.jpg"
                },
                new Category
                {
                    CategoryName = "Scrap & Surplus",
                    Description = "Find valuable scrap materials and surplus items that can be repurposed or recycled.",
                    ImageUrl = "/img/scrap/scrapMetal.png"
                }
            };
        }

        public async Task<ActionResult> Electrical_Appliances(int pageNumber = 1)
        {
            int pageSize = 9; // عدد المنتجات في كل صفحة
            try
            {
                // Query base
                var query = _context.Products
                    .Where(p => p.Category == "Electrical Appliances" && p.Status == "Active" && p.Quantity > 0)
                    .Include(p => p.Seller); // Include Seller if needed for SellerName

                // Get total count for pagination
                int totalItems = await query.CountAsync();
                int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                // Get products for the current page
                var products = await query
                    .OrderByDescending(p => p.CreatedAt) // Order by creation date or another relevant field
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => new ProductViewModel
                    {
                        Id = p.Id,
                        ProductName = p.ProductName,
                        Description = p.Description,
                        Category = p.Category,
                        Quantity = p.Quantity ?? 0,
                        Price = p.Price ?? 0,
                        ImageUrl = p.ImageUrl, // Or handle multiple images if needed
                        SellerName = p.Seller.Name // Assuming Seller is included
                    })
                    .ToListAsync();
                
                // Pass pagination data to the view
                ViewBag.PageNumber = pageNumber;
                ViewBag.TotalPages = totalPages;
                ViewBag.HasPreviousPage = pageNumber > 1;
                ViewBag.HasNextPage = pageNumber < totalPages;
                ViewBag.CategoryName = "Electrical_Appliances"; // Used for generating links

                return View(products);
            }
            catch (Exception ex)
            {
                 // Log the exception (implementation depends on your logging setup)
                ViewBag.ErrorMessage = "An error occurred while fetching products.";
                return View(new List<ProductViewModel>());
            }
        }

        public async Task<ActionResult> Furniture(int pageNumber = 1)
        {
             int pageSize = 9; // عدد المنتجات في كل صفحة
            try
            {
                var query = _context.Products
                    .Where(p => p.Category == "Furniture" && p.Status == "Active" && p.Quantity > 0)
                    .Include(p => p.Seller); 

                int totalItems = await query.CountAsync();
                int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                var products = await query
                    .OrderByDescending(p => p.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => new ProductViewModel
                    {
                        Id = p.Id,
                        ProductName = p.ProductName,
                        Description = p.Description,
                        Category = p.Category,
                        Quantity = p.Quantity ?? 0,
                        Price = p.Price ?? 0,
                        ImageUrl = p.ImageUrl,
                        SellerName = p.Seller.Name
                    })
                    .ToListAsync();

                ViewBag.PageNumber = pageNumber;
                ViewBag.TotalPages = totalPages;
                ViewBag.HasPreviousPage = pageNumber > 1;
                ViewBag.HasNextPage = pageNumber < totalPages;
                ViewBag.CategoryName = "Furniture"; // Used for generating links

                return View(products);
            }
            catch (Exception ex)
            {
                 ViewBag.ErrorMessage = "An error occurred while fetching products.";
                return View(new List<ProductViewModel>());
            }
        }

        public async Task<ActionResult> Scrap_Surplus(int pageNumber = 1)
        {
             int pageSize = 9; // عدد المنتجات في كل صفحة
            try
            {
                var query = _context.Products
                    .Where(p => p.Category == "Scrap & Surplus" && p.Status == "Active" && p.Quantity > 0)
                    .Include(p => p.Seller); 

                int totalItems = await query.CountAsync();
                int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                 var products = await query
                    .OrderByDescending(p => p.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => new ProductViewModel
                    {
                        Id = p.Id,
                        ProductName = p.ProductName,
                        Description = p.Description,
                        Category = p.Category,
                        Quantity = p.Quantity ?? 0,
                        Price = p.Price ?? 0,
                        ImageUrl = p.ImageUrl,
                        SellerName = p.Seller.Name
                    })
                    .ToListAsync();

                ViewBag.PageNumber = pageNumber;
                ViewBag.TotalPages = totalPages;
                ViewBag.HasPreviousPage = pageNumber > 1;
                ViewBag.HasNextPage = pageNumber < totalPages;
                ViewBag.CategoryName = "Scrap_Surplus"; // Used for generating links

                return View(products);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An error occurred while fetching products.";
                return View(new List<ProductViewModel>());
            }
        }

        public ActionResult ProductDetails(int id)
        {
            var product = _context.Products
                .Include(p => p.Seller)
                .Include(p => p.ProductImages)
                .FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        public ActionResult Cart()
        {
            // الحصول على عناصر السلة من الجلسة
            List<CartItemViewModel> cartItems = GetCartFromSession();
            return View(cartItems);
        }

        // GET: ProductController/AddToCart
        public IActionResult AddToCart(int id)
        {
            // التحقق من وجود المنتج
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
        {
                return NotFound();
            }

            // الحصول على السلة الحالية من الجلسة أو إنشاء سلة جديدة
            List<CartItemViewModel> cart = GetCartFromSession();

            // التحقق مما إذا كان المنتج موجودًا بالفعل في السلة
            var existingItem = cart.FirstOrDefault(item => item.ProductId == id);
            if (existingItem != null)
            {
                // زيادة الكمية إذا كان المنتج موجودًا بالفعل في السلة
                existingItem.Quantity++;
            }
            else
            {
                // إضافة عنصر جديد إلى السلة
                cart.Add(new CartItemViewModel
                {
                    ProductId = product.Id,
                    ProductName = product.ProductName,
                    Price = product.Price ?? 0,
                    Quantity = 1,
                    ImageUrl = product.ImageUrl
                });
            }

            // حفظ السلة المحدثة في الجلسة
            SaveCartToSession(cart);

            // إضافة رسالة نجاح باللغة الإنجليزية
            TempData["SuccessMessage"] = "Product added to your cart successfully!";

            // التحقق مما إذا كان الطلب AJAX
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                // إذا كان طلب AJAX، أعد رد JSON
                return Json(new { success = true, message = "Product added to your cart successfully!" });
            }

            // إذا لم يكن طلب AJAX، قم بإعادة التوجيه إلى صفحة السلة
            return RedirectToAction("Cart");
        }

        private List<CartItemViewModel> GetCartFromSession()
        {
            var cart = HttpContext.Session.GetString("Cart");
            if (string.IsNullOrEmpty(cart))
            {
                return new List<CartItemViewModel>();
            }
            return System.Text.Json.JsonSerializer.Deserialize<List<CartItemViewModel>>(cart);
        }

        private void SaveCartToSession(List<CartItemViewModel> cart)
        {
            var cartJson = System.Text.Json.JsonSerializer.Serialize(cart);
            HttpContext.Session.SetString("Cart", cartJson);
        }

        // GET: ProductController/RemoveFromCart
        public IActionResult RemoveFromCart(int id)
        {
            // الحصول على السلة الحالية
            List<CartItemViewModel> cart = GetCartFromSession();
            
            // إزالة العنصر ذو معرف المنتج المطابق
            cart.RemoveAll(item => item.ProductId == id);
            
            // حفظ السلة في الجلسة
            SaveCartToSession(cart);
            
            // العودة إلى سلة التسوق
            return RedirectToAction("Cart");
        }
        
        // POST: ProductController/UpdateCartQuantity
        [HttpPost]
        public IActionResult UpdateCartQuantity(int id, int quantity)
        {
            if (quantity < 1)
            {
                quantity = 1;
            }
            
            // التحقق من الكمية المتوفرة للمنتج في قاعدة البيانات
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
        }

            // تحديد الحد الأقصى للكمية بناءً على المخزون المتوفر
            int availableQuantity = product.Quantity ?? 0;
            if (quantity > availableQuantity)
            {
                quantity = availableQuantity;
                // يمكن إضافة رسالة تنبيه للمستخدم هنا
                TempData["QuantityWarning"] = $"The requested quantity exceeds our stock. Maximum available: {availableQuantity}";
            }
            
            // الحصول على السلة الحالية
            List<CartItemViewModel> cart = GetCartFromSession();
            
            // البحث عن العنصر وتحديث الكمية
            var item = cart.FirstOrDefault(item => item.ProductId == id);
            if (item != null)
            {
                item.Quantity = quantity;
            }
            
            // حفظ السلة في الجلسة
            SaveCartToSession(cart);
            
            return Ok();
        }

        public ActionResult Checkout()
        {
            // التحقق من تسجيل دخول المستخدم
            if (Request.Cookies["UserId"] == null)
            {
                return RedirectToAction("Login", "User");
            }

            // الحصول على عناصر السلة
            List<CartItemViewModel> cartItems = GetCartFromSession();
            
            if (cartItems == null || !cartItems.Any())
        {
                return RedirectToAction("Cart");
        }

            // حساب المبالغ
            decimal subtotal = cartItems.Sum(item => item.Price * item.Quantity);
            decimal shipping = Math.Round(subtotal * 0.05m, 2);
            decimal total = subtotal + shipping;

            // إنشاء نموذج البيانات
            var viewModel = new CheckoutViewModel
            {
                CartItems = cartItems,
                Subtotal = subtotal,
                Shipping = shipping,
                Total = total
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessOrder(CheckoutViewModel model)
        {
            // التحقق من تسجيل دخول المستخدم
            if (Request.Cookies["UserId"] == null)
            {
                return RedirectToAction("Login", "User");
            }

            int userId = int.Parse(Request.Cookies["UserId"]);

            // الحصول على عناصر السلة
            List<CartItemViewModel> cartItems = GetCartFromSession();
            
            if (cartItems == null || !cartItems.Any())
            {
                return RedirectToAction("Cart");
            }

            // حساب المبالغ
            decimal subtotal = cartItems.Sum(item => item.Price * item.Quantity);
            decimal shipping = Math.Round(subtotal * 0.05m, 2);
            decimal total = subtotal + shipping;

            // التأكد من أن طريقة الدفع متوافقة مع القيود الموجودة في قاعدة البيانات
            string paymentMethod = model.PaymentMethod;
            // قيمة صالحة مضمونة: Credit Card
            if (string.IsNullOrEmpty(paymentMethod) || paymentMethod != "Credit Card")
            {
                paymentMethod = "Credit Card";
            }

            // إنشاء الطلب
            var order = new Order
            {
                UserId = userId,
                OrderNumber = GenerateOrderNumber(),
                TotalAmount = total,
                Status = "Pending",
                ShippingAddress = $"{model.Address}, {model.City}, {model.PostalCode}",
                BillingAddress = $"{model.Address}, {model.City}, {model.PostalCode}",
                PaymentMethod = paymentMethod,
                PaymentStatus = "Pending",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // إضافة عناصر الطلب
            foreach (var item in cartItems)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Price,
                    TotalPrice = item.Price * item.Quantity,
                    CreatedAt = DateTime.Now
                };

                _context.OrderItems.Add(orderItem);
            }

            // إنشاء سجل الدفع
            var payment = new Payment
            {
                OrderId = order.Id,
                Amount = total,
                PaymentMethod = paymentMethod,
                PaymentStatus = "Completed",
                TransactionId = GenerateTransactionId(),
                PaymentDate = DateTime.Now,
                LastUpdated = DateTime.Now
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            // تفريغ السلة
            HttpContext.Session.Remove("Cart");

            // تحويل المستخدم إلى صفحة تأكيد الطلب
            return RedirectToAction("OrderConfirmation", new { orderId = order.Id });
            }

        private string GenerateOrderNumber()
            {
            return "ORD-" + DateTime.Now.ToString("yyyyMMddHHmmss");
            }

        private string GenerateTransactionId()
        {
            return "TRX-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
        }

        public IActionResult OrderConfirmation(int orderId)
        {
            // الحصول على بيانات الطلب
            var order = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefault(o => o.Id == orderId);

            if (order == null)
        {
                return NotFound();
            }

            return View(order);
        }

        // POST: ProductController/AddProduct
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct(Product product, IFormFileCollection ProductImages)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("MyAds", "User");
            }

            try
            {
                // التحقق من أن المستخدم مسجل دخول
                if (Request.Cookies["UserId"] == null)
                {
                    return RedirectToAction("Login", "User");
                }

                int userId = int.Parse(Request.Cookies["UserId"]);

                // التحقق من عدد إعلانات المستخدم الفعالة (الكمية > 0)
                var userActiveAdsCount = await _context.Products
                    .CountAsync(p => p.SellerId == userId && p.Quantity > 0);
                    
                if (userActiveAdsCount >= 2)
                {
                    // يمكن إضافة رسالة خطأ هنا
                    TempData["ErrorMessage"] = "لا يمكنك إضافة أكثر من إعلانين فعالين";
                    return RedirectToAction("MyAds", "User");
                }

                // التحقق من وجود صور
                if (ProductImages == null || ProductImages.Count == 0)
                {
                    TempData["ErrorMessage"] = "يجب إضافة صورة واحدة على الأقل";
                    return RedirectToAction("MyAds", "User");
                }

                // التحقق من عدد الصور (الحد الأقصى 5 صور)
                if (ProductImages.Count > 5)
                {
                    TempData["ErrorMessage"] = "الحد الأقصى للصور هو 5 صور";
                    return RedirectToAction("MyAds", "User");
                }

                // إعداد الإعلان الجديد
                product.SellerId = userId;
                product.Status = "Active";
                product.CreatedAt = DateTime.Now;

                // إضافة الإعلان إلى قاعدة البيانات
                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                // معالجة الصور
                int displayOrder = 0;
                foreach (var imageFile in ProductImages)
                {
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        // إنشاء اسم فريد للصورة
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                        
                        // تحديد مسار حفظ الصورة
                        string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "products");
                        
                        // التأكد من وجود المجلد
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
            }
                        
                        // مسار الصورة الكامل
                        string filePath = Path.Combine(uploadsFolder, fileName);
                        
                        // حفظ الصورة على القرص
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(fileStream);
                        }
                        
                        // إنشاء سجل الصورة في قاعدة البيانات
                        var productImage = new ProductImage
                        {
                            ProductId = product.Id,
                            ImageUrl = "/uploads/products/" + fileName,
                            IsMainImage = displayOrder == 0, // الصورة الأولى هي الصورة الرئيسية
                            DisplayOrder = displayOrder,
                            CreatedAt = DateTime.Now
                        };
                        
                        _context.ProductImages.Add(productImage);
                        
                        // إذا كانت هذه الصورة الرئيسية، قم بتحديث حقل ImageUrl في المنتج أيضاً
                        if (displayOrder == 0)
                        {
                            product.ImageUrl = "/uploads/products/" + fileName;
                            _context.Products.Update(product);
                        }
                        
                        displayOrder++;
                    }
                }
                
                await _context.SaveChangesAsync();

                return RedirectToAction("MyAds", "User");
            }
            catch (Exception ex)
            {
                // تسجيل الخطأ
                TempData["ErrorMessage"] = "حدث خطأ أثناء إضافة الإعلان: " + ex.Message;
                return RedirectToAction("MyAds", "User");
            }
        }

        [HttpGet]
        public IActionResult GetCartCount()
        {
            // الحصول على السلة الحالية
            List<CartItemViewModel> cart = GetCartFromSession();
            
            // إرجاع عدد العناصر في السلة
            return Json(new { count = cart.Count });
        }

        // GET: ProductController/ClearCart
        public IActionResult ClearCart()
        {
            // إزالة السلة من الجلسة
            HttpContext.Session.Remove("Cart");
            
            // إضافة رسالة تأكيد
            TempData["SuccessMessage"] = "Your cart has been cleared successfully.";
            
            // العودة إلى صفحة السلة
            return RedirectToAction("Cart");
        }

        // POST: ProductController/DeleteProduct/5
        [HttpPost]
        public IActionResult DeleteProduct(int id)
        {
            try
            {
                // التحقق من أن المستخدم قد سجل الدخول
                if (Request.Cookies["UserId"] == null)
                {
                    return RedirectToAction("Login", "User");
                }

                int userId = int.Parse(Request.Cookies["UserId"]);

                // تنفيذ استعلام SQL مباشر لوضع قيمة 0 في حقل Quantity
                string sql = "UPDATE Products SET Quantity = 0 WHERE ID = @p0 AND SellerID = @p1";
                _context.Database.ExecuteSqlRaw(sql, id, userId);

                TempData["SuccessMessage"] = "Product has been deleted successfully.";
                return RedirectToAction("MyAds", "User");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                return RedirectToAction("MyAds", "User");
            }
        }
    }
}
