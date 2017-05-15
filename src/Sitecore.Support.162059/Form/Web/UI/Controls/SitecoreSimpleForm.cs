using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Form.Core.Ascx.Controls;
using Sitecore.Form.Core.Configuration;
using Sitecore.Form.Core.ContentEditor.Data;
using Sitecore.Form.Core.Data;
using Sitecore.Form.Core.Pipelines.FormSubmit;
using Sitecore.Form.Core.Utility;
using Sitecore.Form.Web.UI.Controls;
using Sitecore.Forms.Core.Data;
using Sitecore.Forms.Core.Rules;
using Sitecore.Pipelines;
using Sitecore.Reflection;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using Sitecore.WFFM.Abstractions.ContentEditor;

namespace Sitecore.Support.Form.Web.UI.Controls
{
    [System.Web.UI.PersistChildren(true), System.Web.UI.ToolboxData("<div runat=\"server\"></div>")]
    public class SitecoreSimpleForm : SimpleForm
    {
        public static readonly string prefixSubmitID = "_submit";

        protected SubmitSummary submitSummary;

        protected FormFooter footer;

        protected FormIntroduction intro;

        protected FormSubmit submit;

        public System.Web.UI.WebControls.ValidationSummary summary;

        protected FormTitle title;

        protected System.Web.UI.WebControls.Panel fieldContainer;

        public FormItem FormItem
        {
            get;
            protected internal set;
        }

        protected virtual System.Web.UI.Control FieldContainer
        {
            get
            {
                return this.fieldContainer;
            }
        }

        protected virtual FormTitle Title
        {
            get
            {
                return this.title;
            }
        }

        protected virtual FormIntroduction Intro
        {
            get
            {
                return this.intro;
            }
        }

        protected virtual FormFooter Footer
        {
            get
            {
                return this.footer;
            }
        }

        protected virtual FormSubmit Submit
        {
            get
            {
                return this.submit;
            }
        }

        protected virtual SubmitSummary SubmitSummary
        {
            get
            {
                return this.submitSummary;
            }
        }

        [System.Obsolete("Use SubmitSummary")]
        protected new virtual System.Web.UI.WebControls.Label Error
        {
            get
            {
                return null;
            }
        }

        public string Class
        {
            get
            {
                return base.Attributes["class"];
            }
            set
            {
                base.Attributes["class"] = value;
            }
        }

        [Browsable(false)]
        public bool IsClearDepend
        {
            get;
            set;
        }

        [Browsable(false)]
        public bool DisableWebEditing
        {
            get;
            set;
        }

        [Browsable(false)]
        public string Parameters
        {
            get;
            set;
        }

        public override ID FormID
        {
            get
            {
                return this.FormItem.ID;
            }
        }

        public bool ReadQueryString
        {
            get;
            set;
        }

        public SitecoreSimpleForm()
        {
        }

        public SitecoreSimpleForm(Item item)
        {
            Assert.IsNotNull(item, "item");
            Assert.IsTrue(item.TemplateID == IDs.FormTemplateID, "This item is not a form");
            this.FormItem = new FormItem(item);
            this.Parameters = string.Empty;
        }

        internal void Initialize()
        {
            this.CallRecursive(this, "OnInit");
            this.CallRecursive(this, "OnLoad");
            this.ClearDepend();
            this.CallRecursive(this, "OnPreRender");
        }

        internal virtual void CallRecursive(System.Web.UI.Control container, string method)
        {
            try
            {
                ReflectionUtil.CallMethod(container, method, true, true, new object[]
                {
                    System.EventArgs.Empty
                });
            }
            catch
            {
            }
            foreach (System.Web.UI.Control container2 in container.Controls)
            {
                this.CallRecursive(container2, method);
            }
        }

        protected override void OnInit(System.EventArgs e)
        {
            if (this.Page == null)
            {
                this.Page = Sitecore.Form.Core.Utility.WebUtil.GetPage();
                ReflectionUtils.SetField(typeof(System.Web.UI.Page), this.Page, "_enableEventValidation", false);
            }
            this.Page.EnableViewState = true;
            base.OnInit(e);
            ThemesManager.RegisterCssScript(this.Page, this.FormItem.InnerItem, Sitecore.Context.Item);
            this.title = new FormTitle(this.FormItem.InnerItem);
            this.title.DisableWebEditing = this.DisableWebEditing;
            this.title.Parameters = this.Parameters;
            this.title.FastPreview = base.FastPreview;
            this.Controls.Add(this.title);
            this.intro = new FormIntroduction(this.FormItem.InnerItem);
            this.intro.DisableWebEditing = this.DisableWebEditing;
            this.intro.Parameters = this.Parameters;
            this.intro.FastPreview = base.FastPreview;
            this.Controls.Add(this.intro);
            this.submit = new FormSubmit(this.FormItem.InnerItem);
            this.submit.ID = this.ID + SitecoreSimpleForm.prefixSubmitID;
            this.submit.DisableWebEditing = this.DisableWebEditing;
            this.submit.Parameters = this.Parameters;
            this.submit.FastPreview = base.FastPreview;
            this.submit.ValidationGroup = this.submit.ID;
            this.submit.Click += new System.EventHandler(this.OnClick);
            if (!base.FastPreview)
            {
                this.summary = new System.Web.UI.WebControls.ValidationSummary();
                this.summary.ID = SimpleForm.prefixSummaryID;
                this.summary.ClientIDMode = ClientIDMode.Predictable;
                this.summary.ValidationGroup = this.submit.ID;
                this.summary.CssClass = "scfValidationSummary";
                this.Controls.Add(this.summary);
            }
            this.submitSummary = new SubmitSummary();
            this.submitSummary.ID = SimpleForm.prefixErrorID;
            this.submitSummary.CssClass = "scfSubmitSummary";
            this.Controls.Add(this.submitSummary);
            this.fieldContainer = new System.Web.UI.WebControls.Panel();
            this.Controls.Add(this.fieldContainer);
            this.Expand();
            this.footer = new FormFooter(this.FormItem.InnerItem);
            this.footer.DisableWebEditing = this.DisableWebEditing;
            this.footer.Parameters = this.Parameters;
            this.footer.FastPreview = base.FastPreview;
            this.Controls.Add(this.footer);
            this.Controls.Add(this.submit);
        }

        protected override void OnLoad(System.EventArgs e)
        {
            base.OnLoad(e);
            this.fieldContainer.DefaultButton = this.Submit.ID;
        }

        protected override void OnPreRender(System.EventArgs e)
        {
            if (this.FindControl("formreference") == null)
            {
                System.Web.UI.WebControls.HiddenField hiddenField = new System.Web.UI.WebControls.HiddenField();
                hiddenField.ID = "formreference";
                hiddenField.Value = this.ID;
                this.Controls.AddAt(0, hiddenField);
            }
            base.OnPreRender(e);
        }

        protected void Expand()
        {
            Item[] sections = this.FormItem.Sections;
            if (sections.Length == 1 && sections[0].TemplateID != IDs.SectionTemplateID)
            {
                Sitecore.Support.Form.Web.UI.Controls.FormSection formSection = new Sitecore.Support.Form.Web.UI.Controls.FormSection(sections[0], this.FormItem[sections[0].ID.ToShortID().ToString()], false, this.Submit.ID, base.FastPreview)
                {
                    ReadQueryString = this.ReadQueryString,
                    DisableWebEditing = this.DisableWebEditing,
                    RenderingParameters = this.Parameters
                };
                ReflectionUtils.SetXmlProperties(formSection, sections[0][Sitecore.Form.Core.Configuration.FieldIDs.FieldParametersID], true);
                ReflectionUtils.SetXmlProperties(formSection, sections[0][Sitecore.Form.Core.Configuration.FieldIDs.FieldLocalizeParametersID], true);
                this.FieldContainer.Controls.Add(formSection);
                return;
            }
            Item[] array = sections;
            for (int i = 0; i < array.Length; i++)
            {
                Item item = array[i];
                Sitecore.Support.Form.Web.UI.Controls.FormSection formSection2 = new Sitecore.Support.Form.Web.UI.Controls.FormSection(item, this.FormItem[item.ID.ToShortID().ToString()], true, this.Submit.ID, base.FastPreview)
                {
                    ReadQueryString = this.ReadQueryString,
                    DisableWebEditing = this.DisableWebEditing
                };
                ReflectionUtils.SetXmlProperties(formSection2, item[Sitecore.Form.Core.Configuration.FieldIDs.FieldParametersID], true);
                ReflectionUtils.SetXmlProperties(formSection2, item[Sitecore.Form.Core.Configuration.FieldIDs.FieldLocalizeParametersID], true);
                Rule.Run(item[Sitecore.Form.Core.Configuration.FieldIDs.ConditionsFieldID], formSection2);
                this.FieldContainer.Controls.Add(formSection2);
            }
        }

        private void ClearDepend()
        {
            if (this.IsClearDepend)
            {
                IListDefinition actionsDefinition = this.FormItem.ActionsDefinition;
                IEnumerable<IGroupDefinition> _group = actionsDefinition.Groups;
                IEnumerable<IListItemDefinition> _list = _group.First().ListItems;

                if (actionsDefinition.Groups.Count() > 0 && _list.Count() > 0)
                {
                    foreach (GroupDefinition current in actionsDefinition.Groups)
                    {
                        foreach (ListItemDefinition current2 in current.ListItems)
                        {
                            Item item = this.FormItem.Database.GetItem(current2.ItemID);
                            if (item != null)
                            {
                                ActionControl child = new ActionControl
                                {
                                    Value = current2.Parameters,
                                    ActionID = item.ID.ToString(),
                                    ID = "a_" + current2.Unicid
                                };
                                this.Controls.Add(child);
                            }
                        }
                    }
                }
                System.Web.UI.WebControls.HiddenField child2 = new System.Web.UI.WebControls.HiddenField
                {
                    Value = System.Web.HttpUtility.HtmlEncode(this.FormItem.SuccessRedirect ? Sitecore.Web.WebUtil.GetFullUrl(this.FormItem.SuccessPage.Url) : this.FormItem.SuccessMessage),
                    ID = this.ID + SimpleForm.prefixSuccessMessageID
                };
                this.Controls.Add(child2);
                return;
            }
            Sitecore.Form.Core.Utility.WebUtil.ExecuteForAllControls(this, delegate (System.Web.UI.Control control)
            {
                if (control is System.Web.UI.WebControls.WebControl)
                {
                    (control as System.Web.UI.WebControls.WebControl).Enabled = false;
                }
                if (control is System.Web.UI.WebControls.BaseValidator)
                {
                    (control as System.Web.UI.WebControls.BaseValidator).Visible = false;
                }
            });
        }

        protected void CollectActions(System.Web.UI.Control source, System.Collections.Generic.List<ActionDefinition> list)
        {
            IListDefinition actionsDefinition = this.FormItem.ActionsDefinition;
            if (actionsDefinition.Groups.Count() <= 0)
            {
                return;
            }
            foreach (GroupDefinition current in actionsDefinition.Groups)
            {
                list.AddRange(from li in current.ListItems
                              select new ActionDefinition(li.ItemID, li.Parameters)
                              {
                                  UniqueKey = li.Unicid
                              });
            }
        }

        protected override void OnRefreshError(string[] messages)
        {
            Assert.ArgumentNotNull(messages, "messages");
            System.Web.UI.Control control = this.FindControl(this.ID + SimpleForm.prefixErrorID);
            if (control != null && control is SubmitSummary)
            {
                SubmitSummary submitSummary = (SubmitSummary)control;
                submitSummary.Messages = messages;
                if (submitSummary.Messages.Length > 0)
                {
                    base.SetFocus(control.ClientID, null);
                }
            }
        }

        protected override void OnSuccessSubmit()
        {
            this.Controls.Clear();
            System.Web.UI.WebControls.Literal literal = new System.Web.UI.WebControls.Literal();
            SubmitSuccessArgs submitSuccessArgs = new SubmitSuccessArgs(this.FormItem);
            CorePipeline.Run("successAction", submitSuccessArgs);
            literal.Text = submitSuccessArgs.Result;
            this.Controls.Add(literal);
            base.SetFocus(this.ID, null);
        }
    }
}