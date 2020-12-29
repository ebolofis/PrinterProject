using ExtECRMainLogic.Classes;
using ExtECRMainLogic.Enumerators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using System;
using System.Threading.Tasks;

namespace ExtECR.Filters
{
    public class AuthorizationFilter : Attribute, IActionFilter, IAsyncActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            AuthorizationHelper authorizationHelper = (AuthorizationHelper)context.HttpContext.RequestServices.GetService(typeof(AuthorizationHelper));
            string password = GetPassword(context);

            AuthorizationEnum status = authorizationHelper.Authorize(password);
            if (status == AuthorizationEnum.Unknown)
            {
                context.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(
                        new
                        {
                            controller = "Home",
                            action = "Login"
                        }
                        ));
            }

            Controller controller = (Controller)context.Controller;
            controller.ViewBag.Auth = status.ToString();
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            this.OnActionExecuting(context);
            var resultContext = await next();
            this.OnActionExecuted(resultContext);
        }

        private string GetPassword(ActionExecutedContext context)
        {
            return context.HttpContext.Session.GetString("Pass");
        }
    }
}