using System.Reflection.Metadata;

namespace BugTracker.Models
{
    public class BugReportModel
    {
        private DatabaseContext context;

        public string ReportId { get; set; }

        public string ReporterId { get; set; }

        public string ProjectId { get; set; }

        public string Summary { get; set; }

        public float SoftwareVersion { get; set; }

        public string Device { get; set; }

        public string OS { get; set; }

        public string ExpectedResult { get; set; }

        public string ActualResult { get; set; }

        public string Steps { get; set; }

        public Blob Evidence { get; set; }

        public string Details { get; set; }

        public string Priority { get; set; }

        public int Severity { get; set; }

        public string Status { get; set; }

        public DateTime Date { get; set; }
    }
}
