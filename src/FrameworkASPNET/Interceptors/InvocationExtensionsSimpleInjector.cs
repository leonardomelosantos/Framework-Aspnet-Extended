namespace FrameworkAspNetExtended.Interceptadores
{
    public static class InvocationExtensions
    {
        public static string FullMethodName(this IInvocation invocation)
        {
            var method = invocation.GetConcreteMethod();

            return string.Format("{0}.{1}", method.ReflectedType.Name, method.Name);
        }
    }
}
