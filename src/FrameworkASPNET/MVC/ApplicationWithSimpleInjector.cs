using FrameworkAspNetExtended.Context;
using FrameworkAspNetExtended.Core;
using FrameworkAspNetExtended.Interceptadores;
using FrameworkAspNetExtended.Reflection;
using FrameworkAspNetExtended.Repositories;
using FrameworkAspNetExtended.Services;
using SimpleInjector;
using SimpleInjector.Integration.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;

namespace FrameworkAspNetExtended.MVC
{
    public class ApplicationWithSimpleInjector : Application
    {
        // private static readonly ILog _log = LogManager.GetLogger(typeof(ApplicationWithSimpleInjector));

        public static void Initialize<T>(ApplicationSettings settings)
            where T : IApplicationManagerCustomOperations
        {
            var container = InitializeBase<T>(settings);

            VerifyRegistrationsAndSetResolver(container);
        }

        public static void Initialize<T1, T2>(ApplicationSettings settings)
            where T1 : IApplicationManagerCustomOperations
            where T2 : IApplicationManagerEvents
        {
            var container = InitializeBase<T1>(settings);

            RegistrarClasseEventos<T2>(container);

            VerifyRegistrationsAndSetResolver(container);
        }

        private static Container InitializeBase<T>(ApplicationSettings settings) where T : IApplicationManagerCustomOperations
        {
            settings.Errors = new List<string>();

            ApplicationContext.DependencyInjection = Entities.Enums.DependencyInjectionEngineType.SimpleInjector;
            ApplicationContext.PrefixNameSpace = settings.PrefixNameSpace;

            LoadAssemblies(settings);

            try
            {
                ExecutarTodasConfiguracoesAutomaticas(settings);
            }
            catch (Exception ex)
            {
                settings.Errors.Add(ex.Message + " " + ex.StackTrace);
            }
            
            ConfigurarLogger(settings);

            // Obtendo a instância do container do SimpleInjector.
            var container = ApplicationContext.ContainerSimpleInjector;

            // Registrando os interceptadores
            container.RegisterSingle<ServiceInterceptor>();
            container.RegisterSingle<RepositoryInterceptor>();

            // Registrando as interfaces e classes que vão trabalhar como repositórios genéricos.
            if (settings.HasRepositoryGenericIoC)
            {
                RegistrarRepositoriosSimpleInjector(container);
            }

            // Registrando as interfaces e classes que vão trabalhar estilo serviço.
            if (settings.HasServicePatternIoC)
            {
                RegistrarServicosSimpleInjector(container);
            }

            // Registrando a classe que será usada para tratar as autenticações
            RegistrarClasseManagerCustomOperations<T>(container);

            // IoC dos Controllers MVC
            container.RegisterMvcControllers(Assembly.GetExecutingAssembly());

            // Registrando DatabaseContext por requisição web, a fim de trabalhar com transações de um ou mais bancos de dados.
            container.RegisterPerWebRequest<DatabaseContext>();

            container.RegisterPerWebRequest<RequestContext>();
            return container;
        }

        private static void VerifyRegistrationsAndSetResolver(Container container)
        {
            container.Verify();

            DependencyResolver.SetResolver(new SimpleInjectorDependencyResolver(container));
        }

        private static void RegistrarClasseManagerCustomOperations<T>(Container container)
            where T : IApplicationManagerCustomOperations
        {
            Type classType = typeof(T);
            var interfaceType = classType.GetInterfaces()
                .First(t => typeof(IApplicationManagerCustomOperations).IsAssignableFrom(t) && t.IsInterface);

            container.RegisterSingle(interfaceType, classType);
        }

        private static void RegistrarClasseEventos<T>(Container container)
            where T : IApplicationManagerEvents
        {
            Type classType = typeof(T);
            var interfaceType = classType.GetInterfaces()
                .First(t => typeof(IApplicationManagerEvents).IsAssignableFrom(t) && t.IsInterface);

            container.RegisterSingle(interfaceType, classType);
        }

        private static void RegistrarRepositoriosSimpleInjector(Container container)
        {
            var classTypes = ReflectionUtil.GetTypesImplementInterface<IRepositoryGeneric>();

            foreach (Type classType in classTypes)
            {
                var interfaceType = classType.GetInterfaces()
                    .First(t => typeof(IRepositoryGeneric).IsAssignableFrom(t)
                                && t.IsInterface
                                && !t.FullName.StartsWith(ApplicationContext.PrefixNamespaceFramework));

                IdentityDbContext(classType);

                container.RegisterSingle(interfaceType, classType);
                container.InterceptWith<RepositoryInterceptor>(t => t == interfaceType);
            }
        }

        private static void IdentityDbContext(Type classType)
        {
            if (classType == null || classType.BaseType == null)
            {
                return;
            }
            var genericTypesArguments = classType.BaseType.GetGenericArguments();
            foreach (var genericTypeIntoRepositoryType in genericTypesArguments)
            {
                if (genericTypeIntoRepositoryType.BaseType == typeof(DbContext) &&
                    !ApplicationContext.AllPossibleDbContextTypes.Contains(genericTypeIntoRepositoryType))
                {
                    ApplicationContext.AllPossibleDbContextTypes.Add(genericTypeIntoRepositoryType);
                }
            }
        }

        private static void RegistrarServicosSimpleInjector(Container container)
        {
            var classTypes = ReflectionUtil.GetTypesImplementInterface<IService>();

            foreach (Type classType in classTypes)
            {
                var interfaceType = classType.GetInterfaces()
                    .First(t => typeof(IService).IsAssignableFrom(t)
                                && t.IsInterface
                                && !t.FullName.StartsWith(ApplicationContext.PrefixNamespaceFramework));

                container.RegisterSingle(interfaceType, classType);
                container.InterceptWith<ServiceInterceptor>(t => t == interfaceType);
            }
        }
    }
}
