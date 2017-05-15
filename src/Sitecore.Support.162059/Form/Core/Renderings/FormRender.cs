using Sitecore;
using Sitecore.Collections;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Form.Core.Configuration;
using Sitecore.Form.Core.Pipelines.RenderForm;
using Sitecore.Forms.Core.Data;
using Sitecore.Layouts;
using Sitecore.Pipelines;
using Sitecore.Pipelines.ExecutePageEditorAction;
using Sitecore.Sites;

namespace Sitecore.Support.Form.Core.Renderings
{
    public class FormRender : Sitecore.Web.UI.WebControl, IPageEditorActionHandler
    {
        private readonly SafeDictionary<string> _renderParameters = new SafeDictionary<string>();

        private Sitecore.Support.Form.Web.UI.Controls.SitecoreSimpleForm form;

        private string formID;

        public bool DisableWebEditing
        {
            get;
            set;
        }

        public string FormID
        {
            get
            {
                if (!string.IsNullOrEmpty(this.DataSource))
                {
                    Item item = StaticSettings.ContextDatabase.GetItem(this.DataSource);
                    if (item != null)
                    {
                        return item.ID.ToString();
                    }
                }
                return this.formID;
            }
            set
            {
                this.formID = value;
            }
        }

        public System.Web.UI.UserControl FormInstance
        {
            get
            {
                return this.form;
            }
        }

        public string FormTemplate
        {
            get;
            set;
        }

        public bool IsClearDepend
        {
            get;
            set;
        }

        public bool IsFastPreview
        {
            get;
            set;
        }

        public Item Item
        {
            get
            {
                if (!string.IsNullOrEmpty(this.FormID))
                {
                    return StaticSettings.ContextDatabase.GetItem(this.FormID);
                }
                return null;
            }
        }

        public string ReadQueryString
        {
            get;
            set;
        }

        protected string ActionName
        {
            get;
            private set;
        }

        protected RenderingReference RenderingReference
        {
            get;
            private set;
        }

        public FormRender()
        {
        }

        public FormRender(string tag) : base(tag)
        {
        }

        public void InitControls()
        {
            if (this.Item != null)
            {
                this.OnInit(null);
                if (this.form != null)
                {
                    this.form.Initialize();
                }
            }
        }

        public RenderFormResult RenderForm()
        {
            Item item = this.Item;
            if (item == null)
            {
                return new RenderFormResult();
            }
            RenderFormArgs renderFormArgs = new RenderFormArgs(item)
            {
                Parameters = Sitecore.Web.WebUtil.ParseQueryString(this.Parameters),
                DisableWebEdit = this.DisableWebEditing
            };
            if (this.form != null)
            {
                System.Web.UI.HtmlTextWriter htmlTextWriter = new System.Web.UI.HtmlTextWriter(new System.IO.StringWriter());
                this.form.DisableWebEditing = this.DisableWebEditing;
                this.form.Parameters = this.Parameters;
                this.form.ReadQueryString = MainUtil.GetBool(this.ReadQueryString, false);
                this.form.FastPreview = this.IsFastPreview;
                if (!string.IsNullOrEmpty(this.CssClass))
                {
                    this.form.CssClass = this.CssClass;
                }
                if (this.RenderingReference != null && this.ActionName == "insert")
                {
                    System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
                    stringBuilder.Append("var element = $sc('.sc-webform-openeditor');");
                    stringBuilder.Append("if (element.length > 0) {");
                    stringBuilder.Append("element.parents('.scPageDesignerControl:first').css('opacity', '1');");
                    stringBuilder.Append("element.remove();");
                    stringBuilder.AppendFormat("Sitecore.PageModes.PageEditor.postRequest('forms:edit(checksave=0,renderingId={0},referenceId={1},id={2})', null, true);", this.RenderingReference.RenderingID, this.RenderingReference.UniqueId, this.FormID);
                    stringBuilder.Append("}");
                    this.form.Controls.Add(new System.Web.UI.WebControls.Literal
                    {
                        Text = "<img class='sc-webform-openeditor' src='/sitecore/images/blank.gif' style='display:none' width='1' height='1' onload=\"" + stringBuilder + "\">"
                    });
                }
                this.form.RenderControl(htmlTextWriter);
                RenderFormResult expr_184 = renderFormArgs.Result;
                expr_184.FirstPart += htmlTextWriter.InnerWriter.ToString();
            }
            return renderFormArgs.Result;
        }

        public void ActionExecuted(RenderingReference renderingReference, string actionName)
        {
            this.RenderingReference = renderingReference;
            this.ActionName = actionName;
        }

        internal static FormRender GetRender(Sitecore.Support.Form.Web.UI.Controls.SitecoreSimpleForm form)
        {
            return new FormRender
            {
                FormID = form.FormID.ToString(),
                DataSource = form.FormID.ToString(),
                CssClass = form.CssClass,
                DisableWebEditing = form.DisableWebEditing,
                Parameters = form.Parameters,
                IsFastPreview = form.FastPreview,
                IsClearDepend = form.IsClearDepend,
                ReadQueryString = (form.ReadQueryString ? "1" : "0"),
                FormTemplate = form.AppRelativeVirtualPath
            };
        }

        protected override void DoRender(System.Web.UI.HtmlTextWriter output)
        {
            Assert.ArgumentNotNull(output, "output");
            if (this.form == null && this.Controls.Count == 0)
            {
                output.Write(this.RenderForm().ToString());
            }
            if (this.form != null)
            {
                if (this.IsFastPreview)
                {
                    output.Write("<div align=\"center\" style=\"width=100%;height=100%\">");
                    output.Write("<div class=\"disabledFormOut\">");
                    output.Write("<div class=\"disabledFormIn\">");
                }
                string value = this.RenderForm().ToString();
                output.Write(value);
                if (this.IsFastPreview)
                {
                    output.Write("</div>");
                    output.Write("</div>");
                    output.Write("</div>");
                    return;
                }
            }
            else if (this.Controls.Count > 0)
            {
                this.Controls[0].RenderControl(output);
            }
        }

        protected override void OnInit(System.EventArgs e)
        {
            if (this.FormID != null && this.Item != null)
            {
                string iD = "form_" + this.Item.ID.ToShortID();
                if (!string.IsNullOrEmpty(this.FormTemplate))
                {
                    try
                    {
                        this.form = (Sitecore.Support.Form.Web.UI.Controls.SitecoreSimpleForm)Sitecore.Form.Core.Utility.WebUtil.CreateUserControl(this.Page, this.FormTemplate);
                        this.form.FormItem = new FormItem(this.Item);
                    }
                    catch (System.Exception exception)
                    {
                        Log.Warn("Invalid form template", exception, this);
                    }
                }
                if (this.form == null)
                {
                    this.form = new Sitecore.Support.Form.Web.UI.Controls.SitecoreSimpleForm(this.Item);
                }
                if (!string.IsNullOrEmpty(this.CssClass))
                {
                    this.form.CssClass = this.CssClass;
                }
                this.form.ID = iD;
                this.form.DisableWebEditing = this.DisableWebEditing;
                this.form.Parameters = this.Parameters;
                this.form.FastPreview = this.IsFastPreview;
                this.form.IsClearDepend = this.IsClearDepend;
                this.form.ReadQueryString = MainUtil.GetBool(this.ReadQueryString, false);
                this.Controls.Add(this.form);
                this.form.Page = this.Page;
            }
            base.OnInit(e);
        }

        protected override void OnPreRender(System.EventArgs e)
        {
            base.OnPreRender(e);
            if (Sitecore.Context.Site.DisplayMode == DisplayMode.Edit)
            {
                Item item = this.Item;
                if (item != null)
                {
                    RenderFormArgs renderFormArgs = new RenderFormArgs(item);
                    renderFormArgs.Parameters = Sitecore.Web.WebUtil.ParseQueryString(this.Parameters);
                    renderFormArgs.DisableWebEdit = this.DisableWebEditing;
                    using (new LongRunningOperationWatcher(Sitecore.Configuration.Settings.Profiling.RenderFieldThreshold, "preRenderForm pipeline[id={0}]", new string[]
                    {
                        item.ID.ToString()
                    }))
                    {
                        CorePipeline.Run("preRenderForm", renderFormArgs);
                    }
                }
            }
        }
    }
}