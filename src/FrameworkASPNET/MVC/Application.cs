using FrameworkAspNetExtended.Context;
using FrameworkAspNetExtended.Core;
using FrameworkAspNetExtended.Reflection;
using log4net;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace FrameworkAspNetExtended.MVC
{
    public abstract class Application
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Application));

        /// <summary>
        /// Configura o log4net (ou outro logger) dinamicamente para não depender de arquivos de configuração
        /// </summary>
        /// <param name="settings"></param>
        protected static void ConfigurarLogger(ApplicationSettings settings)
        {
            /*
            http://stackoverflow.com/questions/16336917/can-you-configure-log4net-in-code-instead-of-using-a-config-file
            */
        }

        protected static void ExecutarTodasConfiguracoesAutomaticas()
        {
            // Obtendo todas as instâncias que implementam a interface 'IConfigurable'
            var types = ReflectionUtil.GetTypesImplementInterface<IConfigurable>();
            foreach (Type type in types)
            {
                ConstructorInfo ci = type.GetConstructor(Type.EmptyTypes);

                IConfigurable configuration = (IConfigurable)ci.Invoke(null);

                Stopwatch watch = Stopwatch.StartNew();

                Log.InfoFormat("Configuração automática: {0}.", configuration.GetType().FullName);

                configuration.ExecutarConfiguracao();

                Log.InfoFormat("[{0}] Configuração automática: {1} concluída com sucesso.", watch.ElapsedMilliseconds, configuration.GetType().FullName);

                watch.Stop();
            }
        }

        protected static void LoadAssemblies()
        {
            string privateBinPath = AppDomain.CurrentDomain.SetupInformation.PrivateBinPath;

            if (Directory.Exists(privateBinPath))
            {
                string[] assemblyFiles = Directory.GetFiles(privateBinPath, "*.dll", SearchOption.AllDirectories);

                foreach (string assemblyFile in assemblyFiles)
                {
                    AssemblyName assemblyName = AssemblyName.GetAssemblyName(assemblyFile);

                    if (assemblyName.FullName.StartsWith(ApplicationContext.PrefixNameSpace))
                    {
                        if (!AppDomain.CurrentDomain.GetAssemblies()
                            .Any(assembly => AssemblyName.ReferenceMatchesDefinition(assemblyName, assembly.GetName())))
                        {
                            try
                            {
                                Assembly.Load(assemblyName);
                            }
                            catch (Exception ex)
                            {
                                Log.Error(string.Format("Erro ao carregar o assembly '{0}'", assemblyName.FullName), ex);
                            }
                        }
                    }
                }
            }
        }
    }
}
