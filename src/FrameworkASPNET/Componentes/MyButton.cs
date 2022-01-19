using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FrameworkASPNET.Componentes
{
    [Bindable(true)]
    [DefaultProperty("Text")]
    [ToolboxData("<{0}:MyButton runat=server />")]
    public class MyButton : Button
    {
        #region Propriedades

        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue(false)]
        [Localizable(true)]
        public bool IsConfirmButton
        {
            get
            {
                Object o = ViewState["IsConfirmButton"];
                return (o != null) ? (bool)o : false;
            }

            set
            {
                ViewState["IsConfirmButton"] = value;
            }
        }


        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        [Localizable(true)]
        public string ConfirmMessage
        {
            get
            {
                Object o = ViewState["ConfirmMessage"];
                return (o != null) ? (string)o : "";
            }

            set
            {
                ViewState["ConfirmMessage"] = value;
            }
        }
        #endregion Propriedades

        protected override void AddAttributesToRender(HtmlTextWriter writer)
        {
            if (IsConfirmButton)
            {
                string strMensagem = ConfirmMessage;

                strMensagem = strMensagem.Replace(@"""", @"&quot;");

                writer.AddAttribute("onclick", "javascript: return confirm(&quot;" + strMensagem + "&quot;)", false);
            }

            base.AddAttributesToRender(writer);
        }                
    }
}
