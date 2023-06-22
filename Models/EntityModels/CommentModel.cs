namespace BugTracker.Models.EntityModels
{
	public class CommentModel
	{
		public string ID { get; }
		public UserModel Commenter { get; set; }
		public string BugReport { get; set; }
		public string Comment { get; set; }
		public string Date { get; set; }

		public CommentModel(string commentID, string userId)
		{
			ID = commentID;
			Commenter = new UserModel(userId);
		}
	}
}
