using ExtECRMainLogic.Classes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Threading.Tasks;

namespace ExtECR.Filters
{
    public class VersionFilter : Attribute, IActionFilter, IAsyncActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            PersistManager persistManager = (PersistManager)context.HttpContext.RequestServices.GetService(typeof(PersistManager));
            Controller controller = (Controller)context.Controller;
            controller.ViewBag.AppVersion = persistManager.version;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            this.OnActionExecuting(context);
            var resultContext = await next();
            this.OnActionExecuted(resultContext);
        }
    }
}