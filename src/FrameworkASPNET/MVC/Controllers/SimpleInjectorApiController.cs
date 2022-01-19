using FrameworkAspNetExtended.Context;
using FrameworkAspNetExtended.Core;
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
using log4net;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace FrameworkAspNetExtended.MVC.Controllers
{
    [CustomHandlerApiError]
    public class SimpleInjectorApiController : System.Web.Http.ApiController
	{
		protected readonly static ILog _log = LogManager.GetLogger(typeof(SimpleInjectorApiController));

		#region ModelState

		protected void PushErrorIntoModelState(string message)
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

		protected bool ValidatePermissionBasicAuthentication()
		{
			UserAutenticatedInfo userInfo = GetAuthorizationBasic();

			var applicationManagerCustomOperations = ApplicationContext.Resolve<IApplicationApiManagerCustomOperations>();
			if (applicationManagerCustomOperations != null)
			{
				if (! applicationManagerCustomOperations.ValidateBasicAuthentication(userInfo, this.Request))
				{
					var resp = new HttpResponseMessage(HttpStatusCode.Unauthorized)
					{
						Content = new StringContent("Não autorizado"),
						ReasonPhrase = "Não autorizado."
					};
					throw new HttpResponseException(resp);
				}
			}
			RegistrarLogNenhumaClasseAutenticacaoEncontrada();
			return false;
		}

		protected UserAutenticatedInfo GetAuthorizationBasic()
		{
			UserAutenticatedInfo credentials = null;
			try
			{
				string authorization = Request.Headers.Authorization.Parameter;
				string loginSenhaConcatenados = System.Text.ASCIIEncoding.ASCII.GetString(Convert.FromBase64String(authorization));
				if (loginSenhaConcatenados.Contains(":"))
				{
					string[] loginSenha = loginSenhaConcatenados.Split(':');
					credentials = new UserAutenticatedInfo()
					{
						Id = loginSenha[0],
						Name = loginSenha[0],
						Username = loginSenha[0],
						Password = loginSenha[1]
					};
				}
			}
			catch (Exception ex)
			{
				_log.Debug(ex);
			}
			return credentials;
		}
		
		/// <summary>
		/// 
		/// </summary>
		private void RegistrarLogNenhumaClasseAutenticacaoEncontrada()
		{
			_log.Debug(Mensagens.MSG_NENHUM_AUTENTICADOR);
		}

	}
}
