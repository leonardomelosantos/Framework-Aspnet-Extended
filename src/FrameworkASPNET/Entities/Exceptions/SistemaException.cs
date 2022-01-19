using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace FrameworkAspNetExtended.Entities.Exceptions
{
    [Serializable]
    public class SistemaException : Exception
    {
        public SistemaException() {}

        public SistemaException(string mensagem)
            : base(mensagem)
        {
        }

        public SistemaException(string mensagem, Exception innerException)
            : base(mensagem, innerException)
        {
        }

        public SistemaException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

    }
}
