
namespace GotFired.Model.Entities.DismissalCase
{
    public class Comment: BaseEntity
    {
        public int DismissalCaseID { get; set; }
        public string Text { get; set; }
        public string UserName { get; set; }
    }
}
