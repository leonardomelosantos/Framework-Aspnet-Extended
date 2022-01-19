using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using FrameworkAspNetExtended.Context;
using FrameworkAspNetExtended.Core;
using FrameworkAspNetExtended.Entities;
using FrameworkAspNetExtended.Entities.Exceptions;

namespace FrameworkAspNetExtended.MVC.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class OperationPermissionAttribute : PermissionAttributeBase, IActionFilter
    {
        public OperationPermissionAttribute(params string[] requiredStringsPermissions)
        {
            RequiredStringsPermissions = requiredStringsPermissions;
        }

        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            MethodInfo methodInfo = ((ReflectedActionDescriptor)filterContext.ActionDescriptor).MethodInfo;

			if (!(filterContext.Controller is Controllers.SimpleInjectorController))
			{
				return;
			}

			var operacoesExigidas = GetRequiredPermissionsFor<OperationPermissionAttribute>(methodInfo);
			if (operacoesExigidas == null || !operacoesExigidas.Any())
			{
				return;
			}
			var applicationManagerCustomOperations = ApplicationContext.Resolve<IApplicationManagerCustomOperations>();
            if (applicationManagerCustomOperations != null)
            {
                var user = applicationManagerCustomOperations.GetUserAuthenticated(
                    filterContext.Controller.ControllerContext.HttpContext);
                IList<string> operacoesExigidasQueUsuarioNaoTem = new List<string>();
                if (user != null)
                {
                    
                    var operacoesUsuario = user.Operations;
                    operacoesExigidasQueUsuarioNaoTem = operacoesExigidas.Except(operacoesUsuario).ToList();
                    if (!operacoesExigidasQueUsuarioNaoTem.Any())
                    {
                        return;
                    }
                }
                throw new PermissaoException(string.Format("Usuário sem permissão: {0}", string.Join(", ", operacoesExigidasQueUsuarioNaoTem.ToArray())));
            }
        }

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
            
        }
    }
}
