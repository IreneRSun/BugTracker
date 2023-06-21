using Microsoft.AspNetCore.Diagnostics;
using System.Diagnostics;

namespace BugTracker.Models.ViewDataModels
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
	}
}