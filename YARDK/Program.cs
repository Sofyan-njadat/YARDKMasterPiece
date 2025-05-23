using Microsoft.EntityFrameworkCore;
using YARDK.Models;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyConnectionString")));

// Add session service with increased timeout and memory limit
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2); // Increased session timeout to 2 hours
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Configure session state to use more memory
builder.Services.AddDistributedMemoryCache(options =>
{
    options.SizeLimit = 100 * 1024 * 1024; // 100MB
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

app.UseAuthorization();

// Enable session
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
