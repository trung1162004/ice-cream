using Project.Data;
using Project.Helpers;
using Project.IdentitySettings;

using Project.IdentitySettings.Validators;
using Project.Models;
using Project.Models.BusinessModels;
using Project.Models.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Project.Services;
using Project.Repository;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
//builder.Services.AddDbContext<AppDbContext>()

builder.Services.AddScoped<IAuthorizationHandler, CheckPaymentExpiryHandler>();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<EmailHelper>();
builder.Services.AddScoped<RecipeRepository, RecipeServices>();
builder.Services.AddScoped<ContactRepository, ContactServices>();
builder.Services.AddScoped<IBookRepository, BookService>();
builder.Services.AddScoped<IOrderRepository, OrderService>();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<FeedbackRepository, FeedbackServices>();
builder.Services.AddScoped<OrderDashboardRepository, OrderDashboarServices>();
builder.Services.AddScoped<UserDetailRepo, UserDetailService>();

builder.Services.AddScoped<OrderRepository, OrderServices>();
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"));
});
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("vi-VN");
    options.SupportedCultures = new List<CultureInfo> { new CultureInfo("vi-VN") };
    options.SupportedUICultures = new List<CultureInfo> { new CultureInfo("vi-VN") };
});

builder.Services.AddSession();
builder.Services.AddIdentity<AppUser, AppRole>(options =>
{
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;

    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 1;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;

    options.SignIn.RequireConfirmedEmail = true;
}).AddUserValidator<UserValidator>()
.AddPasswordValidator<PasswordValidator>()
.AddErrorDescriber<ErrorDescriber>()
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = new PathString("/User/Login");
    options.LogoutPath = new PathString("/User/Logout");
    options.AccessDeniedPath = new PathString("/Home/AccessDenied");

    options.Cookie = new()
    {
        Name = "IdentityCookie",
        HttpOnly = true,
        SameSite = SameSiteMode.Lax,
        SecurePolicy = CookieSecurePolicy.Always
    };
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
});


builder.Services.AddAuthentication()

.AddGoogle(options =>
{
    options.ClientId = builder.Configuration.GetValue<string>("ExternalLoginProviders:Google:ClientId");
    options.ClientSecret = builder.Configuration.GetValue<string>("ExternalLoginProviders:Google:ClientSecret");
});
builder.Services.AddAuthorization(options =>
{

    options.AddPolicy("CheckPaymentExpiry", policy =>
       policy.Requirements.Add(new CheckPaymentExpiryRequirement()));
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=User}/{action=Login}/{id?}");

app.Run();
