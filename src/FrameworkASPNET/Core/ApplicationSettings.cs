using FrameworkAspNetExtended.Entities.Enums;

namespace FrameworkAspNetExtended.Core
{
    /// <summary>
    /// Classe onde são atribuídas configurações do comportamento da aplicação.
    /// </summary>
    public class ApplicationSettings
    {
        public static ApplicationSettings Default = new ApplicationSettings()
        {
            DependencyInjection = DependencyInjectionEngineType.SimpleInjector,
            HasRepositoryGenericIoC = true,
            HasServicePatternIoC = true
        };

        public DependencyInjectionEngineType DependencyInjection { get; set; }
        public bool HasServicePatternIoC { get; set; }
        public bool HasRepositoryGenericIoC { get; set; }
        public string PrefixNameSpace { get; set; }
    }
}
