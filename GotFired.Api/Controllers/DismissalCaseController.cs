
using GotFired.Business;
using GotFired.Model.Entities.DismissalCase;
using GotFired.Model.Entities.Enums;
using GotFired.Model.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GotFired.Api.Controllers
{
    [ApiController]
    [Route("api/v1/dismissalcase")]
    //[EnableCors("http://localhost:54483", "*", "*")]
    [Authorize(Roles = "admin,editor,user")]
    public class DismissalCaseController : ControllerBase
    {
        const short pageSize = 20;
        //IDismissalCaseBusiness _dismissalCaseBusiness;
        readonly DismissalCaseBusiness _dismissalCaseBusiness;
        readonly SmtpManager _smtpManager;

        public DismissalCaseController(DismissalCaseBusiness dismissalCaseBusiness, SmtpManager smtpManager)
        {
            _dismissalCaseBusiness = dismissalCaseBusiness;
            _smtpManager = smtpManager;
        }


        [AllowAnonymous]
        [HttpGet("apply")]
        public IActionResult Apply()
        {
            try
            {
                string guid = _dismissalCaseBusiness.GetInitializer();
                var cities = _dismissalCaseBusiness.GetCities();
                var declaredTerminationReasons = _dismissalCaseBusiness.GetDeclaredTerminationReasons();
                var supportedBys = _dismissalCaseBusiness.GetSupportedBys();
                var sgkTerminationReasons = _dismissalCaseBusiness.GetSGKTerminationReasons();
                var ageIntervals = EnumHelper.GetEnumLookups<AgeInterval>();
                var educationStates = EnumHelper.GetEnumLookups<EducationState>();
                var genders = EnumHelper.GetEnumLookups<Gender>();
                var dismissalStates = EnumHelper.GetEnumLookups<DismissalState>();
                var employmentDurations = EnumHelper.GetEnumLookups<EmploymentDurationSince>();
                var employeeCounts = EnumHelper.GetEnumLookups<EmployeeCount>();
                if (guid == null)
                {
                    return NotFound();
                }
                return Ok(
                    new
                    {
                        guid,
                        cities,
                        declaredTerminationReasons,
                        supportedBys,
                        ageIntervals,
                        educationStates,
                        dismissalStates,
                        genders,
                        employmentDurations,
                        employeeCounts,
                        sgkTerminationReasons
                    });
            }
            catch (Exception)
            {
                return new JsonResult("Hatayı bulamadım");
            }
        }

        [AllowAnonymous]
        [HttpPost("apply")]
        public IActionResult Apply([FromForm] DismissalCaseApplyModel model)
        {
            if (model == null)
            {
                return BadRequest(new { Message = "model cannot be null" });
            }
            if (!ModelState.IsValid)
                return BadRequest(new
                {
                    Message = "The request is invalid.",
                    ModelState = ModelState.Where(e => e.Value.Errors.Count > 0)
                        .ToDictionary(e => e.Key, e => e.Value.Errors.Select(x => x.ErrorMessage).ToArray())
                });

            _dismissalCaseBusiness.AddDismissalCase(model);
            return Ok();
        }

        [HttpGet("index/{page}")]
        public IActionResult Index(int page = 1)
        {
            var dismissalCaseListModel = _dismissalCaseBusiness.GetDismissalCaseListModel(page, pageSize);
            if (dismissalCaseListModel == null)
            {
                return BadRequest(new { Message = "model cannot be null" });
            }
            int count = _dismissalCaseBusiness.GetAllDismissalCasesCount();
            return Ok(new
            {
                data = dismissalCaseListModel,
                paging = new Paging
                {
                    CurrentPage = page,
                    ItemsPerPage = pageSize,
                    TotalItems = count
                }
            });
        }

        [HttpGet("pending/{page}")]
        public IActionResult Pending(int page = 1)
        {
            var dismissalCaseListModel = _dismissalCaseBusiness.GetDismissalCaseListModelByStatus(AppealState.Pending, page, pageSize);
            if (dismissalCaseListModel == null)
            {
                return BadRequest(new { Message = "model cannot be null" });
            }
            int count = _dismissalCaseBusiness.GetAllDismissalCasesCountByStatus(AppealState.Pending);
            return Ok(new
            {
                data = dismissalCaseListModel,
                paging = new Paging
                {
                    CurrentPage = page,
                    ItemsPerPage = pageSize,
                    TotalItems = count
                }
            });
        }

        [HttpGet("answered/{page}")]
        public IActionResult Answered(int page = 1)
        {
            var dismissalCaseListModel = _dismissalCaseBusiness.GetDismissalCaseListModelByStatus(AppealState.Answered, page, pageSize);
            if (dismissalCaseListModel == null)
            {
                return BadRequest(new { Message = "model cannot be null" });
            }
            int count = _dismissalCaseBusiness.GetAllDismissalCasesCountByStatus(AppealState.Answered);
            return Ok(new
            {
                data = dismissalCaseListModel,
                paging = new Paging
                {
                    CurrentPage = page,
                    ItemsPerPage = pageSize,
                    TotalItems = count
                }
            });
        }

        [HttpGet("view/{id}")]
        public IActionResult View(int id)
        {
            var dismissalCaseViewModel = _dismissalCaseBusiness.GetDismissalCaseById(id);
            if (dismissalCaseViewModel == null)
            {
                return BadRequest(new { Message = "model cannot be null" });
            }
            return Ok(dismissalCaseViewModel);
        }
        [AllowAnonymous]
        [HttpGet("userview/{guid}")]
        public IActionResult UserView(string guid)
        {
            var dismissalCase = _dismissalCaseBusiness.GetDismissalCaseByGuid(guid);
            if (dismissalCase == null)
            {
                return BadRequest(new { Message = "model cannot be null" });
            }
            return Ok(dismissalCase);
        }

        [HttpPost("answer")]
        public IActionResult Answer(CommentViewModel commentViewModel)
        {
            _smtpManager.SendEmail(commentViewModel.EmailAddress, commentViewModel.Guid);
            _dismissalCaseBusiness.UpdateDismissalCaseByUserAnswer(commentViewModel);
            return Ok();
        }
        [HttpPost("postNotes")]
        public IActionResult PostNotes(NoteModel noteModel)
        {
            _dismissalCaseBusiness.UpdateDismissalCaseByNotes(noteModel);
            return Ok();
        }
        [HttpPost("makeRead")]
        public IActionResult MakeRead(DismissalCaseId dismissalCaseId)
        {
            _dismissalCaseBusiness.UpdateDismissalCaseByAppealState(dismissalCaseId.Id);
            return Ok();
        }
        [AllowAnonymous]
        [HttpPost("useranswer")]
        public IActionResult UserAnswer([FromForm] CommentViewModel commentViewModel)
        {
            Comment comment = new Comment
            {
                DismissalCaseID = commentViewModel.DismissalCaseId,
                Text = commentViewModel.Text,
                UserName = commentViewModel.UserName,
                CreatedDate = commentViewModel.CreatedDate
            };
            //_dismissalCaseBusiness.AddComment(comment);
            _dismissalCaseBusiness.UpdateDismissalCaseByApplicantAnswer(comment, commentViewModel.Guid);
            return Ok(comment);
        }

        [HttpGet("getCategories")]
        public IActionResult GetCategories()
        {
            var categories = _dismissalCaseBusiness.GetCategories();
            return Ok(categories);
        }
        [HttpGet("getTags")]
        public IActionResult GetTags()
        {
            var categories = _dismissalCaseBusiness.GetTags();
            return Ok(categories);
        }
        [HttpGet("getTags/{query}")]
        public IActionResult GetTags(string query)
        {
            var categories = _dismissalCaseBusiness.GetTags(query);
            return Ok(categories);
        }
        [HttpGet("getAnswerTemplates")]
        public IActionResult GetAnswerTemplates()
        {
            var answerTemplates = _dismissalCaseBusiness.GetAnswerTemplatesLookup();
            if (answerTemplates == null)
            {
                return BadRequest(new { Message = "model cannot be null" });
            }
            return Ok(answerTemplates);
        }
        [HttpGet("getAnswerTemplateContent/{id}")]
        public IActionResult GetAnswerTemplateContent(int id)
        {
            var answerTemplateContent = _dismissalCaseBusiness.GetAnswerTemplateContent(id);
            return Ok(answerTemplateContent);
        }
        [HttpPost("evaluate")]
        public IActionResult Evaluate(EvaluationViewModel model)
        {
            _dismissalCaseBusiness.Evaluate(model);
            return Ok();
        }

        [HttpPost("addTag")]
        public IActionResult AddDismissalCaseTag(DismissalCaseTag model)
        {
            _dismissalCaseBusiness.AddDismissalCaseTag(model);
            return Ok();
        }

        [HttpPost("removeTag")]
        public IActionResult RemoveDismissalCaseTag(DismissalCaseTag model)
        {
            _dismissalCaseBusiness.RemoveDismissalCaseTag(model);
            return Ok();
        }
    }
}
