using GotFired.Business;
using GotFired.Model.Entities.DismissalCase;
using GotFired.Model.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace GotFired.Api.Controllers
{
    [RoutePrefix("api/v1/answerTemplate")]
    
    public class AnswerTemplateController : ApiController
    {
        readonly ParameterBusiness _parameterBusiness;
        const int pageSize = 20;
        public AnswerTemplateController()
        {
            _parameterBusiness = new ParameterBusiness();
        }
        [Authorize(Roles = "admin, user")]
        [HttpGet, Route("list/{page}")]
        public IHttpActionResult Index(int page = 1)
        {
            var answerTemplates = _parameterBusiness.GetAnswerTemplateListModel(page, pageSize);
            return Ok(new
            {
                data = answerTemplates,
                paging = new Paging
                {
                    CurrentPage = page,
                    ItemsPerPage = pageSize,
                    //TotalItems = count
                }
            });
        }
        [Authorize(Roles = "admin")]
        [HttpPost, Route("create")]
        public IHttpActionResult Create(AnswerTemplate answerTemplate)
        {
            _parameterBusiness.CreateAnswerTemplate(answerTemplate);
            return Ok();
        }
        [Authorize(Roles = "admin, user")]
        [HttpGet, Route("getById/{id}")]
        public IHttpActionResult Edit(int id)
        {
            var answerTemplate = _parameterBusiness.GetAnswerTemplate(id);
            return Ok(answerTemplate);
        }
        [Authorize(Roles = "admin")]
        [HttpPost, Route("update")]
        public IHttpActionResult Edit(AnswerTemplate answerTemplate)
        {
            _parameterBusiness.UpdateAnswerTemplate(answerTemplate);
            return Ok();
        }

    }
}
