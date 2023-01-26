using FrameworkAspNetExtended.Context;
using FrameworkAspNetExtended.Core;
using FrameworkAspNetExtended.Reflection;
using log4net;
using System;
using System.Collections.Generic;
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

        protected static void ExecutarTodasConfiguracoesAutomaticas(ApplicationSettings settings)
        {
            List<string> assemblyErrors = new List<string>();

            // Obtendo todas as instâncias que implementam a interface 'IConfigurable'
            System.Collections.Generic.List<Type> types = 
                ReflectionUtil.GetTypesImplementInterface<IConfigurable>(assemblyErrors).ToList();
            
            if (assemblyErrors.Any() && settings != null && settings.Errors != null)
            {
                settings.Errors.AddRange(assemblyErrors);
            }

            foreach (Type type in types)
            {
                try
                {
                    ConstructorInfo ci = type.GetConstructor(Type.EmptyTypes);

                    IConfigurable configuration = (IConfigurable)ci.Invoke(null);

                    Stopwatch watch = Stopwatch.StartNew();

                    Log.InfoFormat("Configuração automática: {0}.", configuration.GetType().FullName);

                    configuration.RunConfiguration();

                    Log.InfoFormat("[{0}] Configuração automática: {1} concluída com sucesso.", watch.ElapsedMilliseconds, configuration.GetType().FullName);

                    watch.Stop();
                } 
                catch (Exception ex)
                {
                    settings.Errors.Add(ex.Message + " - " + ex.StackTrace);
                }
                
            }
        }

        protected static void LoadAssemblies(ApplicationSettings settings)
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
