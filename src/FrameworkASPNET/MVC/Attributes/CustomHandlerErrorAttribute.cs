using FrameworkAspNetExtended.Context;
using FrameworkAspNetExtended.Core;
using FrameworkAspNetExtended.Entities;
using FrameworkAspNetExtended.Entities.Enums;
using FrameworkAspNetExtended.Entities.Exceptions;
using FrameworkAspNetExtended.MVC.Controllers;
using log4net;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace FrameworkAspNetExtended.MVC.Attributes
{
    public class CustomHandlerErrorAttribute : HandleErrorAttribute
    {
        private readonly static ILog _log = LogManager.GetLogger(typeof(CustomHandlerErrorAttribute));

        public const string ParamErrorViewName = "viewName";
        public const string ParamErrorModel = "model";
        public const string ParamErrorModelAction = "modelAction";

        public string CurrentView { get; set; }

        public override void OnException(ExceptionContext filterContext)
        {
            _log.Debug("CustomHandlerErrorAttribute.OnException()");

            if (filterContext.Exception != null && !filterContext.ExceptionHandled)
            {
                var applicationManagerEvents = ApplicationContext.ResolveWithSilentIfException<IApplicationManagerEvents>();

                var ex = filterContext.Exception;
                if (ex is BusinessException)
                {
                    HandleBusinessException(filterContext, ex as BusinessException, applicationManagerEvents);
                }
                else if (ex != null && ex.InnerException is BusinessException)
                {
                    HandleBusinessException(filterContext, ex.InnerException as BusinessException, applicationManagerEvents);
                }
                else
                {
                    if (filterContext.Controller is SimpleInjectorController)
                    {
                        if (applicationManagerEvents != null) applicationManagerEvents.Exception(ex);
                    }
                    new WebCustomErrorEvent(ex.Message, this, ex).Raise();
                }
            }
        }

        private void HandleBusinessException(ExceptionContext filterContext,
            BusinessException businessException, IApplicationManagerEvents applicationManagerEvents)
        {
            if (applicationManagerEvents != null) applicationManagerEvents.BusinessException(businessException);

            IList<string> messages = businessException.Messages ?? new List<string> { businessException.Message };

            this.HandleExceptionView(filterContext, messages);
        }

        private void HandleExceptionView(ExceptionContext filterContext, IList<string> messages)
        {
            _log.Debug("CustomHandlerErrorAttribute.HandleExceptionView()");

            var errors = filterContext.Controller.ViewData.ModelState.Values.SelectMany(v => v.Errors);
            foreach (ModelError error in errors)
            {
                if (!messages.Contains(error.ErrorMessage))
                {
                    messages.Add(error.ErrorMessage);
                }
            }

            if (filterContext.HttpContext.Request.IsAjaxRequest())
            {
                var response = filterContext.HttpContext.Response;
                response.AddHeader("X-Message-Type", MessageType.Error.ToString());
                response.ContentType = "text/json";
                response.Write(new JavaScriptSerializer().Serialize(messages));

                filterContext.ExceptionHandled = true;
                filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
            }
            else
            {
                var controller = filterContext.Controller as IControllerErrors;
                if (controller != null)
                {
                    controller.AddErrors(messages);
                }

                string actionName;
                if (filterContext.Controller.TempData.ContainsKey(ParamErrorViewName))
                {
                    actionName = (string)filterContext.Controller.TempData[ParamErrorViewName];
                }
                else
                {
                    actionName = (string)filterContext.RouteData.Values["action"];
                }

                var view = ViewEngines.Engines.FindView(filterContext.Controller.ControllerContext, actionName,
                    null).View
                           ??
                           ViewEngines.Engines.FindView(filterContext.Controller.ControllerContext, this.View, null)
                               .View;

                if (filterContext.Controller.TempData.ContainsKey(ParamErrorModel))
                {
                    dynamic model = filterContext.Controller.TempData[ParamErrorModel];
                    if (filterContext.Controller.TempData.ContainsKey(ParamErrorModelAction))
                    {
                        dynamic modelAction = filterContext.Controller.TempData[ParamErrorModelAction];
                        if (modelAction != null)
                        {
                            modelAction.Invoke(model);
                        }
                    }
                    filterContext.Controller.ViewData.Model = model;
                }

                filterContext.Result = new ViewResult
                {
                    View = view,
                    MasterName = this.Master,
                    ViewData = filterContext.Controller.ViewData,
                    TempData = filterContext.Controller.TempData
                };

                filterContext.ExceptionHandled = true;
                filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
            }
        }
    }
}