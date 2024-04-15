using FrameworkAspNetExtended.Entities.Exceptions;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace FrameworkAspNetExtended.Entities
{
    [Serializable]
    public class BusinessException : SistemaException
    {
        public IList<string> Messages { get; set; }

        public BusinessException() : this(new List<string>()) { }

        public BusinessException(string mensagem, params object[] args)
            : base(mensagem)
        {
            this.Messages = new List<String> { string.Format(mensagem, args) };
        }
        public BusinessException(string mensagem)
            : base(mensagem)
        {
            this.Messages = new List<String> { mensagem };
        }

        public BusinessException(IList<string> mensagens)
        {
            this.Messages = mensagens;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Messages", this.Messages);
            base.GetObjectData(info, context);
        }
        /// <summary>
        /// Construtor da classe.
        /// </summary>
        /// <param name="info">Informação de serialização da classe.</param>
        /// <param name="context">Dados de contexto para serialização.</param>
        public BusinessException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.Messages = info.GetValue("Messages", typeof(IList<String>)) as IList<String>;
        }
    }
}
