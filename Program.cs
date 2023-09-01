using Auth0.AspNetCore.Authentication;
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

// Auth0 Management API services
var authCx = new Auth0UsrCx(
	builder.Configuration["Auth0:Domain"],
	builder.Configuration["Auth0:ClientId"],
	builder.Configuration["Auth0:ClientSecret"],
	builder.Configuration["Auth0:Audience"]);
builder.Services.Add(new ServiceDescriptor(typeof(UserManagementContext), authCx));

// Database services
var sqlCx = new MySQLDbCx(builder.Configuration.GetConnectionString("DefaultConnection"));
builder.Services.Add(new ServiceDescriptor(typeof(DatabaseContext), sqlCx));

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
