using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Drawing;
using System.Data;

namespace FrameworkASPNET.Componentes
{
    [DefaultProperty("Text")]
    [ToolboxData(@"<{0}:MyGridView AllowPaging=""True"" HabilitarSelecao=""True"" runat=server Width=""100%""><PagerSettings Mode=""NumericFirstLast""/></{0}:MyGridView>")]
    public class MyGridView : GridView
    {
        private const string COR_MOUSEOVER = "corMor";
        private const string INDICE_CORRENTE = "indCor";
        private const string INDICE_COM_PAGINACAO = "indicePaginacao";
        private const string GRID_SEL = "GRID_SEL";
        private bool exibirColunasTemplates = true;

        protected override void InitializeRow(GridViewRow row, DataControlField[] fields)
        {
            base.InitializeRow(row, fields);
        }

        /// <summary>
        /// Mudar a paginação do grid
        /// resetar o item selecionado
        /// </summary>
        /// <param name="e">evento de paginação</param>
        protected override void OnPageIndexChanging(GridViewPageEventArgs e)
        {
            this.PageIndex = e.NewPageIndex;
            this.SelectedIndex = -1;
            this.IndiceCorrente = -1;
            base.OnPageIndexChanging(e);
        }

        /// <summary>
        /// Informar que a propriedade não deverá ser utilizada
        /// </summary>
        [Obsolete("Não utilizar esta propriedade. Utilizar a propriedade 'IndiceSelecionado'", true)]
        public override int SelectedIndex
        {
            get
            {
                return base.SelectedIndex;
            }
            set
            {
                base.SelectedIndex = value;
            }
        }

        /// <summary>
        /// Exibir o indice do item que foi selecionado
        /// calculado junto com a paginação
        /// </summary>
        [Browsable(false)]
        public int IndiceSelecionado
        {
            get
            {
                int indice = -1;
                if (IndiceCorrente != -1)
                {
                    indice = (base.PageIndex * base.PageSize) + IndiceCorrente;
                }
                return indice;
            }
            set
            {
                base.SelectedIndex = -1;
                if (value != -1)
                {
                    base.PageIndex = value / base.PageSize;
                    base.SelectedIndex = (value % base.PageSize);
                }
                IndiceCorrente = base.SelectedIndex;
            }
        }

        private int IndiceCorrente
        {
            get
            {
                if (ViewState[INDICE_CORRENTE] == null)
                {
                    ViewState[INDICE_CORRENTE] = -1;
                }
                return (int)ViewState[INDICE_CORRENTE];
            }
            set
            {
                ViewState[INDICE_CORRENTE] = value;
            }
        }
        
        /// <summary>
        /// Cor do item quando o mouse passar por cima
        /// </summary>
        [Browsable(true)]
        public Color CorMouseOver
        {
            get
            {
                if (ViewState[COR_MOUSEOVER] == null)
                {
                    ViewState[COR_MOUSEOVER] = Color.Empty;
                }
                return (Color)ViewState[COR_MOUSEOVER];
            }
            set
            {
                ViewState[COR_MOUSEOVER] = value;
            }
        }

        /// <summary>
        /// Se o grid deve estar com a seleção de linha habilitada.
        /// </summary>
        [DefaultValue(true)]
        [Browsable(true)]
        public bool HabilitarSelecao
        {
            get
            {
                if (ViewState["HABILITARSELECAO"] == null)
                {
                    ViewState["HABILITARSELECAO"] = true;
                }
                return (bool)ViewState["HABILITARSELECAO"];
            }
            set
            {
                ViewState["HABILITARSELECAO"] = value;
            }
        }

        /// <summary>
        /// Mudar a cor do item quando o mouse passa por cima para a cor de selecionado
        /// </summary>
        /// <param name="e"></param>
        protected override void OnSelectedIndexChanging(GridViewSelectEventArgs e)
        {            
            this.IndiceCorrente = e.NewSelectedIndex;
            base.OnSelectedIndexChanging(e);
        }

        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            this.IndiceCorrente = base.SelectedIndex;
            base.OnSelectedIndexChanged(e);
        }

        /// <summary>
        /// Adicionar para todo o item a opção de seleção (postback)
        /// informar a cor do item quando o mouse passa por cima
        /// </summary>
        /// <param name="e"></param>
        protected override void OnRowDataBound(GridViewRowEventArgs e)
        {
            if (exibirColunasTemplates)
            {
                base.OnRowDataBound(e);
            }
        }

        protected override void OnRowCreated(GridViewRowEventArgs e)
        {
            if (HabilitarSelecao)
            {
                if (e.Row.RowType == DataControlRowType.DataRow)
                {
                    e.Row.Attributes["OnClick"] = "javascript:__doPostBack('" + this.UniqueID + "','Select$" + e.Row.RowIndex + "')";
                    e.Row.Attributes.Add("onMouseOver", "selecionaGrid(this)");
                    e.Row.Attributes.Add("onMouseOut", "deSelecionaGrid(this)");
                    e.Row.Style["Cursor"] = "hand";
                }
            }

            base.OnRowCreated(e);
        }


        /// <summary>
        /// 
        /// </summary>
        public void Limpar()
        {
            try
            {
                DataTable dataTable;
                DataRow dataRow;

                dataTable = new DataTable();

                foreach (DataControlField colColuna in this.Columns)
                {
                    dataTable.Columns.Add(colColuna.HeaderText);
                }

                dataRow = dataTable.NewRow();
                dataTable.Rows.Add(dataRow);
                exibirColunasTemplates = true;
                base.DataSource = dataTable;
                base.DataBind();
                IndiceSelecionado = -1;
                if (base.Rows.Count > 0)
                {
                    base.Rows[0].Visible = false;
                }
                exibirColunasTemplates = true;
            }
            catch { }
        }


        protected override void OnPreRender(EventArgs e)
        {
            if (HabilitarSelecao)
            {
                if (!this.Page.ClientScript.IsClientScriptBlockRegistered(GRID_SEL))
                {
                    this.Page.ClientScript.RegisterClientScriptBlock(typeof(string), GRID_SEL, ScriptSelecao());
                }
            }
            base.OnPreRender(e);
        }

        private string ScriptSelecao()
        {
            StringBuilder retorno;
            retorno = new StringBuilder();
            retorno.Append("<script language=javascript>");
            retorno.Append("var cor;");
            retorno.Append("function selecionaGrid(elemento)");
            retorno.Append("{ ");
            retorno.Append("cor=elemento.style.backgroundColor;");
            retorno.Append(@"elemento.style.backgroundColor = """ + System.Drawing.ColorTranslator.ToHtml(CorMouseOver) + @""";");
            retorno.Append("}");
            retorno.Append("function deSelecionaGrid(elemento)");
            retorno.Append("{ ");
            retorno.Append("elemento.style.backgroundColor = cor;");
            retorno.Append("}");
            retorno.Append(@"</script>");
            return retorno.ToString();
        }

        public int IndiceComPaginacao(int indice, int pageIndexAnterior)
        {
            int index = indice;

            if (this.AllowPaging && this.PageCount > 1 && pageIndexAnterior > 0)
            {
                index = (pageIndexAnterior * this.PageSize) + indice;
            }

            return index;
        }
    }
}
