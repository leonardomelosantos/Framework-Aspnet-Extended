using SimpleInjector;

namespace FrameworkAspNetExtended
{
    public interface IConfigurable
    {
        void RunConfiguration(Container container);
    }
}
