using DNN.Data;
using Microsoft.EntityFrameworkCore;
using DNN.Services.Hubs;
using DNN.Services; // Your Hub namespace

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// ✅ Add DbContext
builder.Services.AddDbContext<DNNDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DNNDbConnection")));
// Register EmailSettings from appsettings.json
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// Register EmailService for dependency injection
builder.Services.AddTransient<EmailService>();


// ✅ Add session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ✅ Register IHttpContextAccessor
builder.Services.AddHttpContextAccessor();

// ✅ Register SignalR
builder.Services.AddSignalR();

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

// ✅ Map SignalR hub endpoint
app.MapHub<StudentRequestHub>("/studentRequestHub");

app.Run();
