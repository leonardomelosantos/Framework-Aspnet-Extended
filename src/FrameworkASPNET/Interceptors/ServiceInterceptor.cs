using FrameworkAspNetExtended.Context;
using FrameworkAspNetExtended.Core;
using FrameworkAspNetExtended.Entities;
using FrameworkAspNetExtended.Entities.Events;
using FrameworkAspNetExtended.Services;
using log4net;
using System;
using System.Data;
using System.Data.Common;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace FrameworkAspNetExtended.Interceptadores
{
    public class ServiceInterceptor : IInterceptor
    {
        private readonly static ILog log = LogManager.GetLogger(typeof(ServiceInterceptor));

        public void Intercept(IInvocation invocation)
        {
            var concreteMethod = invocation.GetConcreteMethod(); // Caracterísica do SimpleInjector

            var eventInfo = new ServiceMethodEventInfo()
            {
                MethodName = concreteMethod != null ? concreteMethod.Name : string.Empty
            };
            var applicationManagerEvents = ApplicationContext.ResolveWithSilentIfException<IApplicationManagerEvents>();

            CallBeforeServiceMethodExecute(applicationManagerEvents, eventInfo);

            DateTime tempoini = DateTime.Now;

            string mensagemLog = Mensagens.MSG_METODO_NEGOCIO_SUCESSO;

            // Resolvendo o DatabaseContext que é criado a cada requisição.
            var databaseContext = ApplicationContext.Resolve<DatabaseContext>();

            // Resolvendo o RequestContext que é criado a cada requisição.
            var requestContext = ApplicationContext.Resolve<RequestContext>();

            try
            {
                requestContext.ControleQtdServicosExecutados++;

                object[] transactionRequeries = concreteMethod.GetCustomAttributes(typeof(TransactionRequired), true);
                object[] autoSaveChanges = concreteMethod.GetCustomAttributes(typeof(AutoSaveChanges), true);

                if (transactionRequeries.Any())
                {
                    databaseContext.InicializarDbContexts();

                    if (requestContext.ControleQtdServicosExecutados == 1)
                    {
                        // IsolationLevel padrão
                        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted;

                        // Obtendo o IsolationLevel específico caso tenha sido informado no Attribute
                        TransactionRequired transactionAttribute = (TransactionRequired)transactionRequeries[0];
                        if (transactionAttribute.IsolationLevel.HasValue)
                        {
                            isolationLevel = transactionAttribute.IsolationLevel.Value;
                        }

                        CreateDatabaseTransaction(databaseContext, requestContext, isolationLevel);
                    }
                }

                invocation.Proceed();

                requestContext.ControleQtdServicosExecutados--;

                if (autoSaveChanges.Any() || transactionRequeries.Any())
                {
                    requestContext.SaveChanges();
                }

                if (transactionRequeries.Any() && requestContext.ControleQtdServicosExecutados <= 0)
                {
                    requestContext.CommitTransactions();
                }
            }
            catch (Exception ex)
            {
                requestContext.ControleQtdServicosExecutados--;

                if (requestContext.ControleQtdServicosExecutados <= 0)
                {
                    requestContext.RollbackTransactions();
                }

                mensagemLog = Mensagens.MSG_METODO_NEGOCIO_ERRO;
                log.Error(mensagemLog, ex);
                throw;
            }
            finally
            {
                if (requestContext.ControleQtdServicosExecutados <= 0)
                {
                    requestContext.CloseConections();
                }
            }

            if (log.IsDebugEnabled)
            {
                TimeSpan tempoIntervaloAtual = DateTime.Now.Subtract(tempoini);
                log.DebugFormat("{0} {1} tempo[{2}ms]", mensagemLog, invocation.FullMethodName(), tempoIntervaloAtual.TotalMilliseconds);
            }

            TimeSpan tempoIntervalo = DateTime.Now.Subtract(tempoini);
            if (log.IsDebugEnabled)
            {
                log.DebugFormat("{0} {1} tempo[{2}ms]", mensagemLog, concreteMethod.Name, tempoIntervalo.TotalMilliseconds);
            }

            CallBeforeServiceMethodExecute(applicationManagerEvents, eventInfo, tempoIntervalo);
        }

        /// <summary>
        /// Obtendo os DbContexts que foram usados à medida que os repositórios foram pedindo os respectivos DbContexts.
        /// </summary>
        /// <param name="databaseContext"></param>
        /// <param name="requestContext"></param>
        /// <param name="isolationLevel"></param>
        private static void CreateDatabaseTransaction(DatabaseContext databaseContext, RequestContext requestContext, IsolationLevel isolationLevel)
        {
            if (databaseContext.DbContexts != null)
            {
                foreach (var dbContext in databaseContext.DbContexts)
                {
                    // Obtendo as conexões de cada instância DbContext
                    if (dbContext is IObjectContextAdapter objectContextAdapter && objectContextAdapter.ObjectContext != null)
                    {
                        DbConnection connection = objectContextAdapter.ObjectContext.Connection;
                        if (!requestContext.HasTransactionByDbConnection(connection))
                        {
                            if (connection.State != ConnectionState.Open)
                            {
                                connection.Open();
                            }
                            requestContext.AddTransaction(connection.BeginTransaction(isolationLevel));
                        }
                    }

                }
            }
        }

        private void CallBeforeServiceMethodExecute(IApplicationManagerEvents applicationManagerEvents,
            ServiceMethodEventInfo eventInfo, TimeSpan tempoIntervalo)
        {
            try
            {
                if (applicationManagerEvents != null)
                {
                    eventInfo.DurationOperationMilliseconds = tempoIntervalo.TotalMilliseconds;
                    applicationManagerEvents.BeforeServiceMethodExecute(eventInfo);
                }
            }
            catch
            {
                // Fazer nada.
            }
        }

        private void CallBeforeServiceMethodExecute(IApplicationManagerEvents applicationManagerEvents,
            ServiceMethodEventInfo eventInfo)
        {
            try
            {
                if (applicationManagerEvents != null)
                {
                    applicationManagerEvents.BeforeServiceMethodExecute(eventInfo);
                }
            }
            catch
            {
                // Fazer nada.
            }
        }
    }
}
