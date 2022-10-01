using FrameworkAspNetExtended.Entities.Enums;
using SimpleInjector;
using System;
using System.Collections.Generic;

namespace FrameworkAspNetExtended.Context
{
    public static class ApplicationContext
    {
        public static string PrefixNameSpace;
        public const string PrefixNamespaceFramework = "FrameworkAspNetExtended.";

        public static DependencyInjectionEngineType DependencyInjection { get; set; }

        private static IList<Type> _allPossibleDbContextTypes;
        public static IList<Type> AllPossibleDbContextTypes
        {
            get
            {
                if (_allPossibleDbContextTypes == null)
                {
                    _allPossibleDbContextTypes = new List<Type>();
                }
                return _allPossibleDbContextTypes;
            }
        }

        private static Container _containerSimpleInjector;
        public static Container ContainerSimpleInjector
        {
            get
            {
                if (_containerSimpleInjector == null)
                {
                    _containerSimpleInjector = new Container();
                }
                return _containerSimpleInjector;
            }
        }

        public static T Resolve<T>() where T : class
        {
            if (DependencyInjection == DependencyInjectionEngineType.SimpleInjector)
            {
                return ContainerSimpleInjector.GetInstance<T>();
            }

            if (DependencyInjection == DependencyInjectionEngineType.WindsorCastle)
            {
                return null;
            }
            return null;
        }

        public static T ResolveWithSilentIfException<T>() where T : class
        {
            try
            {
                if (DependencyInjection == DependencyInjectionEngineType.SimpleInjector)
                {
                    return ContainerSimpleInjector.GetInstance<T>();
                }

                if (DependencyInjection == DependencyInjectionEngineType.WindsorCastle)
                {
                    return null;
                }
            }
            catch (Exception) { }
            return null;
        }
    }
}
