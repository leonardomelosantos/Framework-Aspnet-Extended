using FrameworkAspNetExtended.Context;
using FrameworkAspNetExtended.Core;
using FrameworkAspNetExtended.Entities.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Mvc.Async;

namespace FrameworkAspNetExtended.MVC.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class ProfilePermissionAttribute : PermissionAttributeBase, IActionFilter
    {
        public ProfilePermissionAttribute(params string[] requiredStringsPermissions)
        {
            RequiredStringsPermissions = requiredStringsPermissions;
        }

        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!(filterContext.Controller is Controllers.SimpleInjectorController))
            {
                return;
            }

            MethodInfo methodInfo = TryGetMethodInfo(filterContext);
            var perfisExigidos = GetRequiredPermissionsFor<ProfilePermissionAttribute>(methodInfo);
            if (perfisExigidos == null || !perfisExigidos.Any())
            {
                return;
            }

            var applicationManagerCustomOperations = ApplicationContext.Resolve<IApplicationManagerCustomOperations>();
            if (applicationManagerCustomOperations != null)
            {
                var user = applicationManagerCustomOperations.GetUserAuthenticated(
                    filterContext.Controller.ControllerContext.HttpContext);
                if (user == null)
                {
                    throw new PermissaoException("Usuário nulo e controller/action exige perfil",
                        perfisExigidos.ToArray(), new string[0]);
                }

                IList<string> perfisExigidosQueUsuarioNaoTem = new List<string>();
                var perfisDoUsuario = user.Profiles;
                perfisExigidosQueUsuarioNaoTem = perfisExigidos.Except(perfisDoUsuario).ToList();
                if ((perfisExigidos.Count > perfisExigidosQueUsuarioNaoTem.Count))
                {
                    return;
                }

                var exception = new PermissaoException(string.Format("Usuário sem perfil necessário: {0}", string.Join(", ", perfisExigidosQueUsuarioNaoTem.ToArray())),
                    perfisExigidos.ToArray(),
                    perfisDoUsuario.ToArray());

                var eventsManager = ApplicationContext.Resolve<IApplicationManagerEvents>();
                if (eventsManager == null)
                {
                    throw exception;
                }
                eventsManager.PermissionException(exception);
            }
        }

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {

        }
    }
}
