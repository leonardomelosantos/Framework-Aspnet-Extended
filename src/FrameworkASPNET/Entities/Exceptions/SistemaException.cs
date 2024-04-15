using System;
using System.Runtime.Serialization;

namespace FrameworkAspNetExtended.Entities.Exceptions
{
    [Serializable]
    public class SistemaException : Exception
    {
        public SistemaException() { }

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
