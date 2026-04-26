using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Data;
using Microsoft.AspNetCore.Identity;
using MoneyTracker.Services;
using Microsoft.AspNetCore.DataProtection;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// ==================== SERVICIOS ====================

// PostgreSQL (versión completa para evitar errores de using)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseNpgsql(connString);   // Usando método completo
});

// Identity
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = false;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

// Data Protection
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/app/DataProtectionKeys"))
    .SetApplicationName("DriverFlow");

// Email Service
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<EmailService>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// ==================== PIPELINE ====================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// ==================== MIGRACIONES ====================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();
        Console.WriteLine("✅ Migraciones PostgreSQL OK");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error migraciones: {ex.Message}");
    }
}

app.Run();