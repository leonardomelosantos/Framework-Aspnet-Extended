using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrameworkAspNetExtended.Entities.Exceptions
{
    public class PermissaoException : Exception
    {
		public string DetalhamentoLog { get; set; }

		public PermissaoException()
            : base()
        {

        }

        public PermissaoException(string mensagem)
            : base(mensagem)
        {

        }

		public PermissaoException(string mensagem, string[] perfisExigidos, string[] perfisUsuario)
			: base(mensagem)
		{
			this.DetalhamentoLog = string.Format("Perfis exigidos: {0} | Perfis que usuário tem: {1}", 
				string.Join(",", perfisExigidos), 
				string.Join(",", perfisUsuario));
		}
	}
}
