using GotFired.Api.Filter;
using GotFired.Api.Handlers;
using GotFired.Api.Helper;
using GotFired.Business;
using GotFired.Model.Entities.DismissalCase;
using GotFired.Model.Entities.Enums;
using GotFired.Model.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace GotFired.Api.Controllers
{
    [CustomExceptionFilter]
    [RoutePrefix("api/v1/dismissalcase")]
    //[EnableCors("http://localhost:54483", "*", "*")]
    [Authorize(Roles = "admin,editor,user")]
    public class DismissalCaseController : ApiController
    {
        const short pageSize = 20;
        //IDismissalCaseBusiness _dismissalCaseBusiness;
        readonly DismissalCaseBusiness _dismissalCaseBusiness;

        public DismissalCaseController()
        {
            _dismissalCaseBusiness = new DismissalCaseBusiness();
        }


        [AllowAnonymous]
        [HttpGet, Route("apply")]
        public IHttpActionResult Apply()
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
            catch (Exception ex)
            {
                return Json("Hatayı bulamadım");
            }
        }

        [AllowAnonymous]
        [HttpPost, Route("apply")]
        public IHttpActionResult Apply(DismissalCaseApplyModel model)
        {
            if (model == null)
            {
                return BadRequest("model cannot be null");
            }
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _dismissalCaseBusiness.AddDismissalCase(model);
            return Ok();
        }

        [HttpGet, Route("index/{page}")]
        public IHttpActionResult Index(int page = 1)
        {
            var dismissalCaseListModel = _dismissalCaseBusiness.GetDismissalCaseListModel(page, pageSize);
            if (dismissalCaseListModel == null)
            {
                return BadRequest("model cannot be null");
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

        [HttpGet, Route("pending/{page}")]
        public IHttpActionResult Pending(int page = 1)
        {
            var dismissalCaseListModel = _dismissalCaseBusiness.GetDismissalCaseListModelByStatus(AppealState.Pending, page, pageSize);
            if (dismissalCaseListModel == null)
            {
                return BadRequest("model cannot be null");
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

        [HttpGet, Route("answered/{page}")]
        public IHttpActionResult Answered(int page = 1)
        {
            var dismissalCaseListModel = _dismissalCaseBusiness.GetDismissalCaseListModelByStatus(AppealState.Answered, page, pageSize);
            if (dismissalCaseListModel == null)
            {
                return BadRequest("model cannot be null");
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

        [HttpGet, Route("view/{id}")]
        public IHttpActionResult View(int id)
        {
            var dismissalCaseViewModel = _dismissalCaseBusiness.GetDismissalCaseById(id);
            if (dismissalCaseViewModel == null)
            {
                return BadRequest("model cannot be null");
            }
            return Ok(dismissalCaseViewModel);
        }
        [AllowAnonymous]
        [HttpGet, Route("userview/{guid}")]
        public IHttpActionResult UserView(string guid)
        {
            var dismissalCase = _dismissalCaseBusiness.GetDismissalCaseByGuid(guid);
            if (dismissalCase == null)
            {
                return BadRequest("model cannot be null");
            }
            return Ok(dismissalCase);
        }

        [HttpPost, Route("answer")]
        public IHttpActionResult Answer(CommentViewModel commentViewModel)
        {
            var smtpManager = new SmtpManager();
            smtpManager.SendEmail(commentViewModel.EmailAddress, commentViewModel.Guid);
            _dismissalCaseBusiness.UpdateDismissalCaseByUserAnswer(commentViewModel);
            return Ok();
        }
        [HttpPost, Route("postNotes")]
        public IHttpActionResult PostNotes(NoteModel noteModel)
        {
            _dismissalCaseBusiness.UpdateDismissalCaseByNotes(noteModel);
            return Ok();
        }
        [HttpPost, Route("makeRead")]
        public IHttpActionResult MakeRead(DismissalCaseId dismissalCaseId)
        {
            _dismissalCaseBusiness.UpdateDismissalCaseByAppealState(dismissalCaseId.Id);
            return Ok();
        }
        [AllowAnonymous]
        [HttpPost, Route("useranswer")]
        public IHttpActionResult UserAnswer(CommentViewModel commentViewModel)
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

        [HttpGet, Route("getCategories")]
        public IHttpActionResult GetCategories()
        {
            var categories = _dismissalCaseBusiness.GetCategories();
            return Ok(categories);
        }
        [HttpGet, Route("getTags")]
        public IHttpActionResult GetTags()
        {
            var categories = _dismissalCaseBusiness.GetTags();
            return Ok(categories);
        }
        [HttpGet, Route("getTags/{query}")]
        public IHttpActionResult GetTags(string query)
        {
            var categories = _dismissalCaseBusiness.GetTags(query);
            return Ok(categories);
        }
        [HttpGet, Route("getAnswerTemplates")]
        public IHttpActionResult GetAnswerTemplates()
        {
            var answerTemplates = _dismissalCaseBusiness.GetAnswerTemplatesLookup();
            if (answerTemplates == null)
            {
                return BadRequest("model cannot be null");
            }
            return Ok(answerTemplates);
        }
        [HttpGet, Route("getAnswerTemplateContent/{id}")]
        public IHttpActionResult GetAnswerTemplateContent(int id)
        {
            var answerTemplateContent = _dismissalCaseBusiness.GetAnswerTemplateContent(id);
            return Ok(answerTemplateContent);
        }
        [HttpPost, Route("evaluate")]
        public IHttpActionResult Evaluate(EvaluationViewModel model)
        {
            _dismissalCaseBusiness.Evaluate(model);
            return Ok();
        }

        [HttpPost, Route("addTag")]
        public IHttpActionResult AddDismissalCaseTag(DismissalCaseTag model)
        {
            _dismissalCaseBusiness.AddDismissalCaseTag(model);
            return Ok();
        }

        [HttpPost, Route("removeTag")]
        public IHttpActionResult RemoveDismissalCaseTag(DismissalCaseTag model)
        {
            _dismissalCaseBusiness.RemoveDismissalCaseTag(model);
            return Ok();
        }
    }
}
