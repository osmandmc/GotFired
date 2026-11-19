namespace GotFired.Model.Entities
{
    /// <summary>Who the applicant says supported them (union, lawyer, ...). Created only by <see cref="DismissalCase"/>.</summary>
    public class DismissalCaseSupportedBy : BaseEntity
    {
        private DismissalCaseSupportedBy() { } // for EF Core

        internal DismissalCaseSupportedBy(int supportedById) => SupportedById = supportedById;

        public int DismissalCaseId { get; private set; }
        public int SupportedById { get; private set; }
        public SupportedBy SupportedBy { get; private set; }
        public DismissalCase DismissalCase { get; private set; }
    }
}
