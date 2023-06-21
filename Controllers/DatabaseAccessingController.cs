using BugTracker.Models.DatabaseContexts;
using BugTracker.Models.EntityModels;
using BugTracker.Models.ViewDataModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BugTracker.Controllers
{
	/// <summary>
	/// Class <c>DatabaseAccessingController</c> is an abstract controller that contains 
	/// methods to access the application's database contexts and shared database methods.
	/// </summary>
	public abstract class DatabaseAccessingController : Controller
	{
		/// <summary>
		/// Method <c>GetUserManagementCx</c> requests the service that manages users.
		/// </summary>
		/// <returns>The context for managing users.</returns>
		[Authorize]
		protected UserManagementContext? GetUserManagementCx()
		{
			return HttpContext.RequestServices.GetService(typeof(UserManagementContext)) as UserManagementContext;
        }

		/// <summary>
		/// Method <c>GetDbCx</c> requests the service that manages the database.
		/// </summary>
		/// <returns>The context for managing the database.</returns>
		[Authorize]
		protected DatabaseContext? GetDbCx()
		{
			return HttpContext.RequestServices.GetService(typeof(DatabaseContext)) as DatabaseContext;
        }

		/// <summary>
		/// Method <c>GetUserId</c> gets the ID of the current authenticated & authorized user.
		/// </summary>
		/// <returns>The current user's ID.</returns>
		[Authorize]
		protected string? GetUserId()
		{
			return User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
		}
	}
}
