using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using DNN.Data;
using DNN.Models;
using DNN.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DNN.Controllers
{
    public class HomeController : Controller
    {
        private readonly DNNDbContext _context;

        public HomeController(DNNDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Register()
        {
            ViewBag.Roles = _context.Roles
                             .Select(r => new SelectListItem
                             {
                                 Value = r.RoleId.ToString(),
                                 Text = r.RoleName
                             })
                             .ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegistrationViewModel model)
        {
            // Populate Roles for dropdown if validation fails
            ViewBag.Roles = _context.Roles.ToList();

            if (ModelState.IsValid)
            {
                // Check if username already exists
                if (_context.Users.Any(u => u.Username == model.UserName))
                {
                    ModelState.AddModelError("", "User already exists!");
                    return View(model);
                }

                // Hash password
                var passwordHash = HashPassword(model.Password);

                // Create new User object
                var user = new User
                {
                    Username = model.UserName,
                    PasswordHash = passwordHash,
                    FullName = model.FullName,
                    Email = model.Email,
                    MobileNumber = model.MobileNumber, 
                    RoleId = model.RoleId,
                    CreatedDate = DateTime.Now,
                    IsActive = true
                };

                // Add and save to database
                _context.Users.Add(user);
                _context.SaveChanges();

                // Optionally redirect to login page or confirmation page
                return RedirectToAction("Index", "Home");
            }

            // If we reach here, something failed
            return View(model);
        }


        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (var b in bytes) builder.Append(b.ToString("x2"));
                return builder.ToString();
            }
        }
    }
}