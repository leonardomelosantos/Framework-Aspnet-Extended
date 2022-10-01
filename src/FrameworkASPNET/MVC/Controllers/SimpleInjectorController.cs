using FrameworkAspNetExtended.Context;
using FrameworkAspNetExtended.Core;
using FrameworkAspNetExtended.Entities;
using FrameworkAspNetExtended.Entities.Enums;
using FrameworkAspNetExtended.MVC.Attributes;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace FrameworkAspNetExtended.MVC.Controllers
{
    [CustomHandlerError]
    public class SimpleInjectorController : AsyncController, IControllerErrors
    {
        private readonly static ILog _log = LogManager.GetLogger(typeof(SimpleInjectorController));

        #region ModelState

        public void PushErrorIntoModelState(string message)
        {
            ModelState.AddModelError("", message);
        }

        protected bool ModelStateHandleValid
        {
            get
            {
                if (ModelState.IsValid)
                {
                    return true;
                }
                throw new BusinessException();
            }
        }

        #endregion

        #region Usuário logado

        public UserAutenticatedInfo ValidateLogon(UserAutenticatedInfo userInfo)
        {
            var applicationManagerCustomOperations = ApplicationContext.Resolve<IApplicationManagerCustomOperations>();
            if (applicationManagerCustomOperations != null)
            {
                return applicationManagerCustomOperations.ValidateUserCredentials(userInfo);
            }
            RegistrarLogNenhumaClasseAutenticacaoEncontrada();
            return null;
        }

        public UserAutenticatedInfo RememberPassword(string username)
        {
            var applicationManagerCustomOperations = ApplicationContext.Resolve<IApplicationManagerCustomOperations>();
            if (applicationManagerCustomOperations != null)
            {
                return applicationManagerCustomOperations.RememberPassword(username);
            }
            RegistrarLogNenhumaClasseAutenticacaoEncontrada();
            return null;
        }

        public UserAutenticatedInfo ChangePassword(UserAutenticatedInfo userInfo, string newPassword)
        {
            var applicationManagerCustomOperations = ApplicationContext.Resolve<IApplicationManagerCustomOperations>();
            if (applicationManagerCustomOperations != null)
            {
                return applicationManagerCustomOperations.ChangePassword(ControllerContext.HttpContext, userInfo, newPassword);
            }
            RegistrarLogNenhumaClasseAutenticacaoEncontrada();
            return null;
        }

        public void RegisterLogon(UserAutenticatedInfo user)
        {
            var applicationManagerCustomOperations = ApplicationContext.Resolve<IApplicationManagerCustomOperations>();
            if (applicationManagerCustomOperations != null)
            {
                applicationManagerCustomOperations.RegisterLogon(ControllerContext.HttpContext, user);
            }
            else
            {
                RegistrarLogNenhumaClasseAutenticacaoEncontrada();
            }
        }

        public void RegisterLogoff()
        {
            var applicationManagerCustomOperations = ApplicationContext.Resolve<IApplicationManagerCustomOperations>();
            if (applicationManagerCustomOperations != null)
            {
                applicationManagerCustomOperations.RegisterLogoff(ControllerContext.HttpContext);
            }
            else
            {
                RegistrarLogNenhumaClasseAutenticacaoEncontrada();
            }
        }

        public bool IsAuthenticatedUser()
        {
            var applicationManagerCustomOperations = ApplicationContext.Resolve<IApplicationManagerCustomOperations>();
            if (applicationManagerCustomOperations != null)
            {
                return applicationManagerCustomOperations.IsAuthenticatedUser(ControllerContext.HttpContext);
            }
            else
            {
                RegistrarLogNenhumaClasseAutenticacaoEncontrada();
            }
            return true;
        }

        public string GetIdUserAuthenticated()
        {
            var usuarioAutenticado = GetUserAuthenticated();
            if (usuarioAutenticado != null)
            {
                return usuarioAutenticado.Id;
            }
            return null;
        }

        public UserAutenticatedInfo GetUserAuthenticated()
        {
            var applicationManagerCustomOperations = ApplicationContext.Resolve<IApplicationManagerCustomOperations>();
            if (applicationManagerCustomOperations != null)
            {
                return applicationManagerCustomOperations.GetUserAuthenticated(ControllerContext.HttpContext);
            }
            RegistrarLogNenhumaClasseAutenticacaoEncontrada();
            return null;
        }

        /// <summary>
        /// Chamado quando no "AuthenticatedAttribute" o resultado do método "IsAuthenticatedUser()" retorna false.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public ActionResult HandleNotAuthenticatedUser(string url)
        {
            return new RedirectResult(url);
        }

        /// <summary>
        /// Chamado quando no "AuthenticatedAttribute" o resultado do método "IsAuthenticatedUser()" retorna false.
        /// </summary>
        /// <param name="controllerName"></param>
        /// <param name="actionName"></param>
        /// <returns></returns>
        public ActionResult HandleNotAuthenticatedUser(string controllerName, string actionName)
        {
            return new RedirectResult(Url.Action(actionName, controllerName));
        }

        public ActionResult HandleNotAuthenticatedUserAjax(string url)
        {
            return AjaxSuccessResult("Sessão expirada.", url);
        }

        public ActionResult HandleNotAuthenticatedUserAjax(string controllerName, string actionName)
        {
            return AjaxSuccessResult("Sessão expirada.", Url.Action(actionName, controllerName, null, this.Request.Url.Scheme));
        }

        /// <summary>
        /// 
        /// </summary>
        private void RegistrarLogNenhumaClasseAutenticacaoEncontrada()
        {
            _log.Debug(Mensagens.MSG_NENHUM_AUTENTICADOR);
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

            if (chunk == 0)
            {
                // FileTemp.CreateFileTemp(name, buffer);
            }
            else
            {
                // FileTemp.AppendFileTemp(name, buffer);
            }
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
