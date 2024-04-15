using FrameworkAspNetExtended.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace FrameworkAspNetExtended.Reflection
{
    public static class ReflectionUtil
    {
        public static IEnumerable<Assembly> GetAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.FullName.StartsWith(ApplicationContext.PrefixNameSpace));
        }

        public static IEnumerable<Type> GetTypesImplementInterface<TInterface>(List<string> errors = null)
        {
            List<Type> result = new List<Type>();

            IEnumerable<Assembly> assemblies = GetAssemblies();
            foreach (var assembly in assemblies)
            {
                try
                {
                    var allAssemblyTypes = assembly.GetTypes();

                    var typesThatImplementsInterface = allAssemblyTypes.Where(t => typeof(TInterface).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract);

                    result.AddRange(typesThatImplementsInterface);
                }
                catch (ReflectionTypeLoadException rtlEx)
                {

                    if (errors != null && rtlEx.LoaderExceptions != null && rtlEx.LoaderExceptions.Any())
                    {
                        foreach (var exItem in rtlEx.LoaderExceptions)
                        {
                            errors.Add(exItem.Message + " " + exItem.StackTrace);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (errors != null)
                        errors.Add(ex.Message + " " + ex.StackTrace);
                }
            }
            return result;
        }

        public static IEnumerable<TInterface> GetInstanceImplementInterface<TInterface>()
        {
            var classTypes = GetTypesImplementInterface<TInterface>();

            foreach (Type classType in classTypes)
            {
                ConstructorInfo ci = classType.GetConstructor(Type.EmptyTypes);
                if (ci != null)
                {
                    yield return (TInterface)ci.Invoke(null);
                }
            }
        }

        public static string GetPropertyName<T>(Expression<Func<T, object>> expression)
        {
            var body = expression.Body as MemberExpression;

            if (body == null)
            {
                body = ((UnaryExpression)expression.Body).Operand as MemberExpression;
            }

            return body.Member.Name;
        }
    }
}
