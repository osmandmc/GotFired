namespace GotFired.Model.ViewModels
{
    // API shapes for things that used to be returned (or bound) as database entities. Property names match the
    // JSON the existing front end reads, so the change is invisible to it; internal columns are no longer exposed.

    /// <summary>SGK termination reason: apply.html shows "Code - Name".</summary>
    public class CodedLookup
    {
        public int ID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
    }

    /// <summary>Answer template as read and edited by the SPA. RowVersion is sent back on update.</summary>
    public class AnswerTemplateDto
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
        public string RowVersion { get; set; }
    }

    public class AnswerTemplateCreateModel
    {
        public string Name { get; set; }
        public string Content { get; set; }
    }

    /// <summary>A stored comment as returned to userView.html after the applicant replies.</summary>
    public class CommentDto
    {
        public int ID { get; set; }
        public int DismissalCaseID { get; set; }
        public string Text { get; set; }
        public string UserName { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    /// <summary>Request body of addTag/removeTag.</summary>
    public class TagLinkModel
    {
        public int DismissalCaseID { get; set; }
        public int TagID { get; set; }
    }
}
