using Microsoft.AspNetCore.Mvc;
using DNN.Data;        
using DNN.Models;   
using System.Linq;

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

        public IActionResult Advertisement()
        {
            var ads = _context.Banners
                .OrderByDescending(x => x.StartDate)
                .ToList();

            return View(ads);
        }

        public IActionResult CreateAdvertisement()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateAdvertisement(Banner model, IFormFile? ImageFile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Handle Image Upload
                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        // Create upload folder if it doesn’t exist
                        var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/banners");
                        if (!Directory.Exists(uploadDir))
                            Directory.CreateDirectory(uploadDir);

                        // Generate unique filename
                        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(ImageFile.FileName)}";
                        var filePath = Path.Combine(uploadDir, fileName);

                        // Save file
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            ImageFile.CopyTo(stream);
                        }

                        // Store relative path in database
                        model.ImagePath = $"/images/banners/{fileName}";
                    }
                    else
                    {
                        // If no file uploaded, show message
                        TempData["Error"] = "Please upload a banner image.";
                        return View(model);
                    }

                    // Auto set system fields
                    model.CreatedAt = DateTime.Now;
                    model.IsActive = true;
                    model.Impressions = 0;
                    model.Clicks = 1;
                    model.CreatedBy = User.Identity?.Name ?? "System";

                    _context.Banners.Add(model);
                    _context.SaveChanges();

                    TempData["Success"] = "Advertisement created successfully!";
                    return RedirectToAction("Advertisement");
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Error while creating advertisement: {ex.Message}";
                    return View(model);
                }
            }
            else
            {
                // This executes if validation fails
                TempData["Error"] = "Please correct the highlighted errors and try again.";

                // Optional: Debug info for developer
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine(error.ErrorMessage);
                }

                return View(model);
            }
        }


        [HttpGet]
        public IActionResult EditAdvertisement(int id)
        {
            var banner = _context.Banners.FirstOrDefault(b => b.Id == id);
            if (banner == null)
            {
                TempData["Error"] = "Advertisement not found.";
                return RedirectToAction("Advertisement");
            }
            return View(banner);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditAdvertisement(Banner model, IFormFile? ImageFile)
        {
            ModelState.Remove("ImagePath");

            if (ModelState.IsValid)
            {
                var existing = _context.Banners.FirstOrDefault(b => b.Id == model.Id);
                if (existing == null)
                {
                    TempData["Error"] = "Advertisement not found.";
                    return RedirectToAction("Advertisement");
                }

                try
                {
                    // Handle new image upload
                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/banners");
                        if (!Directory.Exists(uploadDir))
                            Directory.CreateDirectory(uploadDir);

                        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(ImageFile.FileName)}";
                        var filePath = Path.Combine(uploadDir, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            ImageFile.CopyTo(stream);
                        }

                        existing.ImagePath = $"/images/banners/{fileName}";
                    }

                    // Update fields
                    existing.CompanyName = model.CompanyName;
                    existing.Title = model.Title;
                    existing.ClickUrl = model.ClickUrl;
                    existing.BannerType = model.BannerType;
                    existing.Target = model.Target;
                    existing.StartDate = model.StartDate;
                    existing.EndDate = model.EndDate;
                    existing.Priority = model.Priority;
                    existing.Description = model.Description;
                    existing.UpdatedAt = DateTime.Now;
                    existing.UpdatedBy = User.Identity?.Name ?? "System";

                    _context.Banners.Update(existing);
                    _context.SaveChanges();

                    TempData["Success"] = "Advertisement updated successfully!";
                    return RedirectToAction("Advertisement");
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Error while updating advertisement: {ex.Message}";
                }
            }
            else
            {
                TempData["Error"] = "Please correct the highlighted errors and try again.";
            }

            return View(model);
        }


        // ✅ Deactivate Advertisement
        public IActionResult DeactivateAdvertisement(int id)
        {
            var ad = _context.Banners.Find(id);
            if (ad != null)
            {
                ad.IsActive = false;
                _context.SaveChanges();
                TempData["Success"] = "Advertisement deactivated successfully!";
            }
            else
            {
                TempData["Error"] = "Advertisement not found.";
            }

            return RedirectToAction("Advertisement");
        }
    }
}


