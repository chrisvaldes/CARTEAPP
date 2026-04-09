using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using SYSGES_MAGs.Data; 
using SYSGES_MAGs.Middleware;
using SYSGES_MAGs.Repository;
using SYSGES_MAGs.Repository.IRepository;
using SYSGES_MAGs.Services;
using SYSGES_MAGs.Services.IServices;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
 

// connection à la base de données Postgresql
var connectionString = builder.Configuration.GetConnectionString("PostgresConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Register IHttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Register des services et repository
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddScoped<IAuthService, AuthService>(); 
builder.Services.AddScoped<IProfileService, ProfileService>(); 
builder.Services.AddScoped<IProfileRepository, ProfileRepository>();
builder.Services.AddScoped<IMagProcessingService, MagProcessingService>();
builder.Services.AddScoped<IBkPrdCliRepository, BkPrdCliRepository>();
builder.Services.AddScoped<IBkmvtiRepository, BkmvtiRepository>();
builder.Services.AddScoped<ITypeMagRepository, TypeMagRepository>(); 
builder.Services.AddScoped<IBkmvtiService, BkmvtiService>();
 
ExcelPackage.License.SetNonCommercialPersonal("SYSGES-MAGs");

var app = builder.Build();



// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
// Serve static files (css/js/images)
app.UseStaticFiles();

app.UseRouting();

// Ensure authentication middleware runs before authorization and before custom JWT middleware if you rely on built-in auth
app.UseAuthentication();

// Custom JWT middleware that validates token from header/cookie and sets context.User
app.UseMiddleware<JwtAuthMiddleware>();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}")
    .WithStaticAssets();

app.Run();