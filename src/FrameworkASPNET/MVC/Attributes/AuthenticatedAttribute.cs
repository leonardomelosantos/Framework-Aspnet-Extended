using FrameworkAspNetExtended.MVC.Controllers;
using System.Web.Mvc;
using System.Web.Mvc.Filters;

namespace FrameworkAspNetExtended.MVC.Attributes
{
    public class AuthenticatedAttribute : ActionFilterAttribute, IAuthenticationFilter
    {
        public string ControllerWhenUserNotAuthenticated { get; set; }
        public string ActionWhenUserNotAuthenticated { get; set; }
        public string UrlWhenUserNotAuthenticated { get; set; }

        public void OnAuthentication(AuthenticationContext filterContext)
        {
            ExecutarVerificacao(filterContext);
        }

        private void ExecutarVerificacao(AuthenticationContext filterContext)
        {
            if (filterContext.Controller is ExtendedController)
            {
                ExtendedController extendedController = filterContext.Controller as ExtendedController;

                bool isAuthenticated = extendedController.IsAuthenticatedUser();
                if (!isAuthenticated)
                {
                    filterContext.Result =
                        extendedController.HandleNotAuthenticatedUser(filterContext.RequestContext.HttpContext.Request.RawUrl);
                }
            }
            else if (filterContext.Controller is SimpleInjectorController)
            {
                SimpleInjectorController simpleInjectorController = filterContext.Controller as SimpleInjectorController;

                bool isAuthenticated = simpleInjectorController.IsAuthenticatedUser();
                if (!isAuthenticated)
                {
                    if (!string.IsNullOrWhiteSpace(this.UrlWhenUserNotAuthenticated))
                    {
                        if (filterContext.HttpContext.Request.IsAjaxRequest())
                        {
                            filterContext.Result = simpleInjectorController.
                                HandleNotAuthenticatedUserAjax(this.UrlWhenUserNotAuthenticated);
                        }
                        else
                        {
                            filterContext.Result = simpleInjectorController.
                                HandleNotAuthenticatedUser(this.UrlWhenUserNotAuthenticated);
                        }

                    }
                    else if (!string.IsNullOrWhiteSpace(ControllerWhenUserNotAuthenticated))
                    {
                        string action = string.IsNullOrWhiteSpace(ActionWhenUserNotAuthenticated)
                            ? "Index"
                            : ActionWhenUserNotAuthenticated;

                        if (filterContext.HttpContext.Request.IsAjaxRequest())
                        {
                            filterContext.Result = simpleInjectorController.
                                HandleNotAuthenticatedUserAjax(ControllerWhenUserNotAuthenticated, action);
                        }
                        else
                        {
                            filterContext.Result = simpleInjectorController.
                                HandleNotAuthenticatedUser(ControllerWhenUserNotAuthenticated, action);
                        }
                    }
                    else
                    {
                        filterContext.Result = simpleInjectorController.HandleNotAuthenticatedUser("Home", "Index");
                    }
                }
            }
        }

        public void OnAuthenticationChallenge(AuthenticationChallengeContext filterContext)
        {

        }
    }
}
