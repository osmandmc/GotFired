namespace GotFired.Model.Entities
{
    /// <summary>A message in a case's conversation, by staff or the applicant. Created only by <see cref="DismissalCase"/>.</summary>
    public class Comment : BaseEntity
    {
        private Comment() { } // for EF Core

        internal Comment(string text, string userName)
        {
            Text = text;
            UserName = userName;
        }

        public int DismissalCaseID { get; private set; }
        public string Text { get; private set; }
        /// <summary>The author's display name: the staff user name or the applicant's full name.</summary>
        public string UserName { get; private set; }
    }
}
