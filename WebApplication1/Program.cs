using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Stripe;
using WebApplication1.DataAcces;
using WebApplication1.DataAccess.Repository;
using WebApplication1.DataAccess.Repository.IRepository;
using WebApplication1.Models;
using WebApplication1.Utility;
using static System.Net.Mime.MediaTypeNames;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(Options => Options.UseSqlServer(
    builder.Configuration.GetConnectionString("DefaultConnection")));
//Add stripesetting 
//bind strip setting with application
builder.Services.Configure<StripeSetting>(builder.Configuration.GetSection("Stripe"));
builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddDefaultTokenProviders()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddSingleton<IEmailSender, EmailSender>();
builder.Services.AddRazorPages();
// path when we click on add to cart button then do not open page so use this to redirect to login/logout and signup pages
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = $"/Identity/Account/Login";
    options.LogoutPath = $"/Identity/Account/Logout";
    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
});
//use for adding session into container  
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession( options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(100);
        options.Cookie.HttpOnly=true;
        options.Cookie.IsEssential=true;
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
//add stripe in pipeline
//add global api key in pipeline so this work from program.cs to appsetting and then  webAppication1. utility and get string
StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe:SecretKey").Get<string>();
app.UseAuthentication();
app.UseAuthorization();
//build in session has been configured (only support string and integer) but we can add custom objects in session
app.UseSession();
app.MapRazorPages(); //routing for pages
app.MapControllerRoute( //routing for controller
    name: "default",
    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");

app.Run();
