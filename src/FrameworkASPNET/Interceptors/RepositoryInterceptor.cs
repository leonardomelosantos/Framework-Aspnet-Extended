using System;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Reflection;
using FrameworkAspNetExtended.Context;
using FrameworkAspNetExtended.Core;
using FrameworkAspNetExtended.Entities.Events;
using FrameworkAspNetExtended.Services;
using log4net;

namespace FrameworkAspNetExtended.Interceptadores
{
    public class RepositoryInterceptor : IInterceptor
    {
        private readonly static ILog log = LogManager.GetLogger(typeof(RepositoryInterceptor));

        /// <summary>
        /// Método executado ao interceptar o método.
        /// </summary>
        /// <param name="invocation">Representa o método de repositório a ser executado.</param>
        public void Intercept(IInvocation invocation)
        {
            var metodoConcreto = invocation.GetConcreteMethod(); // Característica do interceptador do SimpleInjector

            var eventInfo = new RepositoryMethodEventInfo()
            {
                MethodName = metodoConcreto != null ? metodoConcreto.Name : string.Empty
            };
            var applicationManagerEvents = ApplicationContext.ResolveWithSilentIfException<IApplicationManagerEvents>();

            CallBeforeRepositoryMethodExecute(applicationManagerEvents, eventInfo);

			object[] saveLogAttributes = metodoConcreto.GetCustomAttributes(typeof(SaveLog), true);

			string mensagemLog = "Método do repositório executado.";
            DateTime tempoini = DateTime.Now;
            try
            {
                ExecuteIntercept(invocation);
            }
            catch (Exception ex)
            {
                mensagemLog = "Erro ao executar método do repositório";
				log.Error(string.Format("{0} {1}", mensagemLog, metodoConcreto.Name), ex);

                if (ex is DbUpdateConcurrencyException)
                {
                    try
                    {
                        if (applicationManagerEvents != null) applicationManagerEvents.ConcurrencyException(ex);
                    }
                    catch (Exception) { }
                }
                else
                {
                    throw;
                }
            }

			TimeSpan tempoIntervalo = DateTime.Now.Subtract(tempoini);
			if (saveLogAttributes.Any())
			{
				log.DebugFormat("{0} {1} tempo[{2}ms]", mensagemLog, metodoConcreto.Name, tempoIntervalo.TotalMilliseconds);
			}

			CallBeforeRepositoryMethodExecute(applicationManagerEvents, eventInfo, tempoIntervalo);
        }

        private void CallBeforeRepositoryMethodExecute(IApplicationManagerEvents applicationManagerEvents,
            RepositoryMethodEventInfo eventInfo, TimeSpan tempoIntervalo)
        {
            try
            {
                if (applicationManagerEvents != null)
                {
                    eventInfo.DurationOperationMilliseconds = tempoIntervalo.TotalMilliseconds;
                    applicationManagerEvents.BeforeRepositoryMethodExecute(eventInfo);
                }
            }
            catch (Exception) { }
        }

        private void CallBeforeRepositoryMethodExecute(IApplicationManagerEvents applicationManagerEvents,
            RepositoryMethodEventInfo eventInfo)
        {
            try
            {
                if (applicationManagerEvents != null)
                {
                    applicationManagerEvents.BeforeRepositoryMethodExecute(eventInfo);
                }
            }
            catch (Exception) { }
        }

        protected virtual void ExecuteIntercept(IInvocation invocation)
        {
            invocation.Proceed();
        }
    }
}
