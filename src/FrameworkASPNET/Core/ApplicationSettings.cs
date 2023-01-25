using FrameworkAspNetExtended.Entities.Enums;
using System.Collections.Generic;

namespace FrameworkAspNetExtended.Core
{
    /// <summary>
    /// Classe onde são atribuídas configurações do comportamento da aplicação.
    /// </summary>
    public class ApplicationSettings
    {
        public static ApplicationSettings Default()
        {
            return new ApplicationSettings()
            {
                DependencyInjection = DependencyInjectionEngineType.SimpleInjector,
                HasRepositoryGenericIoC = true,
                HasServicePatternIoC = true,
                Errors = new List<string>()
            };
        }

        public DependencyInjectionEngineType DependencyInjection { get; set; }
        public bool HasServicePatternIoC { get; set; }
        public bool HasRepositoryGenericIoC { get; set; }
        public string PrefixNameSpace { get; set; }
        public List<string> Errors { get; set; }
    }
}
