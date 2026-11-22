using GotFired.Model;
using GotFired.Model.Entities;
using GotFired.Model.Entities.Enums;

namespace GotFired.Model.Tests;

public class DismissalCaseTests
{
    private static readonly DateTime Now = new(2026, 11, 19, 10, 0, 0, DateTimeKind.Utc);
    private static readonly TimeSpan NinetyDays = TimeSpan.FromDays(90);

    private static DismissalCase NewCase(params int[] supportedByIds) => DismissalCase.Submit(
        "guid-1",
        Applicant.Create("Ayşe Yılmaz", "ayse@example.com", null, Gender.Female, AgeInterval.TwentyFiveToThirtyFour, null, null),
        new CaseDetails { CityId = 1, DismissalState = DismissalState.Dismissed },
        supportedByIds);

    // ---- submission ------------------------------------------------------------------------------

    [Fact]
    public void A_submitted_case_is_pending_and_belongs_to_its_applicant()
    {
        var dismissalCase = NewCase();

        Assert.Equal(AppealState.Pending, dismissalCase.AppealState);
        Assert.Equal("Ayşe Yılmaz", dismissalCase.Applicant.FullName);
        Assert.Empty(dismissalCase.Comments);
        Assert.Null(dismissalCase.AccessTokenHash);   // no link until staff answer
    }

    [Fact]
    public void Each_supporter_is_recorded_once()
    {
        var dismissalCase = NewCase(1, 2, 1);

        Assert.Equal([1, 2], dismissalCase.Supporters.Select(s => s.SupportedById));
    }

    [Fact]
    public void A_case_needs_a_submission_id_and_an_applicant()
    {
        var applicant = Applicant.Create("x", null, null, Gender.Male, null, null, null);

        Assert.ThrowsAny<ArgumentException>(() => DismissalCase.Submit("", applicant, new CaseDetails(), []));
        Assert.Throws<ArgumentNullException>(() => DismissalCase.Submit("g", null, new CaseDetails(), []));
    }

    // ---- staff answer and the applicant link --------------------------------------------------------

    [Fact]
    public void A_staff_answer_is_recorded_marks_the_case_answered_and_issues_a_link()
    {
        var dismissalCase = NewCase();

        string link = dismissalCase.AnswerByStaff("<p>cevap</p>", "personel", Now, NinetyDays).LinkToken;

        Assert.Equal(AppealState.Answered, dismissalCase.AppealState);
        var comment = Assert.Single(dismissalCase.Comments);
        Assert.Equal(("<p>cevap</p>", "personel"), (comment.Text, comment.UserName));
        Assert.Equal(43, link.Length);
        Assert.True(dismissalCase.IsAccessibleWith(link, Now));
        Assert.Equal(Now + NinetyDays, dismissalCase.AccessTokenExpiresAt);
    }

    [Fact]
    public void A_new_answer_replaces_the_previous_link()
    {
        var dismissalCase = NewCase();
        string first = dismissalCase.AnswerByStaff("1", "personel", Now, NinetyDays).LinkToken;

        string second = dismissalCase.AnswerByStaff("2", "personel", Now, NinetyDays).LinkToken;

        Assert.False(dismissalCase.IsAccessibleWith(first, Now));
        Assert.True(dismissalCase.IsAccessibleWith(second, Now));
    }

    [Fact]
    public void The_link_works_until_it_expires()
    {
        var dismissalCase = NewCase();
        string link = dismissalCase.AnswerByStaff("cevap", "personel", Now, NinetyDays).LinkToken;

        Assert.True(dismissalCase.IsAccessibleWith(link, Now + NinetyDays - TimeSpan.FromSeconds(1)));
        Assert.False(dismissalCase.IsAccessibleWith(link, Now + NinetyDays));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("11111111-2222-3333-4444-555555555555")]   // the old case-Guid links
    [InlineData("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")]
    public void Anything_but_the_current_link_is_refused(string token)
    {
        var dismissalCase = NewCase();
        _ = dismissalCase.AnswerByStaff("cevap", "personel", Now, NinetyDays);

        Assert.False(dismissalCase.IsAccessibleWith(token, Now));
    }

    [Fact]
    public void A_case_without_an_answer_has_no_working_link()
    {
        Assert.False(NewCase().IsAccessibleWith("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", Now));
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(89, true)]
    [InlineData(90, false)]
    public void The_query_rule_and_the_in_memory_rule_agree(int daysLater, bool expected)
    {
        var dismissalCase = NewCase();
        string link = dismissalCase.AnswerByStaff("cevap", "personel", Now, NinetyDays).LinkToken;
        DateTime at = Now.AddDays(daysLater);

        bool inQuery = DismissalCase.AccessibleWith(ApplicantAccessToken.TryHash(link), at).Compile()(dismissalCase);

        Assert.Equal(expected, inQuery);
        Assert.Equal(expected, dismissalCase.IsAccessibleWith(link, at));
    }

    // ---- applicant reply --------------------------------------------------------------------------

    [Fact]
    public void An_applicant_reply_is_recorded_under_their_name_and_reopens_the_case()
    {
        var dismissalCase = NewCase();
        string link = dismissalCase.AnswerByStaff("cevap", "personel", Now, NinetyDays).LinkToken;

        var reply = dismissalCase.ReplyAsApplicant(link, "<p>soru</p>", Now.AddDays(1));

        Assert.Equal(AppealState.Pending, dismissalCase.AppealState);
        Assert.Equal(("<p>soru</p>", "Ayşe Yılmaz"), (reply.Text, reply.UserName));
        Assert.Equal(["personel", "Ayşe Yılmaz"], dismissalCase.Comments.Select(c => c.UserName));
    }

    [Fact]
    public void A_reply_needs_a_valid_unexpired_link()
    {
        var dismissalCase = NewCase();
        string link = dismissalCase.AnswerByStaff("cevap", "personel", Now, NinetyDays).LinkToken;

        Assert.Throws<BusinessRuleException>(() => dismissalCase.ReplyAsApplicant("wrong", "x", Now));
        Assert.Throws<BusinessRuleException>(() => dismissalCase.ReplyAsApplicant(link, "x", Now + NinetyDays));
        Assert.Equal(AppealState.Answered, dismissalCase.AppealState);
        Assert.Single(dismissalCase.Comments);
    }

    // ---- staff housekeeping -----------------------------------------------------------------------

    [Fact]
    public void Marking_as_read_answers_the_case()
    {
        var dismissalCase = NewCase();

        dismissalCase.MarkAnswered();

        Assert.Equal(AppealState.Answered, dismissalCase.AppealState);
    }

    [Fact]
    public void Notes_are_kept()
    {
        var dismissalCase = NewCase();

        dismissalCase.UpdateNotes("iç not");

        Assert.Equal("iç not", dismissalCase.Notes);
    }

    [Fact]
    public void Evaluating_sets_exactly_the_given_tags()
    {
        var dismissalCase = NewCase();
        dismissalCase.Evaluate([1, 2]);

        dismissalCase.Evaluate([2, 3, 3]);

        Assert.Equal([2, 3], dismissalCase.Tags.Select(t => t.TagID).Order());
    }

    [Fact]
    public void A_tag_is_on_a_case_at_most_once()
    {
        var dismissalCase = NewCase();

        dismissalCase.AddTag(5);
        dismissalCase.AddTag(5);

        Assert.Equal([5], dismissalCase.Tags.Select(t => t.TagID));
    }

    [Fact]
    public void Removing_a_tag_that_is_not_there_changes_nothing()
    {
        var dismissalCase = NewCase();
        dismissalCase.AddTag(5);

        dismissalCase.RemoveTag(7);
        dismissalCase.RemoveTag(5);

        Assert.Empty(dismissalCase.Tags);
    }
}

public class AnswerTemplateTests
{
    [Fact]
    public void A_template_can_be_created_and_edited()
    {
        var template = AnswerTemplate.Create("Karşılama", "<p>Merhaba</p>");

        template.Edit("Karşılama 2", "<p>Merhabalar</p>");

        Assert.Equal(("Karşılama 2", "<p>Merhabalar</p>"), (template.Name, template.Content));
    }
}
