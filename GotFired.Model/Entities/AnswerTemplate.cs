namespace GotFired.Model.Entities
{
    /// <summary>Reusable text staff insert into answers.</summary>
    public class AnswerTemplate : BaseEntity
    {
        private AnswerTemplate() { } // for EF Core

        public static AnswerTemplate Create(string name, string content) => new() { Name = name, Content = content };

        public string Name { get; private set; }
        public string Content { get; private set; }

        public void Edit(string name, string content)
        {
            Name = name;
            Content = content;
        }
    }
}
