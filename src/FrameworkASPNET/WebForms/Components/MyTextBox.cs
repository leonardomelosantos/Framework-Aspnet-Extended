using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using FrameworkDotNetExtended.Extensions;

namespace FrameworkAspNetExtended.WebForms.Componentes
{
    [DefaultProperty("Text")]
    [ToolboxData("<{0}:MyTextBox runat=server></{0}:MyTextBox>")]
    public class MyTextBox : TextBox
    {

        #region Variáveis

        private int qtdCaracteresMascara;

        #endregion Variáveis

        #region Propriedades

        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue(false)]
        [Localizable(true)]
        public bool KeyUpBlur
        {
            get
            {
                Object o = ViewState["KeyUpBlur"];
                return (o != null) ? (bool)o : false;
            }
            set
            {
                ViewState["KeyUpBlur"] = value;
            }
        }

        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue(CaseType.NONE)]
        [Localizable(true)]
        public CaseType CaseType
        {
            get
            {
                Object o = ViewState["CaseType"];
                return (o != null) ? (CaseType)o : CaseType.NONE;
            }
            set
            {
                ViewState["CaseType"] = value;
            }
        }

        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue(CaracterType.NONE)]
        [Localizable(true)]
        public CaracterType CaracterType
        {
            get
            {
                Object o = ViewState["CaracterType"];
                return (o != null) ? (CaracterType)o : CaracterType.NONE;
            }
            set
            {
                ViewState["CaracterType"] = value;
            }
        }

        /// <summary>
        /// A máscara só poderá ser utilizada caso o CaracterType não seja dos tipos: Moeda, Data e Percentual
        /// </summary>
        [Bindable(true)]
        [Category("Appearance")]                        
        [Description("Máscara do textbox. # -> Letra $ -> Número * -> Ambos")]
        [Localizable(true)]
        public string Mascara
        {
            get
            {
                Object o = ViewState["Mascara"];
                return (o != null) ? (String)o : string.Empty;
            }
            set
            {
                qtdCaracteresMascara = MascaraSemFormatacao(value).Trim().Length;
                if (qtdCaracteresMascara > 0)
                {
                    ViewState["Mascara"] = ValidarMascara(value);
                }
                else
                {
                    ViewState["Mascara"] = string.Empty;
                }                
            }
        }

        /// <summary>
        /// Informar que a propriedade não deverá ser utilizada
        /// </summary>        
        public override string Text
        {
            get
            {                
                return base.Text.Replace("'", "").Replace("\"", "");
            }
            set
            {
                base.Text = value;
            }
        }
        
        public string TextNoMask
        {
            get
            {
                string texto = Text.ApenasNumerosOuTexto(TipoCaracter.LetraNúmero);
                return texto;
            }            
        }

        /// <summary>
        /// Símbolo usado para fazer a separação dos milésimos
        /// </summary>
        [Bindable(true)]        
        [Description("Símbolo usado para fazer a separação dos milésimos")]
        [Localizable(true)]
        public string SeparadorMilesimo
        {
            get
            {
                Object o = ViewState["SeparadorMilesimo"];
                return (o != null) ? (String)o : ".";
            }
            set
            {             
                ViewState["SeparadorMilesimo"] = value;             
            }
        }

        /// <summary>
        /// Símbolo usado para fazer a separação dos decimais
        /// </summary>
        [Bindable(true)]
        [Description("Símbolo usado para fazer a separação dos decimais")]
        [Localizable(true)]
        public string SeparadorDecimal
        {
            get
            {
                Object o = ViewState["SeparadorDecimal"];
                return (o != null) ? (String)o : ",";
            }
            set
            {
                ViewState["SeparadorDecimal"] = value;
            }
        }

        #endregion Propriedades
                
        #region Métodos

        #region Renderização

        protected override void AddAttributesToRender(HtmlTextWriter writer)
        {
            string onBlur = string.Empty;
            string onKeyUp = string.Empty;
            string onKeyPress = string.Empty;
            string onKeyDown = string.Empty;

            if ((CaracterType == CaracterType.NONE || CaracterType == CaracterType.LETRAS) && 
                (TextMode != TextBoxMode.Password))
            {
                //Adiciona o estilo e a função javascript da caixa do texto
                if (CaseType == CaseType.UPPER_CASE)
                {
                    base.Attributes.CssStyle.Add("text-transform", "uppercase");
                    onBlur += "CaixaAlta(this);";
                }
                else
                {
                    if (CaseType == CaseType.LOWER_CASE)
                    {
                        base.Attributes.CssStyle.Add("text-transform", "lowercase");
                        onBlur += "CaixaBaixa(this);";
                    }
                }
            }
            
            if (qtdCaracteresMascara > 0)
            {
                if (KeyUpBlur)
                {
                    onBlur += "FormatarTextoUp(this, '" + Mascara + "', " + qtdCaracteresMascara + ");";
                }
                else
                {
                    onKeyUp += "FormatarTextoUp(this, '" + Mascara + "', " + qtdCaracteresMascara + ");";
                }
                onKeyPress += "return(FormatarTexto('" + Mascara + "', event))";
            }
            else
            {
                //Adiciona ao textbox a função javascript que vai controlar o limite de caracters do MultiLine
                if (this.TextMode == TextBoxMode.MultiLine)
                {
                    onKeyPress += "return LimiteTextBoxMultiline(event, this, " + this.MaxLength + ");";
                    onKeyDown += "return LimiteTextBoxMultiline(event, this, " + this.MaxLength + ");";
                    onBlur += "ValidaLimiteTextBoxMultiline(this, " + this.MaxLength + "); ";
                }

                //Adiciona o javascript de bloqueio de caracteres
                if ((CaracterType == CaracterType.NÚMEROS) || (CaracterType == CaracterType.MOEDA))
                {                    
                    if (CaracterType == CaracterType.MOEDA)
                    {
                        onKeyPress += "return(MascaraMoeda(this,'" + SeparadorMilesimo + "','" +
                            SeparadorDecimal + "',event))";
                        onBlur += "ValidarMoeda(this, '" + SeparadorDecimal + "', '" + SeparadorMilesimo + "');";
                    }
                    else
                    {
                        onKeyPress += "return BlockCaracterNaoNumerico(event);";
                        onBlur += "ValidarNumero(this);";
                    }                    
                }
                else
                {
                    if (CaracterType == CaracterType.LETRAS)
                    {
                        onKeyPress += "return BlockCaracterNumerico(event);";
                        onBlur += "ValidarTexto(this);";
                    }
                }
            }

            if (onBlur.Length > 0)
                Attributes.Add("OnBlur", onBlur);

            if (onKeyDown.Length > 0)
                Attributes.Add("OnKeyDown", onKeyDown);

            if (onKeyPress.Length > 0)
                Attributes.Add("OnKeyPress", onKeyPress);

            if (onKeyUp.Length > 0)
                Attributes.Add("OnKeyUp", onKeyUp);

            base.AddAttributesToRender(writer);
        }

        protected override void OnPreRender(EventArgs e)
        {
            if (CaracterType == CaracterType.NONE || CaracterType == CaracterType.LETRAS)
            {
                //Registra a função de caixa da letra apenas se o CaracterType tiver letras
                if ((CaracterType == CaracterType.NONE) || (CaracterType == CaracterType.LETRAS))
                {
                    string script = string.Empty;
                    string key = string.Empty;

                    if (CaseType == CaseType.UPPER_CASE)
                    {
                        script = ScriptCaixaAlta();
                        key = "CAIXA_ALTA_SCRIPT";
                    }
                    else
                    {
                        if (CaseType == CaseType.LOWER_CASE)
                        {
                            script = ScriptCaixaBaixa();
                            key = "CAIXA_BAIXA_SCRIPT";
                        }
                    }
                    if (!this.Page.ClientScript.IsClientScriptBlockRegistered(key))
                    {
                        this.Page.ClientScript.RegisterClientScriptBlock(typeof(string), key, script);
                    }
                }
            }

            if (qtdCaracteresMascara > 0)
            {
                if (!this.Page.ClientScript.IsClientScriptBlockRegistered("MASCARA_SCRIPT"))
                {
                    string script = ScriptMascara();
                    this.Page.ClientScript.RegisterClientScriptBlock(typeof(string), "MASCARA_SCRIPT", script);
                }
                this.MaxLength = Mascara.Length;
            }
            else
            {
                if (this.TextMode == TextBoxMode.MultiLine)
                {
                    if (!this.Page.ClientScript.IsClientScriptBlockRegistered("LIMITE_MULTILINE_SCRIPT"))
                    {
                        this.Page.ClientScript.RegisterClientScriptBlock(typeof(string),
                            "LIMITE_MULTILINE_SCRIPT", LimiteTextBoxMultiline());
                    }
                }

                if (CaracterType == CaracterType.LETRAS)
                {
                    if (!this.Page.ClientScript.IsClientScriptBlockRegistered("BLOCK_NUMBER_SCRIPT"))
                    {
                        this.Page.ClientScript.RegisterClientScriptBlock(typeof(string),
                            "BLOCK_NUMBER_SCRIPT", ScriptBlockCaracterNumerico());
                    }
                    if (!this.Page.ClientScript.IsClientScriptBlockRegistered("VALIDA_TEXTO_SCRIPT"))
                    {
                        this.Page.ClientScript.RegisterClientScriptBlock(typeof(string),
                            "VALIDA_TEXTO_SCRIPT", ScriptValidaTexto());
                    }
                }
                else
                {

                    if (CaracterType == CaracterType.NÚMEROS)
                    {
                        if (!this.Page.ClientScript.IsClientScriptBlockRegistered("BLOCK_CARACTER_SCRIPT"))
                        {
                            this.Page.ClientScript.RegisterClientScriptBlock(typeof(string),
                                "BLOCK_CARACTER_SCRIPT", ScriptBlockCaracterNaoNumerico());
                        }

                        if (!this.Page.ClientScript.IsClientScriptBlockRegistered("VALIDA_NUMERO_SCRIPT"))
                        {
                            this.Page.ClientScript.RegisterClientScriptBlock(typeof(string),
                                "VALIDA_NUMERO_SCRIPT", ScriptValidaNumero());
                        }
                    }
                    else
                    {
                        if (CaracterType == CaracterType.MOEDA)
                        {
                            if (!this.Page.ClientScript.IsClientScriptBlockRegistered("BLOCK_CARACTER_MOEDA_SCRIPT"))
                            {
                                this.Page.ClientScript.RegisterClientScriptBlock(typeof(string),
                                    "BLOCK_CARACTER_MOEDA_SCRIPT", ScriptMoeda());
                            }
                            if (!this.Page.ClientScript.IsClientScriptBlockRegistered("VALIDA_MOEDA_SCRIPT"))
                            {
                                this.Page.ClientScript.RegisterClientScriptBlock(typeof(string),
                                    "VALIDA_MOEDA_SCRIPT", ScriptValidaMoeda());
                            }
                        }
                    }                    
                }                
            }

            base.OnPreRender(e);
        }

        #endregion Renderização

        #region JavaScripts

        private string ScriptCaixaAlta()
        {
            StringBuilder retorno;
            retorno = new StringBuilder();
            retorno.Append("<script language=javascript>");
            retorno.Append("function CaixaAlta(text) { ");            
            retorno.Append("text.value = text.value.toUpperCase();");            
            retorno.Append(" }");            
            retorno.Append(@"</script>");
            return retorno.ToString();
        }

        private string ScriptCaixaBaixa()
        {
            StringBuilder retorno;
            retorno = new StringBuilder();
            retorno.Append("<script language=javascript>");
            retorno.Append("function CaixaBaixa(text) { ");            
            retorno.Append("text.value = text.value.toLowerCase();");
            retorno.Append(" }");
            retorno.Append(@"</script>");
            return retorno.ToString();
        }

        private string LimiteTextBoxMultiline()
        {
            StringBuilder retorno;
            retorno = new StringBuilder();
            retorno.Append("<script language=javascript>");
            retorno.Append("function LimiteTextBoxMultiline(event, textBox, maxLength) { ");            
            retorno.Append("var retorno = true;");
            retorno.Append("var ctrl = event.ctrlKey;");
            retorno.Append("var tecla = event.keyCode;");
            retorno.Append("var resto = maxLength - textBox.value.length;");
            retorno.Append("var textoCopiado = '';");
            retorno.Append("var browser = navigator.appName;");
            retorno.Append("var textoSelecionado;");
            retorno.Append("switch(browser) { ");
            retorno.Append("case 'Netscape': textoSelecionado = document.getSelection(); break;");
            retorno.Append("case 'Microsoft Internet Explorer': textoSelecionado = document.selection.createRange().text; break;");
            retorno.Append(" } ");
            retorno.Append("if (resto > 0) { ");
            retorno.Append("if (ctrl && ((tecla == 86) || (tecla == 118))) { ");
            retorno.Append("if (window.clipboardData.getData(\"Text\").length > resto) { ");
            retorno.Append("textoCopiado = window.clipboardData.getData(\"Text\").substring(0, resto);");
            retorno.Append("retorno = false;");
            retorno.Append("document.getElementById(textBox.id).value += textoCopiado;");
            retorno.Append("}");
            retorno.Append("}");
            retorno.Append("} else {");
            retorno.Append("if ((tecla < 33 || tecla > 40) && tecla != 45 && tecla != 46 && tecla != 8 && tecla != 16) ");
            retorno.Append("if (textoSelecionado.length <= 0)");
            retorno.Append("retorno = false;");
            retorno.Append("}");
            retorno.Append("return retorno;");            
            retorno.Append(" }");

            retorno.Append("function ValidaLimiteTextBoxMultiline(textBox, maxLength)");
            retorno.Append("{ ");
            retorno.Append("if (textBox.value.length > maxLength) {");
            retorno.Append("textBox.value = textBox.value.substring(0, maxLength);");
            retorno.Append("}");
            retorno.Append(" }");
            retorno.Append(@"</script>");
            return retorno.ToString();
        }
        
        private string ScriptBlockCaracterNaoNumerico()
        {
            StringBuilder retorno;
            retorno = new StringBuilder();
            retorno.Append("<script language=javascript>");
            retorno.Append("function BlockCaracterNaoNumerico(evnt) { ");
            retorno.Append("if (navigator.appName ==\"Microsoft Internet Explorer\"){");
            retorno.Append("if (evnt.keyCode < 48 || evnt.keyCode > 57){");
            retorno.Append("return false;");
            retorno.Append("}");
            retorno.Append("}else{");
            retorno.Append("if ((evnt.charCode < 48 || evnt.charCode > 57) && evnt.keyCode == 0){");
            retorno.Append("return false;");
            retorno.Append("}");
            retorno.Append("}");            
            retorno.Append(" }");
            retorno.Append(@"</script>");
            return retorno.ToString();
        }

        private string ScriptBlockCaracterNumerico()
        {
            StringBuilder retorno;
            retorno = new StringBuilder();
            retorno.Append("<script language=javascript>");
            retorno.Append("function BlockCaracterNumerico(evnt) { ");
            retorno.Append("if (navigator.appName ==\"Microsoft Internet Explorer\"){");
            retorno.Append("if (evnt.keyCode >= 48 && evnt.keyCode <= 57){");
            retorno.Append("return false;");
            retorno.Append("}");
            retorno.Append("}else{");
            retorno.Append("if ((evnt.charCode >= 48 && evnt.charCode <= 57) && evnt.keyCode == 0){");
            retorno.Append("return false;");
            retorno.Append("}");
            retorno.Append("}");            
            retorno.Append(" }");
            retorno.Append(@"</script>");
            return retorno.ToString();
        }

        private string ScriptMoeda()
        {
            StringBuilder retorno;
            retorno = new StringBuilder();
            retorno.Append("<script language=javascript>");
            retorno.Append("function MascaraMoeda(objTextBox, SeparadorMilesimo, SeparadorDecimal, e){");
            retorno.Append("var sep = 0;var key = '';var i = j = 0;var len = len2 = 0;var strCheck = '0123456789';");
            retorno.Append("var aux = aux2 = '';var whichCode = (navigator.appName == \"Microsoft Internet Explorer\") ? e.keyCode : e.charCode;");
            retorno.Append("if (navigator.appName == \"Microsoft Internet Explorer\") {");
            retorno.Append("if (document.selection.createRange().text != '') {");
            retorno.Append("objTextBox.value = '';document.selection.empty();}");
            retorno.Append("}else {if ((objTextBox.selectionStart - objTextBox.selectionEnd) < 0) {");
            retorno.Append("objTextBox.value = '';}}");
            retorno.Append("if (whichCode == 13 || whichCode == 0) { return true };");
            retorno.Append("key = String.fromCharCode(whichCode); if (strCheck.indexOf(key) == -1) {return false };");
            retorno.Append("len = objTextBox.value.length;");
            retorno.Append("for(i = 0; i < len; i++) {");
            retorno.Append("if ((objTextBox.value.charAt(i) != '0') && (objTextBox.value.charAt(i) != SeparadorDecimal)) { break;}}");
            retorno.Append("aux = ''; for(; i < len; i++) {");
            retorno.Append("if (strCheck.indexOf(objTextBox.value.charAt(i))!=-1) { aux += objTextBox.value.charAt(i) }};");
            retorno.Append("aux += key; len = aux.length;");
            retorno.Append("if (len == 0) { objTextBox.value = ''; }");
            retorno.Append("if (len == 1) { objTextBox.value = '0'+ SeparadorDecimal + '0' + aux; }");
            retorno.Append("if (len == 2) { objTextBox.value = '0'+ SeparadorDecimal + aux; }");
            retorno.Append("if (len > 2) { aux2 = '';");
            retorno.Append("for (j = 0, i = len - 3; i >= 0; i--) {");
            retorno.Append("if (j == 3) { aux2 += SeparadorMilesimo; j = 0; }");
            retorno.Append("aux2 += aux.charAt(i); j++; }");
            retorno.Append("objTextBox.value = ''; len2 = aux2.length;");
            retorno.Append("for (i = len2 - 1; i >= 0; i--) { objTextBox.value += aux2.charAt(i); }");
            retorno.Append("objTextBox.value += SeparadorDecimal + aux.substr(len - 2, len); }");
            retorno.Append("return false; }");
            retorno.Append(@"</script>");
            return retorno.ToString();
        }

        private string ScriptMascara()
        {
            StringBuilder retorno;
            retorno = new StringBuilder();

            if (Mascara.Trim().Length > 0)
            {
                retorno.Append("<script language=javascript>");
                retorno.Append("function FormatarTextoUp(txtBox, mascara, tamanho){");
                retorno.Append("var strCheck = '()-_{}[]/;:. ,<>\\|\\'\\\"';");
                retorno.Append("var valorFormatado = '';var texto = txtBox.value;var tipo;var j = 0;var entrou;");
                retorno.Append("var vazio = true;var diferente;");
                retorno.Append("for (x = 0; x < texto.length; x++){");
                retorno.Append("if (strCheck.indexOf(texto.charAt(x)) != -1){");
                retorno.Append("texto = texto.replace(texto.charAt(x), \"\" );x--;}}");
                retorno.Append("for(i = 0; i < texto.length; i++) {");
                retorno.Append("entrou = false;diferente = false;");
                retorno.Append("if (texto.charCodeAt(i) < 48 || texto.charCodeAt(i) > 57){");
                retorno.Append("tipo = 0;}else{tipo = 1;}");
                retorno.Append("if (mascara.charAt(j) == '#' || mascara.charAt(j) == '*') {");
                retorno.Append("if (tipo == 0) {");
                retorno.Append("valorFormatado += texto.charAt(i);vazio = false;diferente = true;}");
                retorno.Append("entrou = true;}else{");
                retorno.Append("if (mascara.charAt(j) == '$') {");
                retorno.Append("if (tipo == 1) {");
                retorno.Append("valorFormatado += texto.charAt(i);vazio = false;diferente = true;}");
                retorno.Append("entrou = true;}}");
                retorno.Append("if (!entrou) {");
                retorno.Append("valorFormatado += mascara.charAt(j);i--;diferente = true;}");
                retorno.Append("if (diferente) {j++;}");
                retorno.Append("if (valorFormatado.length >= mascara.length) {break;}}");
                retorno.Append("if (vazio) {txtBox.value = '';}else{txtBox.value = valorFormatado;}}");

                retorno.Append("function FormatarTexto(mascara, event){");
                retorno.Append("var i, nCount, sValue, fldLen, mskLen, sCod, bolMask;");
                retorno.Append("var strCheck = '()-_{}[]/;:. ,<>\\|\\'\\\"';");
                retorno.Append("var numberCheck = '0123456789';var nTecla;");
                retorno.Append("if (navigator.appName == \"Microsoft Internet Explorer\"){");
                retorno.Append("nTecla = event.keyCode;sValue = event.srcElement.value;");
                retorno.Append("}else{nTecla = event.charCode;sValue = event.target.value;}");
                retorno.Append("if (navigator.appName == \"Microsoft Internet Explorer\") {");
                retorno.Append("if (document.selection.createRange().text != '') {");
                retorno.Append("objTextBox.value = '';document.selection.empty();}");
                retorno.Append("}else {if ((objTextBox.selectionStart - objTextBox.selectionEnd) < 0) {");
                retorno.Append("objTextBox.value = '';}}");
                retorno.Append("var retorno;var boolTam = true;");
                retorno.Append("for (j = 0; j < sValue.length; j++){");
                retorno.Append("if (strCheck.indexOf(sValue.charAt(j)) != -1){");
                retorno.Append("sValue = sValue.toString().replace(sValue.charAt(j), \"\" );j--;}}");
                retorno.Append("fldLen = sValue.length;i = 0;nCount = 0;sCod = \"\";mskLen = fldLen;");
                retorno.Append("while (i <= mskLen){");
                retorno.Append("if (i < mascara.length)	{");
                retorno.Append("bolMask = (strCheck.indexOf(mascara.charAt(i)) != -1);");
                retorno.Append("}else{bolMask = false;}");
                retorno.Append("if (bolMask){sCod += mascara.charAt(i);mskLen++;}");
                retorno.Append("else{sCod += sValue.charAt(nCount);nCount++;}i++;}");
                retorno.Append("if (navigator.appName == \"Microsoft Internet Explorer\"){");
                retorno.Append("event.srcElement.value = sCod;}else{event.target.value = sCod;}");
                retorno.Append("if (nTecla != 8) {");
                retorno.Append("if (boolTam && mskLen > mascara.length - 1){retorno = false;}else{");
                retorno.Append("if (mascara.charAt(i-1) == \"$\"){");
                retorno.Append("retorno = ((nTecla > 47) && (nTecla < 58)); }else{");
                retorno.Append("if (mascara.charAt(i-1) == \"#\"){");
                retorno.Append("retorno = ((nTecla < 47) || (nTecla > 58));}else{");
                retorno.Append("if (mascara.charAt(i-1) == \"*\") { retorno = true; }}}}");
                retorno.Append("}else{retorno = true;}return retorno;}");
                retorno.Append(@"</script>");
            }

            return retorno.ToString();
        }

        private string ScriptValidaMoeda()
        {
            StringBuilder retorno;
            retorno = new StringBuilder();            
            retorno.Append("<script language=javascript>");            
            retorno.Append("function ValidarMoeda(txtBox, sepDecimal, sepMil) {");
            retorno.Append("var strCheck = '1234567890' + sepDecimal + sepMil;var texto = txtBox.value;");
            retorno.Append("for (x = 0; x < texto.length; x++){");
            retorno.Append("if (strCheck.indexOf(texto.charAt(x)) == -1){");
            retorno.Append("texto = texto.replace(texto.charAt(x), \"\" );x--;}}");
            retorno.Append("txtBox.value = texto;}");
            retorno.Append(@"</script>");
            return retorno.ToString();
        }

        private string ScriptValidaTexto()
        {
            StringBuilder retorno;
            retorno = new StringBuilder();            
            retorno.Append("<script language=javascript>");
            retorno.Append("function ValidarTexto(txtBox) {");
            retorno.Append("var strCheck = '1234567890';var texto = txtBox.value;");
            retorno.Append("for (x = 0; x < texto.length; x++){");
            retorno.Append("if (strCheck.indexOf(texto.charAt(x)) != -1){");
            retorno.Append("texto = texto.replace(texto.charAt(x), \"\" );x--;}}");
            retorno.Append("txtBox.value = texto;}");
            retorno.Append(@"</script>");
            return retorno.ToString();
        }

        private string ScriptValidaNumero()
        {
            StringBuilder retorno;
            retorno = new StringBuilder();            
            retorno.Append("<script language=javascript>");
            retorno.Append("function ValidarNumero(txtBox) {");
            retorno.Append("var strCheck = '1234567890';var texto = txtBox.value;");
            retorno.Append("for (x = 0; x < texto.length; x++){");
            retorno.Append("if (strCheck.indexOf(texto.charAt(x)) == -1){");
            retorno.Append("texto = texto.replace(texto.charAt(x), \"\" );x--;}}");
            retorno.Append("txtBox.value = texto;}");
            retorno.Append(@"</script>");
            return retorno.ToString();
        }

        #endregion JavaScripts

        /// <summary>
        /// Método que realiza a validação da máscara passada.
        /// </summary>
        /// <param name="mascara"></param>
        /// <returns></returns>
        protected string ValidarMascara(string mascara)
        {
            string mascaraValidada = string.Empty;
            string check = "()-_{}[]/;:. ,<>\\|#$*";

            foreach (char c in mascara.ToCharArray())
            {
                if (check.LastIndexOf(c) != -1)
                {
                    mascaraValidada += c;
                }
            }

            return mascaraValidada;
        }

        /// <summary>
        /// Método que retira a máscara deixando apenas os tipos de caracteres aceitos.
        /// </summary>
        /// <param name="mascara"></param>
        /// <returns></returns>
        protected string MascaraSemFormatacao(string mascara)
        {
            string retorno = string.Empty;
            string check = "#$*";

            foreach (char c in mascara.ToCharArray())
            {
                if (check.LastIndexOf(c) != -1)
                {
                    retorno += c;
                }
            }

            return retorno;
        }

        #endregion Métodos
    }

    public enum CaseType
    {
        NONE,
        UPPER_CASE,
        LOWER_CASE,
    }

    public enum CaracterType
    {
        NONE,
        LETRAS,
        NÚMEROS,
        MOEDA
    }
}
