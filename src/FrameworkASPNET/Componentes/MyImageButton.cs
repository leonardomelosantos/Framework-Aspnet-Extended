using System;
using System.Web;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.ComponentModel;
using System.Web.UI.WebControls;

namespace FrameworkASPNET.Componentes
{
    [DefaultProperty("ImageUrl")]
    [ToolboxData("<{0}:MyImageButton runat=server />")]
    public class MyImageButton : ImageButton
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

        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        [Localizable(true)]
        public string OnMouseOverImageUrl
        {
            get
            {
                Object o = ViewState["OnMouseOverImageUrl"];
                return (o != null) ? (string)o : "";
            }

            set
            {
                ViewState["OnMouseOverImageUrl"] = value;
            }
        }


        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        [Localizable(true)]
        public string OnMouseOutImageUrl
        {
            get
            {
                Object o = ViewState["OnMouseOutImageUrl"];
                return (o != null) ? (string)o : "";
            }

            set
            {
                ViewState["OnMouseOutImageUrl"] = value;
            }
        }


        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        [Localizable(true)]
        public string DisablesImageUrl
        {
            get
            {
                Object o = ViewState["DisabledImageUrl"];
                return (o != null) ? (string)o : "";
            }

            set
            {
                ViewState["DisabledImageUrl"] = value;
            }
        }

        #endregion Propriedades

        #region Métodos OverRide

        protected override void AddAttributesToRender(HtmlTextWriter writer)
        {
            if (IsConfirmButton)
            {
                string strMensagem = ConfirmMessage;

                strMensagem = strMensagem.Replace(@"""", @"&quot;");

                writer.AddAttribute("onclick", "javascript: return confirm(&quot;" + strMensagem + "&quot;)", false);
            }

            if (OnMouseOverImageUrl.Trim().Length > 0)
            {
                writer.AddAttribute("OnMouseOver", "this.src='" + OnMouseOverImageUrl + "'", false);

                if (OnMouseOutImageUrl.Trim().Length == 0)
                {
                    writer.AddAttribute("OnMouseOut", "this.src='" + ImageUrl + "'", false);
                }
                else
                {
                    writer.AddAttribute("OnMouseOut", "this.src='" + OnMouseOutImageUrl + "'", false);
                }
            }

            base.AddAttributesToRender(writer);
        }

        public override string ImageUrl
        {
            get
            {
                if (DesignMode || Enabled || (DisablesImageUrl == ""))
                {
                    return base.ImageUrl;
                }
                else
                {
                    return DisablesImageUrl;
                }
            }
            set
            {
                base.ImageUrl = value;
            }
        }

        #endregion
        
    }
}
