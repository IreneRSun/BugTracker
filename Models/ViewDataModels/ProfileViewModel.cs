using BugTracker.Models.EntityModels;

namespace BugTracker.Models.ViewDataModels
{
    public class ProfileViewModel
    {
        public UserModel User { get; }
        public bool IsCurrentUser { get; }

        public ProfileViewModel(UserModel user, bool isCurrentUser) {
            User = user;
            IsCurrentUser = isCurrentUser;
        }
    }
}
