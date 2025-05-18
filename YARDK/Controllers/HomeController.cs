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
            List<Category> categories = new List<Category>();
            List<dynamic> feedbacks = new List<dynamic>();

            try
            {
                // محاولة الحصول على الفئات من قاعدة البيانات
                categories = await _context.Categories.ToListAsync();
                
                // الحصول على آخر 4 تقييمات من قاعدة البيانات
                var dbFeedbacks = await _context.Feedbacks
                    .OrderByDescending(f => f.CreatedAt)
                    .Take(4)
                    .ToListAsync();
                
                // تحقق من وجود تقييمات في قاعدة البيانات
                if (dbFeedbacks != null && dbFeedbacks.Any())
                {
                    feedbacks = dbFeedbacks.Select(f => new
                    {
                        Name = f.Name,
                        Message = f.Message
                    }).ToList<dynamic>();
                    
                    _logger.LogInformation($"تم جلب {feedbacks.Count} تقييمات من قاعدة البيانات");
                }
                else
                {
                    _logger.LogWarning("لا توجد تقييمات في قاعدة البيانات");
                }
                
                ViewBag.Feedbacks = feedbacks;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "حدث خطأ أثناء الحصول على البيانات من قاعدة البيانات");
                ViewBag.Feedbacks = new List<object>();
            }

            return View(categories);
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(Feedback feedback)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    feedback.CreatedAt = DateTime.Now;
                    _context.Feedbacks.Add(feedback);
                    await _context.SaveChangesAsync();

                    return Json(new { success = true });
                }

                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return Json(new { success = false, message = string.Join(", ", errors) });
            }
            catch (Exception ex)
            {
                // Log the error
                return Json(new { success = false, message = "An error occurred while processing your request." });
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
