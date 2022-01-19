using FrameworkAspNetExtended.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace FrameworkAspNetExtended.Reflection
{
    public class ReflectionUtil
    {
        public static IEnumerable<Assembly> GetAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.FullName.StartsWith(ApplicationContext.PrefixNameSpace));
        }

        public static IEnumerable<Type> GetTypesImplementInterface<TInterface>()
        {
            return GetAssemblies()
                .SelectMany(a => a.GetTypes()
                    .Where(t => typeof(TInterface).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract));
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
