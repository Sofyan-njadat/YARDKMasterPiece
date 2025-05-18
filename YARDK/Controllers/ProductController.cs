using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YARDK.Models;
using System.IO;
using System.Text.Json;

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

        public async Task<ActionResult> AllProducts(string sortBy = "newest", string category = "all", bool isActive = true)
        {
            try
            {
                int pageSize = 10; // عدد المنتجات في كل صفحة

                // Store current values in ViewBag
                ViewBag.IsActive = isActive;
                ViewBag.CurrentSort = sortBy;
                ViewBag.CurrentCategory = category;

                var products = _context.Products
                    .Include(p => p.Seller)
                    .Include(p => p.ProductImages)
                    .Where(p => (isActive && p.Status == "Active" && p.Quantity > 0) || (!isActive && (p.Status != "Active" || p.Quantity == 0)));

                // الحصول على جميع الفئات الفريدة من قاعدة البيانات
                var uniqueCategories = await _context.Products
                    .Select(p => p.Category)
                    .Distinct()
                    .ToListAsync();

                Console.WriteLine("Available categories in database:");
                foreach (var cat in uniqueCategories)
                {
                    Console.WriteLine($"Category: '{cat}'");
                }

                if (category != "all")
                {
                    string categoryName = category switch
                    {
                        "Furniture" => "Furniture",
                        "Electrical_Appliances" => "Electrical Appliances", // تم تغيير & إلى مسافة
                        "Scrap_Surplus" => "Scrap & Surplus",
                        _ => category
                    };
                    Console.WriteLine($"Selected category: '{categoryName}'");
                    products = products.Where(p => p.Category == categoryName);
                }

                // تسجيل عدد المنتجات بعد التصفية
                var filteredCount = await products.CountAsync();
                Console.WriteLine($"Number of products after filtering: {filteredCount}");

                switch (sortBy)
                {
                    case "price_asc":
                        products = products.OrderBy(p => p.Price);
                        break;
                    case "price_desc":
                        products = products.OrderByDescending(p => p.Price);
                        break;
                    case "name_asc":
                        products = products.OrderBy(p => p.ProductName);
                        break;
                    case "name_desc":
                        products = products.OrderByDescending(p => p.ProductName);
                        break;
                    case "oldest":
                        products = products.OrderBy(p => p.CreatedAt);
                        break;
                    case "newest":
                    default:
                        products = products.OrderByDescending(p => p.CreatedAt);
                        break;
                }

                var totalItems = await products.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                var productsList = await products
                    .Select(p => new ProductViewModel
                    {
                        Id = p.Id,
                        ProductName = p.ProductName ?? "No Name",
                        Description = p.Description ?? "No Description",
                        Category = p.Category ?? "Uncategorized",
                        Quantity = p.Quantity ?? 0,
                        Price = p.Price ?? 0,
                        ImageUrl = p.ProductImages.Any() && p.ProductImages.FirstOrDefault(img => img.IsMainImage) != null 
                            ? p.ProductImages.FirstOrDefault(img => img.IsMainImage).ImageUrl 
                            : (p.ProductImages.Any() ? p.ProductImages.First().ImageUrl : (p.ImageUrl != null ? p.ImageUrl : "/img/no-image.jpg")),
                        SellerName = p.Seller != null ? p.Seller.Name : "Unknown Seller",
                        Status = p.Status
                    })
                    .ToListAsync();

                var categories = await _context.Products
                    .Select(p => p.Category)
                    .Distinct()
                    .Where(c => c != null)
                    .ToListAsync();

                ViewBag.SortBy = sortBy;
                ViewBag.Category = category;
                ViewBag.Categories = categories;
                ViewBag.TotalItems = totalItems;

                return View(productsList);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"An error occurred while fetching products: {ex.Message}";
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
            List<CartItemViewModel> cartItems = new List<CartItemViewModel>();
            
            try 
            {
                bool isLoggedIn = Request.Cookies["UserId"] != null;
                
                if (isLoggedIn)
                {
                    // User is logged in - check if they have items in their database cart
                    int userId = int.Parse(Request.Cookies["UserId"]);
                    
                    // First, merge any items from the session to the database
                    MergeSessionCartToDatabase(userId);
                    
                    // Get cart items from database
                    cartItems = GetCartItemsFromDatabase(userId);
                }
                else
                {
                    // User is not logged in - get cart from session
                    cartItems = GetCartFromSession();
                }
                
                Console.WriteLine($"Cart items count: {cartItems.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Cart action: {ex.Message}");
                // Log the error
            }
            
            return View(cartItems);
        }

        private List<CartItemViewModel> GetCartItemsFromDatabase(int userId)
        {
            var cartItems = new List<CartItemViewModel>();
            
            try
            {
                // Find the user's cart
                var cart = _context.Carts
                    .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                    .FirstOrDefault(c => c.UserId == userId);
                    
                if (cart != null && cart.CartItems.Any())
                {
                    // Convert database cart items to view model
                    cartItems = cart.CartItems.Select(ci => new CartItemViewModel
                    {
                        Id = ci.Id,
                        CartId = ci.CartId,
                        ProductId = ci.ProductId,
                        ProductName = ci.Product.ProductName,
                        Price = ci.Product.Price ?? 0,
                        Quantity = ci.Quantity,
                        ImageUrl = ci.Product.ImageUrl,
                        CreatedAt = ci.CreatedAt
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting cart items from database: {ex.Message}");
                // Log the error
            }
            
            return cartItems;
        }

        private void MergeSessionCartToDatabase(int userId)
        {
            try
            {
                // Get items from session
                var sessionCart = GetCartFromSession();
                
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

        // GET: ProductController/AddToCart
        public IActionResult AddToCart(int id)
        {
            try
            {
                // التحقق من وجود المنتج
                var product = _context.Products.FirstOrDefault(p => p.Id == id);
                if (product == null)
                {
                    return NotFound();
                }

                bool isLoggedIn = Request.Cookies["UserId"] != null;
                
                if (isLoggedIn)
                {
                    // User is logged in - add directly to database
                    int userId = int.Parse(Request.Cookies["UserId"]);
                    AddToCartDatabase(userId, product);
                }
                else
                {
                    // User is not logged in - add to session
                    AddToCartSession(product);
                }

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
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AddToCart: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while adding the product to your cart.";
                return RedirectToAction("Cart");
            }
        }

        private void AddToCartDatabase(int userId, Product product)
        {
            // Check if user has a cart
            var cart = _context.Carts.FirstOrDefault(c => c.UserId == userId);
            
            // If no cart exists, create one
            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    CreatedAt = DateTime.Now
                };
                
                _context.Carts.Add(cart);
                _context.SaveChanges(); // Save to get the cart ID
            }
            
            // Check if product already in cart
            var cartItem = _context.CartItems
                .FirstOrDefault(ci => ci.CartId == cart.Id && ci.ProductId == product.Id);
                
            if (cartItem != null)
            {
                // Update quantity if item already exists
                cartItem.Quantity += 1;
                cartItem.UpdatedAt = DateTime.Now;
            }
            else
            {
                // Add new item
                cartItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = product.Id,
                    Quantity = 1,
                    CreatedAt = DateTime.Now
                };
                
                _context.CartItems.Add(cartItem);
            }
            
            _context.SaveChanges();
        }

        private void AddToCartSession(Product product)
        {
            // الحصول على السلة الحالية من الجلسة أو إنشاء سلة جديدة
            List<CartItemViewModel> cart = GetCartFromSession();

            // التحقق مما إذا كان المنتج موجودًا بالفعل في السلة
            var existingItem = cart.FirstOrDefault(item => item.ProductId == product.Id);
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
                    ImageUrl = product.ImageUrl,
                    CreatedAt = DateTime.Now
                });
            }

            // حفظ السلة المحدثة في الجلسة
            SaveCartToSession(cart);
        }

        // GET: ProductController/RemoveFromCart
        public IActionResult RemoveFromCart(int id)
        {
            try
            {
                bool isLoggedIn = Request.Cookies["UserId"] != null;
                
                if (isLoggedIn)
                {
                    // User is logged in - remove from database
                    int userId = int.Parse(Request.Cookies["UserId"]);
                    RemoveFromCartDatabase(userId, id);
                }
                else
                {
                    // User is not logged in - remove from session
                    RemoveFromCartSession(id);
                }
                
                // إضافة رسالة نجاح
                TempData["SuccessMessage"] = "Product removed from cart successfully.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in RemoveFromCart: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while removing the product from your cart.";
            }
            
            // العودة إلى سلة التسوق
            return RedirectToAction("Cart");
        }

        private void RemoveFromCartDatabase(int userId, int productId)
        {
            // Find the user's cart
            var cart = _context.Carts.FirstOrDefault(c => c.UserId == userId);
            
            if (cart != null)
            {
                // Find the cart item to remove
                var cartItem = _context.CartItems
                    .FirstOrDefault(ci => ci.CartId == cart.Id && ci.ProductId == productId);
                    
                if (cartItem != null)
                {
                    _context.CartItems.Remove(cartItem);
                    _context.SaveChanges();
                }
            }
        }

        private void RemoveFromCartSession(int productId)
        {
            // الحصول على السلة الحالية
            List<CartItemViewModel> cart = GetCartFromSession();
            
            // إزالة العنصر ذو معرف المنتج المطابق
            cart.RemoveAll(item => item.ProductId == productId);
            
            // حفظ السلة في الجلسة
            SaveCartToSession(cart);
        }

        // POST: ProductController/UpdateCartQuantity
        [HttpPost]
        public IActionResult UpdateCartQuantity(int id, int quantity)
        {
            try
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
                
                bool isLoggedIn = Request.Cookies["UserId"] != null;
                
                if (isLoggedIn)
                {
                    // User is logged in - update in database
                    int userId = int.Parse(Request.Cookies["UserId"]);
                    UpdateCartQuantityDatabase(userId, id, quantity);
                }
                else
                {
                    // User is not logged in - update in session
                    UpdateCartQuantitySession(id, quantity);
                }
                
                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateCartQuantity: {ex.Message}");
                return StatusCode(500);
            }
        }

        private void UpdateCartQuantityDatabase(int userId, int productId, int quantity)
        {
            // Find the user's cart
            var cart = _context.Carts.FirstOrDefault(c => c.UserId == userId);
            
            if (cart != null)
            {
                // Find the cart item to update
                var cartItem = _context.CartItems
                    .FirstOrDefault(ci => ci.CartId == cart.Id && ci.ProductId == productId);
                    
                if (cartItem != null)
                {
                    cartItem.Quantity = quantity;
                    cartItem.UpdatedAt = DateTime.Now;
                    _context.SaveChanges();
                }
            }
        }

        private void UpdateCartQuantitySession(int productId, int quantity)
        {
            // الحصول على السلة الحالية
            List<CartItemViewModel> cart = GetCartFromSession();
            
            // البحث عن العنصر وتحديث الكمية
            var item = cart.FirstOrDefault(item => item.ProductId == productId);
            if (item != null)
            {
                item.Quantity = quantity;
            }
            
            // حفظ السلة في الجلسة
            SaveCartToSession(cart);
        }

        public ActionResult Checkout()
        {
            // التحقق من تسجيل دخول المستخدم
            if (Request.Cookies["UserId"] == null)
            {
                return RedirectToAction("Login", "User");
            }

            // الحصول على معرف المستخدم
            int userId = int.Parse(Request.Cookies["UserId"]);

            // الحصول على عناصر السلة
            List<CartItemViewModel> cartItems = new List<CartItemViewModel>();
            
            // محاولة الحصول على عناصر السلة من قاعدة البيانات
            var dbCartItems = GetCartItemsFromDatabase(userId);
            
            if (dbCartItems != null && dbCartItems.Any())
            {
                cartItems = dbCartItems;
            }
            else
            {
                // الحصول على عناصر السلة من الجلسة إذا لم تكن موجودة في قاعدة البيانات
                cartItems = GetCartFromSession();
            }
            
            if (cartItems == null || !cartItems.Any())
            {
                return RedirectToAction("Cart");
            }

            // حساب المبالغ
            decimal subtotal = cartItems.Sum(item => item.Price * item.Quantity);
            decimal shipping = Math.Round(subtotal * 0.05m, 2);
            decimal total = subtotal + shipping;

            // الحصول على بيانات المستخدم
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            
            if (user == null)
            {
                return RedirectToAction("Login", "User");
            }
            
            // محاولة استخراج الاسم الأول والأخير
            string firstName = user.Name;
            string lastName = "";
            
            // إذا كان الاسم يحتوي على مسافة، افصل الاسم الأول والأخير
            if (user.Name != null && user.Name.Contains(" "))
            {
                var nameParts = user.Name.Split(' ', 2);
                firstName = nameParts[0];
                lastName = nameParts[1];
            }
            
            // الحصول على بيانات بطاقة الدفع إن وجدت
            var paymentMethod = _context.PaymentMethods.FirstOrDefault(p => p.UserId == userId);

            // إنشاء نموذج البيانات
            var viewModel = new CheckoutViewModel
            {
                CartItems = cartItems,
                Subtotal = subtotal,
                Shipping = shipping,
                Total = total,
                FirstName = firstName,
                LastName = lastName,
                Email = user.Email,
                Phone = user.Phone,
                UserId = userId,
                SavedPaymentMethod = paymentMethod
            };
            
            // تعبئة بيانات بطاقة الدفع إذا كانت موجودة
            if (paymentMethod != null)
            {
                viewModel.CardNumber = paymentMethod.CardNumber;
                viewModel.ExpiryDate = paymentMethod.ExpiryDate;
                viewModel.CVV = paymentMethod.CVV;
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessOrder(CheckoutViewModel model)
        {
            try
            {
                // التحقق من تسجيل دخول المستخدم
                if (Request.Cookies["UserId"] == null)
                {
                    return Json(new { success = false, message = "Please login to complete your order." });
                }

                int userId = int.Parse(Request.Cookies["UserId"]);
                bool isModelValid = true;

                // التحقق من صحة البيانات
                if (string.IsNullOrWhiteSpace(model.FirstName))
                {
                    ModelState.AddModelError("FirstName", "First name is required");
                    isModelValid = false;
                }

                if (string.IsNullOrWhiteSpace(model.LastName))
                {
                    ModelState.AddModelError("LastName", "Last name is required");
                    isModelValid = false;
                }

                if (string.IsNullOrWhiteSpace(model.Email))
                {
                    ModelState.AddModelError("Email", "Email is required");
                    isModelValid = false;
                }

                if (string.IsNullOrWhiteSpace(model.Phone))
                {
                    ModelState.AddModelError("Phone", "Phone number is required");
                    isModelValid = false;
                }

                if (string.IsNullOrWhiteSpace(model.Address))
                {
                    ModelState.AddModelError("Address", "Address is required");
                    isModelValid = false;
                }

                if (string.IsNullOrWhiteSpace(model.City))
                {
                    ModelState.AddModelError("City", "City is required");
                    isModelValid = false;
                }

                if (string.IsNullOrWhiteSpace(model.PostalCode))
                {
                    ModelState.AddModelError("PostalCode", "Postal code is required");
                    isModelValid = false;
                }

                // التحقق من بيانات بطاقة الائتمان إذا لم تكن هناك بطاقة محفوظة
                var savedPaymentMethod = await _context.PaymentMethods.FirstOrDefaultAsync(p => p.UserId == userId);
                
                if (savedPaymentMethod == null)
                {
                    if (string.IsNullOrWhiteSpace(model.CardNumber))
                    {
                        ModelState.AddModelError("CardNumber", "Card number is required");
                        isModelValid = false;
                    }
                    
                    if (string.IsNullOrWhiteSpace(model.ExpiryDate))
                    {
                        ModelState.AddModelError("ExpiryDate", "Expiry date is required");
                        isModelValid = false;
                    }
                    
                    if (string.IsNullOrWhiteSpace(model.CVV))
                    {
                        ModelState.AddModelError("CVV", "CVV is required");
                        isModelValid = false;
                    }
                }
                else
                {
                    // استخدام البطاقة المحفوظة
                    model.CardNumber = savedPaymentMethod.CardNumber;
                    model.ExpiryDate = savedPaymentMethod.ExpiryDate;
                    model.CVV = savedPaymentMethod.CVV;
                }
                
                if (!isModelValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    
                    return Json(new { success = false, message = string.Join("<br>", errors) });
                }

                // الحصول على عناصر السلة
                List<CartItemViewModel> cartItems;
                var dbCartItems = GetCartItemsFromDatabase(userId);
                
                if (dbCartItems != null && dbCartItems.Any())
                {
                    cartItems = dbCartItems;
                }
                else
                {
                    cartItems = GetCartFromSession();
                }
                
                if (cartItems == null || !cartItems.Any())
                {
                    return Json(new { success = false, message = "Your cart is empty." });
                }

                // حساب المبالغ
                decimal subtotal = cartItems.Sum(item => item.Price * item.Quantity);
                decimal shipping = Math.Round(subtotal * 0.05m, 2);
                decimal total = subtotal + shipping;

                // تنظيف وتحضير بيانات الطلب
                var shippingAddress = model.Address?.Trim() ?? "";
                var city = model.City?.Trim() ?? "";
                var postalCode = model.PostalCode?.Trim() ?? "";
                
                if (string.IsNullOrWhiteSpace(shippingAddress) || string.IsNullOrWhiteSpace(city))
                {
                    return Json(new { success = false, message = "Please provide a valid shipping address." });
                }

                // ضبط القيم الافتراضية
                string paymentMethod = "Visa";
                string orderNumber = GenerateOrderNumber();

                // إنشاء الطلب
                var order = new Order
                {
                    UserId = userId,
                    OrderNumber = orderNumber,
                    TotalAmount = total,
                    Status = "Pending",
                    ShippingAddress = $"{shippingAddress}, {city}, {postalCode}",
                    BillingAddress = $"{shippingAddress}, {city}, {postalCode}",
                    PaymentMethod = paymentMethod,
                    PaymentStatus = "Pending",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // التحقق من وجود المستخدم
                        var user = await _context.Users.FindAsync(userId);
                        if (user == null)
                        {
                            return Json(new { success = false, message = "User not found." });
                        }

                        // حفظ الطلب
                        _context.Orders.Add(order);
                        await _context.SaveChangesAsync();

                        // إضافة عناصر الطلب
                        foreach (var item in cartItems)
                        {
                            var product = await _context.Products.FindAsync(item.ProductId);
                            if (product == null)
                            {
                                throw new Exception($"Product with ID {item.ProductId} not found.");
                            }

                            if (product.Quantity < item.Quantity)
                            {
                                throw new Exception($"Insufficient quantity for product {product.ProductName}.");
                            }

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
                            
                            // تحديث كمية المنتج
                            product.Quantity -= item.Quantity;
                            _context.Products.Update(product);
                        }

                        // حفظ جميع التغييرات
                        await _context.SaveChangesAsync();

                        // إنشاء سجل الدفع
                        var payment = new Payment
                        {
                            OrderId = order.Id,
                            Amount = total,
                            PaymentMethod = paymentMethod,
                            PaymentStatus = "Pending",
                            TransactionId = GenerateTransactionId(),
                            PaymentDate = DateTime.Now,
                            LastUpdated = DateTime.Now
                        };

                        _context.Payments.Add(payment);
                        await _context.SaveChangesAsync();

                        // تفريغ السلة
                        await ClearUserCart(userId);
                        HttpContext.Session.Remove("Cart");

                        // تأكيد المعاملة
                        await transaction.CommitAsync();

                        return Json(new { 
                            success = true, 
                            message = "Order placed successfully!",
                            redirectUrl = Url.Action("OrderConfirmation", "Product", new { orderId = order.Id })
                        });
                    }
                    catch (Exception ex)
                    {
                        // التراجع عن المعاملة في حالة حدوث خطأ
                        await transaction.RollbackAsync();
                        Console.WriteLine($"Error processing order: {ex.Message}");
                        Console.WriteLine($"Stack trace: {ex.StackTrace}");
                        if (ex.InnerException != null)
                        {
                            Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                            Console.WriteLine($"Inner stack trace: {ex.InnerException.StackTrace}");
                        }
                        return Json(new { 
                            success = false, 
                            message = $"An error occurred while processing your order: {ex.Message}. Please try again or contact customer support."
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ProcessOrder: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    Console.WriteLine($"Inner stack trace: {ex.InnerException.StackTrace}");
                }
                return Json(new { 
                    success = false, 
                    message = $"An error occurred while processing your order: {ex.Message}. Please try again or contact customer support."
                });
            }
        }

        private async Task ClearUserCart(int userId)
        {
            try
            {
                // البحث عن سلة المستخدم
                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.UserId == userId);
                    
                if (cart != null && cart.CartItems.Any())
                {
                    // حذف جميع عناصر السلة
                    _context.CartItems.RemoveRange(cart.CartItems);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing user cart: {ex.Message}");
            }
        }

        private string GenerateOrderNumber()
        {
            return "ORD-" + DateTime.Now.ToString("yyyyMMddHHmmss");
        }

        private string GenerateTransactionId()
        {
            return "TRX-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
        }

        [HttpGet]
        public IActionResult OrderConfirmation(int orderId)
        {
            try
            {
                // التحقق من تسجيل دخول المستخدم
                if (Request.Cookies["UserId"] == null)
                {
                    return RedirectToAction("Login", "User");
                }

                int userId = int.Parse(Request.Cookies["UserId"]);

                // الحصول على تفاصيل الطلب مع العناصر والمنتجات
                var order = _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .FirstOrDefault(o => o.Id == orderId && o.UserId == userId);

                if (order == null)
                {
                    TempData["ErrorMessage"] = "Order not found or access denied.";
                    return RedirectToAction("MyOrders", "User");
                }

                return View(order);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OrderConfirmation: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while retrieving order details.";
                return RedirectToAction("MyOrders", "User");
            }
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.ProductImages)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product == null)
                {
                    return Json(new { 
                        success = false, 
                        message = "Product not found.",
                        messageAr = "المنتج غير موجود"
                    });
                }

                // تحديث حالة المنتج بدلاً من حذفه
                product.Quantity = 0;
                product.Status = "Inactive";
                
                _context.Products.Update(product);
                await _context.SaveChangesAsync();

                return Json(new { 
                    success = true, 
                    message = "Product has been deleted successfully.",
                    messageAr = "تم حذف المنتج بنجاح"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner error: {ex.InnerException.Message}");
                }

                return Json(new { 
                    success = false, 
                    message = "An error occurred while processing your request.",
                    messageAr = "حدث خطأ أثناء معالجة طلبك"
                });
            }
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
                return JsonSerializer.Deserialize<List<CartItemViewModel>>(cart);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deserializing cart: {ex.Message}");
                return new List<CartItemViewModel>();
            }
        }

        private void SaveCartToSession(List<CartItemViewModel> cart)
        {
            try
            {
                var cartJson = JsonSerializer.Serialize(cart);
                HttpContext.Session.SetString("Cart", cartJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error serializing cart: {ex.Message}");
            }
        }

        public ActionResult AdminProductDetails(int id)
        {
            try
            {
                var product = _context.Products
                    .Include(p => p.Seller)
                    .Include(p => p.ProductImages)
                    .FirstOrDefault(p => p.Id == id);

                if (product == null)
                {
                    TempData["ErrorMessage"] = "Product not found.";
                    return RedirectToAction("AllProducts");
                }

                return View(product);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                return RedirectToAction("AllProducts");
            }
        }
    }
}
