using GotoCarRental.Data;
using GotoCarRental.Models;
using GotoCarRental.Repository;
using GotoCarRental.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Configure Identity options
builder.Services.Configure<IdentityOptions>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 6;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;

    // User settings
    options.User.RequireUniqueEmail = true;
});

builder.Services.AddControllersWithViews();

// Thêm repositories
builder.Services.AddScoped<ICarRepository, EFCarRepository>();
builder.Services.AddScoped<IBrandRepository, EFBrandRepository>();
builder.Services.AddScoped<ICategoryRepository, EFCategoryRepository>();
builder.Services.AddScoped<IFeatureRepository, EFFeatureRepository>();
builder.Services.AddScoped<IRentalRepository, EFRentalRepository>();
builder.Services.AddScoped<IReviewRepository, EFReviewRepository>();
builder.Services.AddScoped<IModel3DTemplateRepository, EFModel3DTemplateRepository>();
builder.Services.AddScoped<IProvinceRepository, EFProvinceRepository>();

// Đăng ký services
builder.Services.AddScoped<IGeoLocationService, GeoLocationService>();







var app = builder.Build();

// Seed roles and default admin user
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await RoleSeeder.SeedRolesAsync(services);

}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
// Thêm sau app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = new FileExtensionContentTypeProvider
    {
        Mappings =
        {
            [".glb"] = "model/gltf-binary",
            [".gltf"] = "model/gltf+json"
        }
    }
});



app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

app.MapControllers();


app.MapRazorPages();
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");




app.Run();







