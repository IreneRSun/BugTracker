using BugTracker.Models.DatabaseContexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BugTracker.Controllers
{
	public abstract class DatabaseAccessingController : Controller
	{
		[Authorize]
		protected DatabaseContext? GetDbCxt()
		{
			return HttpContext.RequestServices.GetService(typeof(DatabaseContext)) as DatabaseContext;
		}

		[Authorize]
		protected string? GetUserId()
		{
			return User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
		}
	}
}
