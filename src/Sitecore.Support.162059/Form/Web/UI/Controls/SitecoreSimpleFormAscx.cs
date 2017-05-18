using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Form.Core.Ascx.Controls;
using Sitecore.Form.Core.Attributes;
using Sitecore.Form.Core.Configuration;
using Sitecore.Form.Core.Utility;
using Sitecore.Form.Web.UI.Controls;
using Sitecore.Forms.Core.Rules;
using System;
using System.Linq;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Sitecore.Support.Form.Web.UI.Controls
{
    [PersistChildren(true), ToolboxData("<div runat=\"server\"></div>"), Dummy]
    public class SitecoreSimpleFormAscx : Sitecore.Form.Web.UI.Controls.SitecoreSimpleForm
    {
        public SitecoreSimpleFormAscx() : base() { }
        public SitecoreSimpleFormAscx(Item item) : base(item) { }
        // the overriden method
        protected new void Expand()
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
        [Obsolete("Use SubmitSummary")]
        protected override Label Error
        {
            get
            {
                return null;
            }
        }
        protected override Control FieldContainer
        {
            get
            {
                return base.fieldContainer;
            }
        }
        protected override FormFooter Footer
        {
            get
            {
                return base.footer;
            }
        }
        protected override FormIntroduction Intro
        {
            get
            {
                return base.intro;
            }
        }
        protected override FormSubmit Submit
        {
            get
            {
                return base.submit;
            }
        }
        protected override Sitecore.Form.Web.UI.Controls.SubmitSummary SubmitSummary
        {
            get
            {
                return base.submitSummary;
            }
        }
        protected override FormTitle Title
        {
            get
            {
                return base.title;
            }
        }

        protected override void OnInit(EventArgs e)
        {
            Assert.IsNotNull(base.FormItem, "FormItem");
            if (this.Page == null)
            {
                this.Page = WebUtil.GetPage();
                ReflectionUtils.SetField(typeof(Page), this.Page, "_enableEventValidation", false);
            }
            this.Page.EnableViewState = true;
            ThemesManager.RegisterCssScript(this.Page, base.FormItem.InnerItem, Sitecore.Context.Item);
            // Use reflection to initialize object
            PropertyInfo _formTitle = typeof(FormTitle).GetProperty("Item");
            _formTitle.SetValue(Title, base.FormItem.InnerItem);
            //base.title.Item = base.FormItem.InnerItem;
            base.title.SetTagKey(base.FormItem.TitleTag);
            base.title.DisableWebEditing = base.DisableWebEditing;
            base.title.Parameters = base.Parameters;
            base.title.FastPreview = base.FastPreview;
            // Use reflection to initialize object
            PropertyInfo _formIntroduction = typeof(FormIntroduction).GetProperty("Item");
            _formIntroduction.SetValue(Intro, base.FormItem.InnerItem);
            //base.intro.Item = base.FormItem.InnerItem;
            base.intro.DisableWebEditing = base.DisableWebEditing;
            base.intro.Parameters = base.Parameters;
            base.intro.FastPreview = base.FastPreview;
            // Use reflection to initialize object
            PropertyInfo _formSubmit = typeof(FormSubmit).GetProperty("Item");
            _formSubmit.SetValue(Submit, base.FormItem.InnerItem);
            //base.submit.Item = base.FormItem.InnerItem;
            base.submit.ID = this.ID + "_submit";
            base.submit.DisableWebEditing = base.DisableWebEditing;
            base.submit.Parameters = base.Parameters;
            base.submit.FastPreview = base.FastPreview;
            base.submit.ValidationGroup = base.submit.ID;
            base.submit.Click += new EventHandler(this.OnClick);
            if (base.FastPreview)
            {
                base.summary.Visible = false;
            }
            base.summary.ID = SimpleForm.prefixSummaryID;
            base.summary.ValidationGroup = base.submit.ID;
            base.submitSummary.ID = this.ID + SimpleForm.prefixErrorID;
            // Invoke overriden method to applay default css class for section
            Expand();
            // Use reflection to initialize object
            PropertyInfo _formFooter = typeof(FormFooter).GetProperty("Item");
            _formFooter.SetValue(Footer, base.FormItem.InnerItem);
            //base.footer.Item = base.FormItem.InnerItem;
            base.footer.DisableWebEditing = base.DisableWebEditing;
            base.footer.Parameters = base.Parameters;
            base.footer.FastPreview = base.FastPreview;
            base.EventCounter.ID = this.ID + SimpleForm.prefixEventCountID;
            this.Controls.Add(base.EventCounter);
            base.AntiCsrf.ID = this.ID + SimpleForm.PrefixAntiCsrfId;
            this.Controls.Add(base.AntiCsrf);
            object sessionValue = SessionUtil.GetSessionValue<object>(base.AntiCsrf.ID);
            if (sessionValue == null)
            {
                sessionValue = Guid.NewGuid().ToString();
                SessionUtil.SetSessionValue(base.AntiCsrf.ID, sessionValue);
            }
            if (!base.IsPostBack || !base.Request.Form.AllKeys.Any<string>(k => ((k != null) && k.Contains(base.submit.ID))))
            {
                base.AntiCsrf.Value = sessionValue.ToString();
            }
        }
    }
}