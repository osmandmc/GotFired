using GotFired.Business.Services;
using GotFired.Model.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GotFired.Api.Controllers
{
    [ApiController]
    [Route("api/v1/answerTemplate")]
    public class AnswerTemplateController(IAnswerTemplateService templates) : ControllerBase
    {
        private const int PageSize = 20;

        [Authorize(Roles = "admin, user")]
        [HttpGet("list/{page}")]
        public IActionResult Index(int page = 1) =>
            Ok(new { data = templates.List(page, PageSize), paging = new Paging { CurrentPage = page, ItemsPerPage = PageSize } });

        [Authorize(Roles = "admin")]
        [HttpPost("create")]
        public IActionResult Create(AnswerTemplateCreateModel model)
        {
            templates.Create(model);
            return Ok();
        }

        [Authorize(Roles = "admin, user")]
        [HttpGet("getById/{id}")]
        public IActionResult Edit(int id) => Ok(templates.Get(id));

        [Authorize(Roles = "admin")]
        [HttpPost("update")]
        public IActionResult Edit(AnswerTemplateDto model)
        {
            templates.Update(model);
            return Ok();
        }
    }
}
