using System.Security.Cryptography;
using System.Text;
using DNN.Data;
using DNN.Models;
using DNN.Models.ViewModels;
using DNN.Services;
using DNN.Services.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;


namespace DNN.Controllers
{
    public class HomeController : Controller
    {
        private readonly DNNDbContext _context;
        private const int PageSize = 10;
        private readonly IHubContext<StudentRequestHub> _hub;
        private readonly EmailService _emailService;
        public HomeController(DNNDbContext context, IHubContext<StudentRequestHub> hub, EmailService emailService)
        {
            _context = context;
            _hub = hub;
            _emailService = emailService;
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
        public async Task<IActionResult> Register(RegistrationViewModel model, IFormFile? ProfileImage)
        
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

                // Initialize user
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

                // ✅ Handle Profile Image Upload
                if (ProfileImage != null && ProfileImage.Length > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
                    var fileExtension = Path.GetExtension(ProfileImage.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("", "Invalid image format! Please upload JPG, JPEG, PNG, GIF, BMP, or WEBP files only.");
                        return View(model);
                    }

                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/profiles");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await ProfileImage.CopyToAsync(fileStream);
                    }

                    user.ProfileImagePath = $"/uploads/profiles/{uniqueFileName}";
                }

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "Home");
            }
            return View(model);
        }


        // GET: /Admin/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserId == id);
            if (user == null) return NotFound();

            var model = new EditUserViewModel
            {
                UserId = user.UserId,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                MobileNumber = user.MobileNumber,
                RoleId = user.RoleId,
                IsActive = user.IsActive
            };

            ViewBag.Roles = await _context.Roles.OrderBy(r => r.RoleName).ToListAsync();
            return View(model); // Create/Edit view separately
        }

        public async Task<IActionResult> ManageUsers(int page = 1, string query = "")
        {
            if (page < 1) page = 1;

            // ✅ Step 1: Start with all users (materialize the query first)
            IQueryable<User> usersQuery = _context.Users
                .Include(u => u.Role)
                .AsNoTracking();

            // ✅ Step 2: Apply search filter
            if (!string.IsNullOrWhiteSpace(query))
            {
                string searchText = query.Trim().ToLower();

                usersQuery = usersQuery.Where(u =>
                    u.Username.ToLower().Contains(searchText) ||
                    u.FullName.ToLower().Contains(searchText) ||
                    u.Email.ToLower().Contains(searchText) ||
                    u.MobileNumber.ToLower().Contains(searchText) ||
                    (u.Role != null && u.Role.RoleName.ToLower().Contains(searchText))
                );
            }

            // ✅ Step 3: Materialize the query to List first to avoid async issues
            var filteredUsers = usersQuery
                .OrderByDescending(u => u.CreatedDate)
                .ToList();

            // ✅ Step 4: Get Total Count (synchronous)
            int totalCount = filteredUsers.Count;

            // ✅ Step 5: Calculate Pagination
            int totalPages = (int)Math.Ceiling(totalCount / (double)PageSize);
            if (totalPages == 0) totalPages = 1;
            if (page > totalPages) page = totalPages;

            // ✅ Step 6: Fetch Paginated Data (synchronous)
            var users = filteredUsers
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .Select(u => new UserListItemViewModel
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    FullName = u.FullName,
                    Email = u.Email,
                    MobileNumber = u.MobileNumber,
                    RoleName = u.Role != null ? u.Role.RoleName : "",
                    IsActive = u.IsActive,
                    CreatedDate = u.CreatedDate,
                    ProfileImagePath = u.ProfileImagePath
                })
                .ToList();

            // ✅ Step 7: Prepare ViewModel
            var vm = new ManageUsersViewModel
            {
                Users = users,
                CurrentPage = page,
                TotalPages = totalPages,
                Query = query
            };

            // ✅ Step 8: Return the View (keep method async but use Task.FromResult)
            return await Task.FromResult(View(vm));
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers(string query = "", int page = 1)
        {
            var usersQuery = _context.Users
                .Include(u => u.Role)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                var q = query.Trim();
                usersQuery = usersQuery.Where(u =>
                    u.Username.Contains(q) ||
                    u.FullName.Contains(q) ||
                    u.Email.Contains(q) ||
                    u.MobileNumber.Contains(q) ||
                    (u.Role != null && u.Role.RoleName.Contains(q))
                );
            }

            var totalCount = await usersQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)PageSize);

            var users = await usersQuery
                .OrderByDescending(u => u.CreatedDate)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .Select(u => new UserListItemViewModel
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    FullName = u.FullName,
                    Email = u.Email,
                    MobileNumber = u.MobileNumber,
                    RoleName = u.Role != null ? u.Role.RoleName : "",
                    IsActive = u.IsActive,
                    CreatedDate = u.CreatedDate
                })
                .ToListAsync();

            return Json(new
            {
                users,
                pagination = new
                {
                    currentPage = page,
                    totalPages
                }
            });
        }

        // POST: /Admin/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = await _context.Roles.OrderBy(r => r.RoleName).ToListAsync();
                return View(model);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == model.UserId);
            if (user == null) return NotFound();

            // Update fields
            user.Username = model.Username;
            user.FullName = model.FullName;
            user.Email = model.Email;
            user.MobileNumber = model.MobileNumber;
            user.RoleId = model.RoleId;
            user.IsActive = model.IsActive;

            await _context.SaveChangesAsync();

            // Redirect to ManageUsers page
            return RedirectToAction("ManageUsers");
        }

        // POST: /Admin/Deactivate/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.IsActive = false;
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Please enter both username and password.";
                return View();
            }

            // ✅ Hash the entered password before comparing
            string hashedPassword = HashPassword(password);

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == username && u.PasswordHash == hashedPassword);

            if (user == null)
            {
                ViewBag.Error = "Invalid username or password.";
                return View();
            }

            if (!user.IsActive)
            {
                ViewBag.Error = "Your account is deactivated.";
                return View();
            }

            // ✅ Store data in session
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("UserRole", user.Role?.RoleName ?? "User");

            // ✅ Redirect based on role
            if (user.Role?.RoleName == "Admin")
                return RedirectToAction("ManageUsers", "Home");
            else
                return RedirectToAction("Index", "Dashboard");
        }

        // GET: /Home/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                ViewBag.Error = "Please enter your username.";
                return View();
            }

            // Check if the user exists
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                ViewBag.Error = "Username not found.";
                return View();
            }

            // Create a new request
            var request = new StudentPasswordRequest
            {
                Username = username,
                RequestType = "Reset Password",
                Status = "Pending",
                RequestDate = DateTime.Now
            };

            _context.StudentPasswordRequests.Add(request);
            await _context.SaveChangesAsync();

            ViewBag.Message = "Your password reset request has been submitted. Admin will handle it soon.";
            return View();
        }

        public async Task<IActionResult> StudentPasswordRequests()
        {
            var requests = await _context.StudentPasswordRequests
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();

            return View(requests);
        }

        [HttpPost]
        public async Task<IActionResult> HandlePasswordRequest(int id, string actionType)
        {
            var request = await _context.StudentPasswordRequests.FindAsync(id);
            if (request == null)
                return NotFound();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
            if (user == null)
                return NotFound();

            if (actionType == "ResetPassword")
            {
                // Generate new password and hash it
                var newPassword = GenerateRandomPassword();
                user.PasswordHash = HashPassword(newPassword);

                request.NewPassword = newPassword;
                request.Status = "Completed";
                request.CompletedDate = DateTime.Now;

                // ✅ Send email via EmailService
                await _emailService.SendResetPasswordEmailAsync(user, newPassword);
            }
            else if (actionType == "KnowOldPassword")
            {
                request.Status = "Completed";
                request.CompletedDate = DateTime.Now;

                // ✅ Send "old password not recoverable" email via EmailService
                await _emailService.SendPasswordNotRecoverableEmailAsync(user);
            }

            await _context.SaveChangesAsync();

            // ✅ Notify all connected admins via SignalR
            await _hub.Clients.All.SendAsync("ReceiveRequestUpdate", $"Request for {request.Username} has been handled.");

            return RedirectToAction(nameof(StudentPasswordRequests));
        }

        private string GenerateRandomPassword(int length = 8)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789@#$!";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
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