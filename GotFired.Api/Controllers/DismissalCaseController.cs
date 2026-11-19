using GotFired.Business.Email;
using GotFired.Business.Services;
using GotFired.Model.Entities.Enums;
using GotFired.Model.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GotFired.Api.Controllers
{
    [ApiController]
    [Route("api/v1/dismissalcase")]
    [Authorize(Roles = "admin,editor,user")]
    public class DismissalCaseController(ICaseService cases, IAnswerTemplateService templates, IEmailSender email) : ControllerBase
    {
        private const int PageSize = 20;

        // ---- Public application form (apply.html) ----------------------------------------------------

        [AllowAnonymous]
        [HttpGet("apply")]
        public IActionResult Apply()
        {
            try
            {
                var lookups = cases.GetApplicationLookups();
                return Ok(new
                {
                    guid = cases.NewApplicationGuid(),
                    cities = lookups.Cities,
                    declaredTerminationReasons = lookups.DeclaredTerminationReasons,
                    supportedBys = lookups.SupportedBys,
                    ageIntervals = EnumHelper.GetEnumLookups<AgeInterval>(),
                    educationStates = EnumHelper.GetEnumLookups<EducationState>(),
                    dismissalStates = EnumHelper.GetEnumLookups<DismissalState>(),
                    genders = EnumHelper.GetEnumLookups<Gender>(),
                    employmentDurations = EnumHelper.GetEnumLookups<EmploymentDurationSince>(),
                    employeeCounts = EnumHelper.GetEnumLookups<EmployeeCount>(),
                    sgkTerminationReasons = lookups.SgkTerminationReasons
                });
            }
            catch (Exception)
            {
                return new JsonResult("Hatayı bulamadım");
            }
        }

        // apply.html posts a jQuery-serialized form.
        [AllowAnonymous]
        [HttpPost("apply")]
        public IActionResult Apply([FromForm] DismissalCaseApplyModel model)
        {
            if (!ModelState.IsValid)
            {
                // Web API 2 shape; apply.html alerts the ModelState messages.
                return BadRequest(new
                {
                    Message = "The request is invalid.",
                    ModelState = ModelState.Where(e => e.Value.Errors.Count > 0)
                        .ToDictionary(e => e.Key, e => e.Value.Errors.Select(x => x.ErrorMessage).ToArray())
                });
            }
            cases.SubmitApplication(model);
            return Ok();
        }

        // ---- Staff (admin SPA) ------------------------------------------------------------------------

        [HttpGet("index/{page}")]
        public IActionResult Index(int page = 1) => Page(page, null);

        [HttpGet("pending/{page}")]
        public IActionResult Pending(int page = 1) => Page(page, AppealState.Pending);

        [HttpGet("answered/{page}")]
        public IActionResult Answered(int page = 1) => Page(page, AppealState.Answered);

        [HttpGet("view/{id}")]
        public IActionResult View(int id)
        {
            var model = cases.GetCase(id);
            return model == null ? NotFoundMessage() : Ok(model);
        }

        [HttpPost("answer")]
        public IActionResult Answer(CommentViewModel commentViewModel)
        {
            // Recipient, link and author come from the case record and the signed-in user, not the request body.
            var notification = cases.Answer(commentViewModel.DismissalCaseId, commentViewModel.Text, User.Identity?.Name ?? commentViewModel.UserName);
            if (!string.IsNullOrWhiteSpace(notification.Email))
            {
                email.SendApplicantLink(notification.Email, notification.Token);
            }
            return Ok();
        }

        [HttpPost("postNotes")]
        public IActionResult PostNotes(NoteModel noteModel)
        {
            cases.SaveNotes(noteModel);
            return Ok();
        }

        [HttpPost("makeRead")]
        public IActionResult MakeRead(DismissalCaseId dismissalCaseId)
        {
            cases.MarkAnswered(dismissalCaseId.Id);
            return Ok();
        }

        [HttpPost("evaluate")]
        public IActionResult Evaluate(EvaluationViewModel model)
        {
            cases.Evaluate(model);
            return Ok();
        }

        [HttpPost("addTag")]
        public IActionResult AddDismissalCaseTag(TagLinkModel model)
        {
            cases.AddTag(model);
            return Ok();
        }

        [HttpPost("removeTag")]
        public IActionResult RemoveDismissalCaseTag(TagLinkModel model)
        {
            cases.RemoveTag(model);
            return Ok();
        }

        [HttpGet("getCategories")]
        public IActionResult GetCategories() => Ok(cases.GetCategories());

        [HttpGet("getTags")]
        public IActionResult GetTags() => Ok(cases.GetTags());

        [HttpGet("getTags/{query}")]
        public IActionResult GetTags(string query) => Ok(cases.GetTags(query));

        [HttpGet("getAnswerTemplates")]
        public IActionResult GetAnswerTemplates() => Ok(templates.Lookup());

        [HttpGet("getAnswerTemplateContent/{id}")]
        public IActionResult GetAnswerTemplateContent(int id) => Ok(templates.Get(id));

        // ---- Applicant (userView.html, secret link token) -----------------------------------------------

        // The parameter is named "guid" because userView.html builds the URL from ?guid=; it carries the
        // applicant's secret link token, not the case Guid.
        [AllowAnonymous]
        [HttpGet("userview/{guid}")]
        public IActionResult UserView(string guid)
        {
            Response.Headers.CacheControl = "no-store";
            var model = cases.GetApplicantView(guid);
            return model == null ? NotFoundMessage() : Ok(model);
        }

        // userView.html posts a jQuery-serialized form; Guid carries the link token. The case ID and name in the
        // form are ignored.
        [AllowAnonymous]
        [HttpPost("useranswer")]
        public IActionResult UserAnswer([FromForm] CommentViewModel commentViewModel)
        {
            Response.Headers.CacheControl = "no-store";
            var comment = cases.AddApplicantReply(commentViewModel.Guid, commentViewModel.Text);
            return comment == null ? NotFoundMessage() : Ok(comment);
        }

        private IActionResult Page(int page, AppealState? state)
        {
            var (list, total) = cases.ListCases(page, PageSize, state);
            return Ok(new { data = list, paging = new Paging { CurrentPage = page, ItemsPerPage = PageSize, TotalItems = total } });
        }

        // Web API 2 returned BadRequest("...") as {"Message": "..."}; the pages expect 400 for "not found".
        private BadRequestObjectResult NotFoundMessage() => BadRequest(new { Message = "model cannot be null" });
    }
}
