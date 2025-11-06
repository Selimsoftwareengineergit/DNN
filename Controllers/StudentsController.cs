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
            var activeNotices = _context.Notices
                .Where(n => n.IsActive && (n.ExpireDate == null || n.ExpireDate > DateTime.Now))
                .OrderByDescending(n => n.EntryDate)
                .Take(5)
                .ToList();

            ViewBag.ActiveNotices = activeNotices;

            return View();
        }
    }
}