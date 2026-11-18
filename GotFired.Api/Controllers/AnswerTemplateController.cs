using GotFired.Business;
using GotFired.Model.Entities.DismissalCase;
using GotFired.Model.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GotFired.Api.Controllers
{
    [ApiController]
    [Route("api/v1/answerTemplate")]
    public class AnswerTemplateController : ControllerBase
    {
        private readonly ParameterBusiness _parameterBusiness;
        private const int pageSize = 20;

        public AnswerTemplateController(ParameterBusiness parameterBusiness)
        {
            _parameterBusiness = parameterBusiness;
        }

        [Authorize(Roles = "admin, user")]
        [HttpGet("list/{page}")]
        public IActionResult Index(int page = 1)
        {
            var answerTemplates = _parameterBusiness.GetAnswerTemplateListModel(page, pageSize);
            return Ok(new
            {
                data = answerTemplates,
                paging = new Paging
                {
                    CurrentPage = page,
                    ItemsPerPage = pageSize,
                }
            });
        }

        [Authorize(Roles = "admin")]
        [HttpPost("create")]
        public IActionResult Create(AnswerTemplate answerTemplate)
        {
            _parameterBusiness.CreateAnswerTemplate(answerTemplate);
            return Ok();
        }

        [Authorize(Roles = "admin, user")]
        [HttpGet("getById/{id}")]
        public IActionResult Edit(int id)
        {
            var answerTemplate = _parameterBusiness.GetAnswerTemplate(id);
            return Ok(answerTemplate);
        }

        [Authorize(Roles = "admin")]
        [HttpPost("update")]
        public IActionResult Edit(AnswerTemplate answerTemplate)
        {
            _parameterBusiness.UpdateAnswerTemplate(answerTemplate);
            return Ok();
        }
    }
}
