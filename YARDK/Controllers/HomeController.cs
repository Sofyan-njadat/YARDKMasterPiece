using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YARDK.Models;

namespace YARDK.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly MyDbContext _context;

        public HomeController(ILogger<HomeController> logger, MyDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            List<Category> categories;

            try
            {
                // محاولة الحصول على الفئات من قاعدة البيانات
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
                _logger.LogError(ex, "حدث خطأ أثناء الحصول على الفئات من قاعدة البيانات");
                // استخدام فئات افتراضية في حالة حدوث خطأ
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

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
