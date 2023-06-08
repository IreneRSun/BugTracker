using BugTracker.Models.DatabaseContexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BugTracker.Controllers
{
	/// <summary>
	/// Class <c>DatabaseAccessingController</c> is an abstract controller that can 
	/// </summary>
	public abstract class DatabaseAccessingController : Controller
	{
		/// <summary>
		/// Method
		/// </summary>
		/// <returns></returns>
		[Authorize]
		protected DatabaseContext? GetDbCxt()
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
