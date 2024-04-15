using FrameworkAspNetExtended.Entities;
using FrameworkAspNetExtended.Entities.Enums;
using FrameworkAspNetExtended.Entities.Exceptions;
using FrameworkAspNetExtended.MVC.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;

namespace FrameworkAspNetExtended.MVC.Controllers
{
    [CustomHandlerError]
    public class ExtendedController : Controller, IControllerErrors
    {
        public void PushErrorIntoModelState(string message)
        {
            ModelState.AddModelError("", message);
        }

        protected bool ModelStateHandleValid
        {
            get
            {
                if (!ModelState.IsValid)
                {
                    throw new BusinessException();
                }
                return true;
            }
        }

        #region Usuário logado

        /// <summary>
        /// Geralmente usado automaticamente pelo "AuthenticatedAttribute"
        /// </summary>
        /// <returns></returns>
        public virtual bool IsAuthenticatedUser()
        {
            return true;
        }

        public virtual string GetIdUserAuthenticated()
        {
            return null;
        }

        public virtual object GetUserAuthenticated()
        {
            return null;
        }

        public virtual ActionResult HandleNotAuthenticatedUser(string url)
        {
            return new RedirectToRouteResult(
                        new RouteValueDictionary(
                            new
                            {
                                controller = "Home",
                                action = "Index"
                            })
                        );
        }

        #endregion

        #region Tratamento de Exceções

        public virtual void TratarBusinessException(BusinessException exception)
        {

        }

        public virtual void TratarPermissaoException(PermissaoException exception)
        {

        }

        public virtual void TratarExceptionNaoTratada(Exception exception)
        {

        }

        #endregion

        #region Funcionalidades para exibição de mensagens

        public void AddErrors(IList<string> messages)
        {
            foreach (var message in messages)
            {
                if (!ModelState.Values.Any(v => v.Errors.Any(e => e.ErrorMessage == message)))
                {
                    ModelState.AddModelError("", message);
                }
            }
        }

        public void DisplayErrorMessage(IList<string> messages, bool displayAfterRedirect = false)
        {
            DisplayMessage(MessageType.Error, messages, displayAfterRedirect);
        }

        public void DisplayErrorMessage(string message, bool displayAfterRedirect = false)
        {
            DisplayMessage(MessageType.Error, message, displayAfterRedirect);
        }

        public void DisplaySuccessMessage(string message = null, bool displayAfterRedirect = true)
        {
            DisplayMessage(MessageType.Success, message ?? "Operação realizada com sucesso!", displayAfterRedirect);
        }

        public void DisplayAlertMessage(string message, bool displayAfterRedirect = true)
        {
            DisplayMessage(MessageType.Alert, message, displayAfterRedirect);
        }

        public void DisplayAlertMessage(IList<string> messages, bool displayAfterRedirect = true)
        {
            DisplayMessage(MessageType.Alert, messages, displayAfterRedirect);
        }

        private void DisplayMessage(MessageType messageType, object message, bool displayAfterRedirect = false)
        {

            if (this.Request.IsAjaxRequest())
            {
                this.Response.AddHeader("X-Message-Type", messageType.ToString());
            }

            var keyMessageType = messageType.ToString();

            if (displayAfterRedirect)
            {
                TempData[keyMessageType] = message;
            }
            else
            {
                ViewData[keyMessageType] = message;
            }
        }

        #endregion

        #region Results

        protected ActionResult AjaxSuccessResult(object messages, bool reloadCurrentView)
        {
            if (reloadCurrentView)
            {
                Response.AddHeader("RELOAD_LOCATION", "true");
                this.DisplayMessage(MessageType.Success, messages, displayAfterRedirect: true);
                return Json(messages);
            }
            return AjaxSuccessResult(messages);
        }

        protected ActionResult AjaxSuccessResult(object messages, String urlToRedirectAndShowMessage = null)
        {

            if (!String.IsNullOrWhiteSpace(urlToRedirectAndShowMessage))
            {
                Response.AddHeader("REDIRECT_LOCATION", urlToRedirectAndShowMessage);
                this.DisplayMessage(MessageType.Success, messages, displayAfterRedirect: true);
                return Json(messages);
            }
            Response.AddHeader("X-Message-Type", MessageType.Success.ToString());
            return Json(messages);
        }

        #endregion

        #region Tratamento de erros

        protected void SetViewError<T>(string viewName, T model)
        {
            TempData[CustomHandlerErrorAttribute.ParamErrorViewName] = viewName;
            TempData[CustomHandlerErrorAttribute.ParamErrorModel] = model;
            TempData[CustomHandlerErrorAttribute.ParamErrorModelAction] = null;
        }

        protected void SetViewError<T>(string viewName, T model, Action<T> modelAction)
        {
            TempData[CustomHandlerErrorAttribute.ParamErrorViewName] = viewName;
            TempData[CustomHandlerErrorAttribute.ParamErrorModel] = model;
            TempData[CustomHandlerErrorAttribute.ParamErrorModelAction] = modelAction;
        }

        protected void SetViewError(string viewName)
        {
            TempData[CustomHandlerErrorAttribute.ParamErrorViewName] = viewName;
            TempData[CustomHandlerErrorAttribute.ParamErrorModel] = null;
            TempData[CustomHandlerErrorAttribute.ParamErrorModelAction] = null;
        }

        protected void SetViewError<T>(T model)
        {
            TempData[CustomHandlerErrorAttribute.ParamErrorViewName] = null;
            TempData[CustomHandlerErrorAttribute.ParamErrorModel] = model;
            TempData[CustomHandlerErrorAttribute.ParamErrorModelAction] = null;
        }

        protected void SetViewError<T>(T model, Action<T> modelAction)
        {
            TempData[CustomHandlerErrorAttribute.ParamErrorViewName] = null;
            TempData[CustomHandlerErrorAttribute.ParamErrorModel] = model;
            TempData[CustomHandlerErrorAttribute.ParamErrorModelAction] = modelAction;
        }

        #endregion

        #region Upload e download

        [HttpPost]
        public ActionResult UploadFile(int? chunk, string name)
        {
            var fileUpload = Request.Files[0];
            chunk = chunk ?? 0;

            var buffer = new byte[fileUpload.InputStream.Length];
            fileUpload.InputStream.Read(buffer, 0, buffer.Length);

            // TODO To be implemented
            //if (chunk == 0)
            //{
            //    // FileTemp.CreateFileTemp(name, buffer);
            //}
            //else
            //{
            //    // FileTemp.AppendFileTemp(name, buffer);
            //}

            return Content("chunk uploaded", "text/plain");
        }

        public FileResult StartDownloadFile(byte[] contentFile, string filename)
        {
            Response.AddHeader("Content-Disposition", string.Format("attachment;filename={0}", filename));
            var stream = new MemoryStream(contentFile);
            return new FileStreamResult(stream, filename);
        }

        #endregion

    }
}
