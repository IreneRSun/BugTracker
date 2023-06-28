namespace BugTracker.Models.EntityModels
{
	/// <summary>
	/// Class <c>BugReportModel</c> models a comment made on a bug report.
	/// </summary>
	public class CommentModel
	{
		/// <value>
		/// Property <c>ID</c> represents the comment's id.
		/// </value>
		public string ID { get; }
		/// <value>
		/// Property <c>Commenter</c> represents the user that made the comment.
		/// </value>
		public UserModel? Commenter { get; }
		/// <value>
		/// Property <c>BugReportID</c> represents the id of the bug report the comment is for.
		/// </value>
		public string BugReportID { get; }
		/// <value>
		/// Property <c>Comment</c> represents the comment made.
		/// </value>
		public string Comment { get; }
		/// <value>
		/// Property <c>Date</c> represents the date the comment was made.
		/// </value>
		public DateTime Date { get; }

		/// <summary>
		/// Method <c>CommentModel</c> initializes this class with a comment's data.
		/// </summary>
		/// <param name="commentID">The ID of the comment.</param>
		/// <param name="commenter">The user that made the comment.</param>
		/// <param name="reportId">The ID of the bug report the comment is for.</param>
		/// <param name="comment">The string comment.</param>
		/// <param name="date">The date the comment was made.</param>
		public CommentModel(string commentID, UserModel? commenter, string reportId, string comment, DateTime date)
		{
			ID = commentID;
			Commenter = commenter;
			BugReportID = reportId;
			Comment = comment;
			Date = date;
		}
	}
}
