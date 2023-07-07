using System.Drawing;

namespace BugTracker.Models.EntityModels
{
    /// <summary>
    /// Class <c>BugReportModel</c> models a bug report.
    /// </summary>
    public class BugReportModel
    {
        /// <value>
        /// Property <c>ID</c> represents the report's id.
        /// </value>
        public string ID { get; }
        /// <value>
        /// Property <c>ReporterID</c> represents the reporter's id.
        /// </value>
        public string? ReporterID { get; }
        /// <value>
        /// Property <c>ProjectID</c> represents the project id of the project that the report is for.
        /// </value>
        public string ProjectID { get; }
        /// <value>
        /// Property <c>Summary</c> represents the report's summary.
        /// </value>
        public string Summary { get; }
        /// <value>
        /// Property <c>SoftwareVersion</c> represents the software version that the bug occurred on.
        /// </value>
        public decimal SoftwareVersion { get; }
        /// <value>
        /// Property <c>Device</c> represents the device that the bug occurred on.
        /// </value>
        public string? Device { get; }
        /// <value>
        /// Property <c>OS</c> represents the operating system that the bug occured on.
        /// </value>
        public string OS { get; }
        /// <value>
        /// Property <c>Details</c> represents further details regarding the bug.
        /// </value>
        public string Details { get; }
        /// <value>
        /// Property <c>Priority</c> represents the priority level of the bug.
        /// </value>
        public string? Priority { get; }
        /// <value>
        /// Property <c>Severity</c> represents the severity level of the bug.
        /// </value>
        public string? Severity { get; }
        /// <value>
        /// Property <c>Status</c> represents the status of the bug.
        /// </value>
        public string? Status { get; }
		/// <value>
		/// Property <c>HelpWanted</c> represents whether the bug report was assigned a help-wanted label.
		/// </value>
		public bool HelpWanted { get; }
        /// <value>
        /// Property <c>Date</c> represents the date the bug report was made.
        /// </value>
        public DateTime Date { get; }
		/// <value>
		/// Property <c>Upvotes</c> represents the number of upvotes the report has.
		/// </value>
		public int Upvotes { get; }
        /// <value>
		/// Property <c>DisplayColor</c> represents the color to display the bug report in.
		/// </value>
        public string DisplayColor { get; }
        
        /// <summary>
        /// Method <c>BugReportModel</c> initializes this class with a bug report's data.
        /// </summary>
        /// <param name="reportID">The ID of the report.</param>
        /// <param name="reporterId">The ID of the user who reported the bug.</param>
        /// <param name="projectId">The ID of the project the report is for.</param>
        /// <param name="summary">The summary of the report.</param>
        /// <param name="softwareVersion">The software version of the project on which the bug occurred.</param>
        /// <param name="device">The device used when the bug occurred.</param>
        /// <param name="os">The operating system used when the bug occurred.</param>
        /// <param name="details">The details regarding the bug.</param>
        /// <param name="priority">The priority level of the report.</param>
        /// <param name="severity">The severity level of the report.</param>
        /// <param name="status">The status of the report.</param>
        /// <param name="helpWanted">Whether help is wanted for handling this report.</param>
        /// <param name="creationDate">The date the bug was reported.</param>
        public BugReportModel(string reportID, string? reporterId, string projectId, string summary, 
            decimal softwareVersion, string? device, string os, string details, 
            string? priority, string? severity, string? status, bool helpWanted, DateTime creationDate, int upvotes)
        {
            ID = reportID;
            ReporterID = reporterId;
            ProjectID = projectId;
            Summary = summary;
            SoftwareVersion = softwareVersion;
            Device = device;
            OS = os;
            Details = details;
            Priority = priority;
            Severity = severity;
            Status = status;
            HelpWanted = helpWanted;
            Date = creationDate;
            Upvotes = upvotes;
            DisplayColor = Utils.GenerateColor();
        }
    }
}
