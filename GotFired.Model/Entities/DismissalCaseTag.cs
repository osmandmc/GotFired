namespace GotFired.Model.Entities
{
    /// <summary>A tag on a case. Created only by <see cref="DismissalCase"/>.</summary>
    public class DismissalCaseTag : BaseEntity
    {
        private DismissalCaseTag() { } // for EF Core

        internal DismissalCaseTag(int tagId) => TagID = tagId;

        public int DismissalCaseID { get; private set; }
        public int TagID { get; private set; }
        public DismissalCase DismissalCase { get; private set; }
        public Tag Tag { get; private set; }
    }
}
