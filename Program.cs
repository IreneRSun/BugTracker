using Auth0.AspNetCore.Authentication;
using BugTracker.Controllers;
using BugTracker.Models.DatabaseContexts;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container

// Controller services
builder.Services.AddControllersWithViews();

// Auth0 authentication/authorization services
builder.Services
    .AddAuth0WebAppAuthentication(options =>
    {
        options.Domain = builder.Configuration["Auth0:Domain"];
        options.ClientId = builder.Configuration["Auth0:ClientId"];
        options.Scope = "openid profile email";
    });

// Database services
var sqlDbCx = new MySQLDatabaseContext(builder.Configuration.GetConnectionString("DefaultConnection"));
var authDbCx = new Auth0ManagementContext(
    builder.Configuration["Auth0:Domain"],
    builder.Configuration["Auth0:ClientId"],
    builder.Configuration["Auth0:ClientSecret"],
    builder.Configuration["Auth0:Audience"]);

builder.Services.Add(new ServiceDescriptor(typeof(DatabaseContext), new DatabaseContext(sqlDbCx, authDbCx)));

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
