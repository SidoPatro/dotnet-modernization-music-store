using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MvcMusicStore.Models;
using System;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// Register MVC services
builder.Services.AddControllersWithViews();

// Register distributed memory cache and session (replaces legacy Session/FormsAuthentication)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Register MusicStore EF Core DbContext with SQL Server
builder.Services.AddDbContext<MusicStoreEntities>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MusicStoreEntities")));

// Register ASP.NET Core Identity with an in-memory EF Core DbContext
// (Replaces legacy System.Web.Security Membership / FormsAuthentication)
builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseInMemoryDatabase("MvcMusicStoreIdentity"));

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<IdentityDbContext>()
.AddDefaultTokenProviders();

// Register static configuration manager for legacy controller access to IConfiguration
MvcMusicStore.ConfigurationManager.Configuration = builder.Configuration;

var app = builder.Build();

// Seed the database (replaces EF6 Database.SetInitializer / DropCreateDatabaseIfModelChanges)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MusicStoreEntities>();
    db.Database.EnsureCreated();
    if (!db.Genres.Any())
    {
        SampleData.Seed(db);
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    // Replaces RegisterGlobalFilters HandleErrorAttribute global filter
    app.UseExceptionHandler("/Home/Error");
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Session must be configured before Authentication/Authorization and routing endpoints
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// Migrated from RegisterRoutes: replaces routes.MapRoute("Default", ...)
// routes.IgnoreRoute("{resource}.axd/{*pathInfo}") removed - .axd routes not relevant in .NET 8.0
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
