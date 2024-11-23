using Claim_System.Data; // Ensure this namespace is correct
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure Entity Framework with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ClaimDBConnection")));

// Add session support
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);  // Set session timeout duration
    options.Cookie.HttpOnly = true;  // Make session cookie more secure
    options.Cookie.IsEssential = true;  // Make sure cookie is always sent
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/AppViews/Error");
    app.UseHsts();
}

app.UseHttpsRedirection(); // Ensures all requests are redirected to HTTPS in production
app.UseStaticFiles(); // Enable static files (e.g., uploaded documents)

// Use session middleware for session management
app.UseSession();

// Routing middleware to handle controller actions
app.UseRouting();

// Authorization middleware (if you plan to implement authentication later)
app.UseAuthorization();

// Custom routes for specific views (login and claim routes for Lecturer and Contractor)
app.MapControllerRoute(
    name: "userType",
    pattern: "{controller=UserType}/{action=UserType}/{id?}");

// Lecturer Login route
app.MapControllerRoute(
    name: "login_Lecturer",
    pattern: "AppViews/login_Lecturer",
    defaults: new { controller = "Lecturer", action = "login_Lecturer" });

// Contractor Login route
app.MapControllerRoute(
    name: "login_Contractor",
    pattern: "AppViews/login_Contractor",
    defaults: new { controller = "Contractor", action = "login_Contractor" });

// Pending Claims route
app.MapControllerRoute(
    name: "pendingClaims",
    pattern: "AppViews/pendingClaims",
    defaults: new { controller = "Contractor", action = "pendingClaims" });

// Claim Status route
app.MapControllerRoute(
    name: "claimStatus",
    pattern: "AppViews/claimStatus/{claimId}",
    defaults: new { controller = "ClaimStatus", action = "claimStatus" });

// Default route if no specific path is matched
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();