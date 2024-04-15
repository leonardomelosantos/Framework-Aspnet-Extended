using FrameworkAspNetExtended.Context;
using FrameworkAspNetExtended.Core;
using FrameworkAspNetExtended.Entities.Events;
using FrameworkAspNetExtended.Repositories;
using FrameworkAspNetExtended.Services;
using log4net;
using System;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

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

            object[] saveLogAttributes = GetSaveLogMethodAttibute(metodoConcreto);

            HandleCutomOperationTimeoutAttibute(metodoConcreto, invocation);

            string mensagemLog = "Método do repositório executado.";
            DateTime tempoini = DateTime.Now;
            try
            {
                ExecuteIntercept(invocation);
            }
            catch (Exception ex)
            {
                mensagemLog = "Erro ao executar método do repositório";
                log.ErrorFormat("{0} {1}", mensagemLog, metodoConcreto.Name, ex);

                if (ex is DbUpdateConcurrencyException)
                {
                    try
                    {
                        if (applicationManagerEvents != null)
                            applicationManagerEvents.ConcurrencyException(ex);
                    }
                    catch
                    {
                        // Fazer nada.
                    }
                }
                else
                {
                    throw;
                }
            }

            TimeSpan tempoIntervalo = DateTime.Now.Subtract(tempoini);
            if (saveLogAttributes != null && saveLogAttributes.Any() && metodoConcreto != null)
            {
                log.DebugFormat("{0} {1} tempo[{2}ms]", mensagemLog, metodoConcreto.Name, tempoIntervalo.TotalMilliseconds);
            }

            CallBeforeRepositoryMethodExecute(applicationManagerEvents, eventInfo, tempoIntervalo);
        }

        private void HandleCutomOperationTimeoutAttibute(MethodBase metodoConcreto, IInvocation invocation)
        {
            try
            {
                int customMinutesTimeout = 0;
                //object[] customTimeoutAttibute = new object[0];
                if (metodoConcreto != null)
                {
                    System.Collections.Generic.IEnumerable<CustomCommandTimeoutAttribute> customTimeoutAttibute = metodoConcreto.GetCustomAttributes<CustomCommandTimeoutAttribute>(true);
                    if (customTimeoutAttibute.Any())
                    {
                        customMinutesTimeout = customTimeoutAttibute.First().Seconds;
                    }
                }
                if (customMinutesTimeout <= 0)
                    return;

                if (invocation != null && invocation.InvocationTarget is IRepositoryGeneric repositoryCaller
                    && repositoryCaller.Context != null
                    && repositoryCaller.Context.Database != null)
                {
                    repositoryCaller.Context.Database.CommandTimeout = customMinutesTimeout * 60;
                }
            } 
            catch (Exception ex)
            {
                log.Error("Erro ao setar operation timeout customizado.", ex);
            }
        }

        private static object[] GetSaveLogMethodAttibute(MethodBase metodoConcreto)
        {
            object[] saveLogAttributes = new object[0];
            if (metodoConcreto != null)
            {
                saveLogAttributes = metodoConcreto.GetCustomAttributes(typeof(SaveLog), true);
            }

            return saveLogAttributes;
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
            catch
            {
                // Fazer nada.
            }
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
            catch
            {
                // Fazer nada.
            }
        }

        protected virtual void ExecuteIntercept(IInvocation invocation)
        {
            invocation.Proceed();
        }
    }
}
