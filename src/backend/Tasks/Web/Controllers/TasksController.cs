using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tasks.Common;
using Tasks.Statistics;
using ZNetCS.AspNetCore.Authentication.Basic;

namespace Tasks.Web.Controllers
{
    [Route("tasks")]
    [Authorize(AuthenticationSchemes = BasicAuthenticationDefaults.AuthenticationScheme)]
    public class TasksController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            var content = System.IO.File.ReadAllText("Web/Views/Tasks.html");
            return Content(content, "text/html");
        }

        [HttpGet("get")]
        public IActionResult Load()
        {
            var result = StatisticsStore.GetWebInfo();
            return Ok(result);
        }

        [HttpPost("manual/{taskName}")]
        public IActionResult ManualRun([FromRoute] string taskName)
        {
            ManualQueue.AddTask(taskName);
            return Ok();
        }
    }
}
