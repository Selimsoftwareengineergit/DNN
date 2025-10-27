using DNN.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// ✅ Add DbContext
builder.Services.AddDbContext<DNNDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DNNDbConnection")));

// ✅ Add session support
builder.Services.AddDistributedMemoryCache(); // Required for session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // session timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ✅ Register IHttpContextAccessor
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// ✅ Use session before authorization
app.UseSession();

app.UseAuthorization();

// Your static assets mapping (if using DNN)
app.MapStaticAssets();

// Map default controller route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();