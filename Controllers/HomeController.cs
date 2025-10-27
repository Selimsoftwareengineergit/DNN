using System.Security.Cryptography;
using System.Text;
using DNN.Data;
using DNN.Models;
using DNN.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;


namespace DNN.Controllers
{
    public class HomeController : Controller
    {
        private readonly DNNDbContext _context;
        private const int PageSize = 10;
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
                    CreatedDate = u.CreatedDate
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