using System;
using log4net;

namespace FrameworkAspNetExtended.Logger
{
    public class LoggerUtil
    {
        public static void LoggarDebugCalculandoTempoFinal(ILog log, string mensagem, string metodo, DateTime tempoInicial)
        {
            if (log.IsDebugEnabled)
            {
                TimeSpan tempoIntervalo = DateTime.Now.Subtract(tempoInicial);
                log.DebugFormat("{0} {1} tempo[{2}ms]", mensagem, metodo, tempoIntervalo.TotalMilliseconds);
            }
        }
    }
}
