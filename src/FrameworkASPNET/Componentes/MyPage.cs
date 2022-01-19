using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Configuration;
using System.Collections;
using System.Collections.Specialized;
using System.Web.UI.WebControls;
using System.Xml;
using System.Data;

namespace FrameworkASPNET.Componentes
{
    public class MyPage : Page
    {

        #region Sessão Usuário Logado

        private const String SESSION_USUARIO = "UsuarioLogado";

        public void Sessao_SalvarUsuarioLogado(object u)
        {
            Session[SESSION_USUARIO] = u;
        }

        public object Sessao_ConsultarUsuarioLogado()
        {
            //object retorno = Session[SESSION_USUARIO];
            //if (Session[SESSION_USUARIO] is usuario)
            //{
            //    return (usuario)retorno;
            //}
            //else
            //{
            //    return null;
            //}
            return Session[SESSION_USUARIO];
        }

        #endregion

        #region Sessão Operador Logado

        private const String SESSION_OPERADOR = "OperadorLogado";

        public void Sessao_SalvarOperadorLogado(object op)
        {
            Session[SESSION_OPERADOR] = op;
        }

        public object Sessao_ConsultarOperadorLogado()
        {
            return Session[SESSION_OPERADOR];
        }

        #endregion

        #region Sessão Url Redirecionamento

        private const String URL_REDIRECT = "UrlRedirecionamento";

        public void Sessao_SalvarUrlRedirecionamento(String url)
        {
            Session[URL_REDIRECT] = url;
        }

        public String Sessao_ConsultarUrlRedirecionamento()
        {
            String retorno = "";
            if (Session[URL_REDIRECT] != null)
            {
                retorno = (String)Session[URL_REDIRECT];
            }
            return retorno;
        }

        #endregion

        #region Sessão Pedido sendo pago

        private const String URL_PEDIDOSENDOPAGO = "PedidoSendoPago";

        public void Sessao_SalvarPedidoSendoPago(String idPedido)
        {
            Session[URL_PEDIDOSENDOPAGO] = idPedido;
        }

        public String Sessao_ConsultarPedidoSendoPago()
        {
            String retorno = "";
            if (Session[URL_PEDIDOSENDOPAGO] != null)
            {
                retorno = (String)Session[URL_PEDIDOSENDOPAGO];
            }
            return retorno;
        }

        #endregion

    }
}
