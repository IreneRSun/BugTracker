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
        public string? ReporterID { get; set; }
        /// <value>
        /// Property <c>ProjectID</c> represents the project id of the project that the report is for.
        /// </value>
        public string? ProjectID { get; set; }
        /// <value>
        /// Property <c>Summary</c> represents the report's summary.
        /// </value>
        public string? Summary { get; set; }
        /// <value>
        /// Property <c>SoftwareVersion</c> represents the software version that the bug occurred on.
        /// </value>
        public decimal? SoftwareVersion { get; set; }
        /// <value>
        /// Property <c>Device</c> represents the device that the bug occurred on.
        /// </value>
        public string? Device { get; set; }
        /// <value>
        /// Property <c>OS</c> represents the operating system that the bug occured on.
        /// </value>
        public string? OS { get; set; }
        /// <value>
        /// Property <c>ExpectedResult</c> represents the result the user expected when using the project.
        /// </value>
        public string? ExpectedResult { get; set; }
        /// <value>
        /// Property <c>ActualResult</c> represents the result that actually occurred when the user was using the project.
        /// </value>
        public string? ActualResult { get; set; }
        /// <value>
        /// Property <c>Steps</c> represents the steps to recreate the bug.
        /// </value>
        public string? Steps { get; set; }
        /// <value>
        /// Property <c>Details</c> represents further details regarding the bug.
        /// </value>
        public string? Details { get; set; }
        /// <value>
        /// Property <c>Priority</c> represents the priority level of the bug.
        /// </value>
        public string? Priority { get; set; }
        /// <value>
        /// Property <c>Severity</c> represents the severity level of the bug.
        /// </value>
        public int? Severity { get; set; }
        /// <value>
        /// Property <c>Status</c> represents the status of the bug.
        /// </value>
        public string? Status { get; set; }
        /// <value>
        /// Property <c>Date</c> represents the date the bug report was made.
        /// </value>
        public DateTime? Date { get; set; }

        /// <summary>
        /// Method <c>BugReportModel</c> initializes this class with an ID.
        /// </summary>
        /// <param name="reportID">The ID of the bug report.</param>
        public BugReportModel(string reportID)
        {
            ID = reportID;
        }
    }
}
