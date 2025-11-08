using DNN.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DNN.Controllers
{
    public class StudentsController : Controller
    {
        private readonly DNNDbContext _context;

        public StudentsController(DNNDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var today = DateTime.Now;

            // Load active notices (not expired)
            var activeNotices = _context.Notices
                .Where(n => n.IsActive && (n.ExpireDate == null || n.ExpireDate > today))
                .OrderByDescending(n => n.EntryDate)
                .Take(5)
                .ToList();

            // Load active banners (within date range)
            var activeBanners = _context.Banners
                .Where(b => b.IsActive && b.StartDate <= today && b.EndDate >= today)
                .OrderBy(b => b.Priority)
                .ToList();

            // Pass data to View
            ViewBag.ActiveNotices = activeNotices;
            ViewBag.ActiveBanners = activeBanners;

            return View();
        }
    }
}