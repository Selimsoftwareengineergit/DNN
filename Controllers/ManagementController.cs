using Microsoft.AspNetCore.Mvc;
using DNN.Data;        
using DNN.Models;   
using System.Linq;
using DNN.Models;

namespace DNN.Controllers
{
    public class ManagementController : Controller
    {
        private readonly DNNDbContext _context;

        public ManagementController(DNNDbContext context)
        {
            _context = context;
        }

        public IActionResult Notice()
        {
            // Fetch all notices from DB
            var notices = _context.Notices
                .OrderByDescending(n => n.EntryDate)
                .ToList();

            return View(notices);
        }

        public IActionResult CreateNotice()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateNotice(Notice model)
        {
            if (ModelState.IsValid)
            {
                if (model.EntryDate == default)
                    model.EntryDate = DateTime.UtcNow;

                _context.Notices.Add(model);
                _context.SaveChanges();

                TempData["Success"] = "Notice created successfully!";
                return RedirectToAction(nameof(Notice));
            }

            TempData["Error"] = "Failed to create notice. Please check the details.";
            return View(model);
        }

        public IActionResult EditNotice(int id)
        {
            var notice = _context.Notices.Find(id);
            if (notice == null)
            {
                TempData["Error"] = "Notice not found.";
                return RedirectToAction(nameof(Notice));
            }

            return View(notice);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditNotice(int id, Notice model)
        {
            if (id != model.Id)
            {
                TempData["Error"] = "Invalid notice ID.";
                return RedirectToAction(nameof(Notice));
            }

            if (ModelState.IsValid)
            {
                var existingNotice = _context.Notices.Find(id);
                if (existingNotice == null)
                {
                    TempData["Error"] = "Notice not found.";
                    return RedirectToAction(nameof(Notice));
                }

                // Update fields
                existingNotice.Subject = model.Subject;
                existingNotice.Description = model.Description;
                existingNotice.EntryDate = model.EntryDate;
                existingNotice.ExpireDate = model.ExpireDate;
                existingNotice.IsActive = model.IsActive;

                _context.SaveChanges();

                TempData["Success"] = "Notice updated successfully!";
                return RedirectToAction(nameof(Notice));
            }

            TempData["Error"] = "Failed to update notice. Please check your input.";
            return View(model);
        }
    }
}

