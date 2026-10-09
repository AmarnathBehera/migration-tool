using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(WebChannelSnippetInfo), WebChannelSnippetInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="WebChannelSnippetInfo"/>.
    /// </summary>
    public partial class WebChannelSnippetInfo : AbstractInfo<WebChannelSnippetInfo, IInfoProvider<WebChannelSnippetInfo>>, IInfoWithId, IInfoWithGuid
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdo.webchannelsnippet";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<WebChannelSnippetInfo>), OBJECT_TYPE, "BDO.WebChannelSnippet", "WebChannelSnippetID", null, "WebSettingsGUID", null, null, null, null, null)
        {
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("WebChannelSnippetWebChannelSettingsID", "bdo.webchannelsettings", ObjectDependencyEnum.Required),
            },
        };


        /// <summary>
        /// Web channel snippet ID.
        /// </summary>
        [DatabaseField]
        public virtual int WebChannelSnippetID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(WebChannelSnippetID)), 0);
            set => SetValue(nameof(WebChannelSnippetID), value);
        }


        /// <summary>
        /// Web channel snippet web channel settings ID.
        /// </summary>
        [DatabaseField]
        public virtual int WebChannelSnippetWebChannelSettingsID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(WebChannelSnippetWebChannelSettingsID)), 0);
            set => SetValue(nameof(WebChannelSnippetWebChannelSettingsID), value);
        }


        /// <summary>
        /// Script head key value.
        /// </summary>
        [DatabaseField]
        public virtual string ScriptHeadKeyValue
        {
            get => ValidationHelper.GetString(GetValue(nameof(ScriptHeadKeyValue)), String.Empty);
            set => SetValue(nameof(ScriptHeadKeyValue), value, String.Empty);
        }


        /// <summary>
        /// Marketo munchkin code key value.
        /// </summary>
        [DatabaseField]
        public virtual string MarketoMunchkinCodeKeyValue
        {
            get => ValidationHelper.GetString(GetValue(nameof(MarketoMunchkinCodeKeyValue)), String.Empty);
            set => SetValue(nameof(MarketoMunchkinCodeKeyValue), value, String.Empty);
        }


        /// <summary>
        /// Scriptexecutedwhencookiesareaccepted key value.
        /// </summary>
        [DatabaseField]
        public virtual string ScriptexecutedwhencookiesareacceptedKeyValue
        {
            get => ValidationHelper.GetString(GetValue(nameof(ScriptexecutedwhencookiesareacceptedKeyValue)), String.Empty);
            set => SetValue(nameof(ScriptexecutedwhencookiesareacceptedKeyValue), value, String.Empty);
        }


        /// <summary>
        /// Scriptinbottomof body key value.
        /// </summary>
        [DatabaseField]
        public virtual string ScriptinbottomofBodyKeyValue
        {
            get => ValidationHelper.GetString(GetValue(nameof(ScriptinbottomofBodyKeyValue)), String.Empty);
            set => SetValue(nameof(ScriptinbottomofBodyKeyValue), value, String.Empty);
        }


        /// <summary>
        /// Scriptin topof body key value.
        /// </summary>
        [DatabaseField]
        public virtual string ScriptinTopofBodyKeyValue
        {
            get => ValidationHelper.GetString(GetValue(nameof(ScriptinTopofBodyKeyValue)), String.Empty);
            set => SetValue(nameof(ScriptinTopofBodyKeyValue), value, String.Empty);
        }


        /// <summary>
        /// Scripts topof page head key value.
        /// </summary>
        [DatabaseField]
        public virtual string ScriptsTopofPageHeadKeyValue
        {
            get => ValidationHelper.GetString(GetValue(nameof(ScriptsTopofPageHeadKeyValue)), String.Empty);
            set => SetValue(nameof(ScriptsTopofPageHeadKeyValue), value, String.Empty);
        }


        /// <summary>
        /// Web settings GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid WebSettingsGUID
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(WebSettingsGUID)), Guid.Empty);
            set => SetValue(nameof(WebSettingsGUID), value);
        }


        /// <summary>
        /// Script head inheritfromglobalsettings.
        /// </summary>
        [DatabaseField]
        public virtual bool ScriptHeadInheritfromglobalsettings
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(ScriptHeadInheritfromglobalsettings)), false);
            set => SetValue(nameof(ScriptHeadInheritfromglobalsettings), value);
        }


        /// <summary>
        /// Scripts topof page inheritfromglobalsetting.
        /// </summary>
        [DatabaseField]
        public virtual bool ScriptsTopofPageInheritfromglobalsetting
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(ScriptsTopofPageInheritfromglobalsetting)), false);
            set => SetValue(nameof(ScriptsTopofPageInheritfromglobalsetting), value);
        }


        /// <summary>
        /// Scriptinbottomof body inherit from global.
        /// </summary>
        [DatabaseField]
        public virtual bool ScriptinbottomofBodyInheritFromGlobal
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(ScriptinbottomofBodyInheritFromGlobal)), false);
            set => SetValue(nameof(ScriptinbottomofBodyInheritFromGlobal), value);
        }


        /// <summary>
        /// Scriptin topof body inheritfromglobal.
        /// </summary>
        [DatabaseField]
        public virtual bool ScriptinTopofBodyInheritfromglobal
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(ScriptinTopofBodyInheritfromglobal)), false);
            set => SetValue(nameof(ScriptinTopofBodyInheritfromglobal), value);
        }


        /// <summary>
        /// Scriptexecutedwhencookiesareaccepted inheritfromglobal.
        /// </summary>
        [DatabaseField]
        public virtual bool ScriptexecutedwhencookiesareacceptedInheritfromglobal
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(ScriptexecutedwhencookiesareacceptedInheritfromglobal)), false);
            set => SetValue(nameof(ScriptexecutedwhencookiesareacceptedInheritfromglobal), value);
        }


        /// <summary>
        /// Marketo munchkin code inheritfromglobal.
        /// </summary>
        [DatabaseField]
        public virtual bool MarketoMunchkinCodeInheritfromglobal
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(MarketoMunchkinCodeInheritfromglobal)), false);
            set => SetValue(nameof(MarketoMunchkinCodeInheritfromglobal), value);
        }


        /// <summary>
        /// From email.
        /// </summary>
        [DatabaseField]
        public virtual string FromEmail
        {
            get => ValidationHelper.GetString(GetValue(nameof(FromEmail)), String.Empty);
            set => SetValue(nameof(FromEmail), value, String.Empty);
        }


        /// <summary>
        /// From email inheritfromglobal.
        /// </summary>
        [DatabaseField]
        public virtual bool FromEmailInheritfromglobal
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(FromEmailInheritfromglobal)), false);
            set => SetValue(nameof(FromEmailInheritfromglobal), value);
        }


        /// <summary>
        /// Display name.
        /// </summary>
        [DatabaseField]
        public virtual string DisplayName
        {
            get => ValidationHelper.GetString(GetValue(nameof(DisplayName)), "Email");
            set => SetValue(nameof(DisplayName), value, String.Empty);
        }


        /// <summary>
        /// Minutestotriggercalendarreminder inheritfromglobal.
        /// </summary>
        [DatabaseField]
        public virtual bool MinutestotriggercalendarreminderInheritfromglobal
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(MinutestotriggercalendarreminderInheritfromglobal)), false);
            set => SetValue(nameof(MinutestotriggercalendarreminderInheritfromglobal), value);
        }


        /// <summary>
        /// Minutestotriggercalendarreminder.
        /// </summary>
        [DatabaseField]
        public virtual int Minutestotriggercalendarreminder
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(Minutestotriggercalendarreminder)), 0);
            set => SetValue(nameof(Minutestotriggercalendarreminder), value, 0);
        }


        /// <summary>
        /// Displaylocationtitleinasidebar inheritfromglobal.
        /// </summary>
        [DatabaseField]
        public virtual bool DisplaylocationtitleinasidebarInheritfromglobal
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(DisplaylocationtitleinasidebarInheritfromglobal)), false);
            set => SetValue(nameof(DisplaylocationtitleinasidebarInheritfromglobal), value);
        }


        /// <summary>
        /// Displaylocationtitleinasidebar.
        /// </summary>
        [DatabaseField]
        public virtual bool Displaylocationtitleinasidebar
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(Displaylocationtitleinasidebar)), false);
            set => SetValue(nameof(Displaylocationtitleinasidebar), value);
        }


        /// <summary>
        /// Non ASCI ito ASCI icharacterdictionary inheritfromglobal.
        /// </summary>
        [DatabaseField]
        public virtual bool NonASCIItoASCIIcharacterdictionaryInheritfromglobal
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(NonASCIItoASCIIcharacterdictionaryInheritfromglobal)), false);
            set => SetValue(nameof(NonASCIItoASCIIcharacterdictionaryInheritfromglobal), value);
        }


        /// <summary>
        /// Non ASCI ito ASCI icharacterdictionary.
        /// </summary>
        [DatabaseField]
        public virtual string NonASCIItoASCIIcharacterdictionary
        {
            get => ValidationHelper.GetString(GetValue(nameof(NonASCIItoASCIIcharacterdictionary)), String.Empty);
            set => SetValue(nameof(NonASCIItoASCIIcharacterdictionary), value, String.Empty);
        }


        /// <summary>
        /// Enableexternallinkwarning inheritfromlocal.
        /// </summary>
        [DatabaseField]
        public virtual bool EnableexternallinkwarningInheritfromlocal
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(EnableexternallinkwarningInheritfromlocal)), false);
            set => SetValue(nameof(EnableexternallinkwarningInheritfromlocal), value);
        }


        /// <summary>
        /// Enableexternallinkwarning.
        /// </summary>
        [DatabaseField]
        public virtual bool Enableexternallinkwarning
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(Enableexternallinkwarning)), false);
            set => SetValue(nameof(Enableexternallinkwarning), value);
        }


        /// <summary>
        /// Seo settings robots inheritfromglobal.
        /// </summary>
        [DatabaseField]
        public virtual bool SeoSettingsRobotsInheritfromglobal
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(SeoSettingsRobotsInheritfromglobal)), false);
            set => SetValue(nameof(SeoSettingsRobotsInheritfromglobal), value);
        }


        /// <summary>
        /// Seo settings robots.
        /// </summary>
        [DatabaseField]
        public virtual string SeoSettingsRobots
        {
            get => ValidationHelper.GetString(GetValue(nameof(SeoSettingsRobots)), String.Empty);
            set => SetValue(nameof(SeoSettingsRobots), value, String.Empty);
        }


        /// <summary>
        /// Seo settings security inheritfrom global.
        /// </summary>
        [DatabaseField]
        public virtual bool SeoSettingsSecurityInheritfromGlobal
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(SeoSettingsSecurityInheritfromGlobal)), false);
            set => SetValue(nameof(SeoSettingsSecurityInheritfromGlobal), value);
        }


        /// <summary>
        /// Seo settings security.
        /// </summary>
        [DatabaseField]
        public virtual string SeoSettingsSecurity
        {
            get => ValidationHelper.GetString(GetValue(nameof(SeoSettingsSecurity)), String.Empty);
            set => SetValue(nameof(SeoSettingsSecurity), value, String.Empty);
        }


        /// <summary>
        /// Disable download inheritfromglobal.
        /// </summary>
        [DatabaseField]
        public virtual bool DisableDownloadInheritfromglobal
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(DisableDownloadInheritfromglobal)), false);
            set => SetValue(nameof(DisableDownloadInheritfromglobal), value);
        }


        /// <summary>
        /// Disable download.
        /// </summary>
        [DatabaseField]
        public virtual bool DisableDownload
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(DisableDownload)), false);
            set => SetValue(nameof(DisableDownload), value);
        }


        /// <summary>
        /// Disable print inheritfromglobal.
        /// </summary>
        [DatabaseField]
        public virtual bool DisablePrintInheritfromglobal
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(DisablePrintInheritfromglobal)), false);
            set => SetValue(nameof(DisablePrintInheritfromglobal), value);
        }


        /// <summary>
        /// Disable print.
        /// </summary>
        [DatabaseField]
        public virtual bool DisablePrint
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(DisablePrint)), false);
            set => SetValue(nameof(DisablePrint), value);
        }


        /// <summary>
        /// Disable facebook inheritfromglobal.
        /// </summary>
        [DatabaseField]
        public virtual bool DisableFacebookInheritfromglobal
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(DisableFacebookInheritfromglobal)), false);
            set => SetValue(nameof(DisableFacebookInheritfromglobal), value);
        }


        /// <summary>
        /// Disable facebook.
        /// </summary>
        [DatabaseField]
        public virtual bool DisableFacebook
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(DisableFacebook)), false);
            set => SetValue(nameof(DisableFacebook), value);
        }


        /// <summary>
        /// Disable twitter inheritfromglobal.
        /// </summary>
        [DatabaseField]
        public virtual bool DisableTwitterInheritfromglobal
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(DisableTwitterInheritfromglobal)), false);
            set => SetValue(nameof(DisableTwitterInheritfromglobal), value);
        }


        /// <summary>
        /// Disable twitter.
        /// </summary>
        [DatabaseField]
        public virtual bool DisableTwitter
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(DisableTwitter)), false);
            set => SetValue(nameof(DisableTwitter), value);
        }


        /// <summary>
        /// Disable linked in inheritfromglobal.
        /// </summary>
        [DatabaseField]
        public virtual bool DisableLinkedInInheritfromglobal
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(DisableLinkedInInheritfromglobal)), false);
            set => SetValue(nameof(DisableLinkedInInheritfromglobal), value);
        }


        /// <summary>
        /// Disable linked in.
        /// </summary>
        [DatabaseField]
        public virtual bool DisableLinkedIn
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(DisableLinkedIn)), false);
            set => SetValue(nameof(DisableLinkedIn), value);
        }


        /// <summary>
        /// Disable email inheritfromglobal.
        /// </summary>
        [DatabaseField]
        public virtual bool DisableEmailInheritfromglobal
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(DisableEmailInheritfromglobal)), false);
            set => SetValue(nameof(DisableEmailInheritfromglobal), value);
        }


        /// <summary>
        /// Disable email.
        /// </summary>
        [DatabaseField]
        public virtual bool DisableEmail
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(DisableEmail)), false);
            set => SetValue(nameof(DisableEmail), value);
        }


        /// <summary>
        /// Disable copy link button inheritfromglobal.
        /// </summary>
        [DatabaseField]
        public virtual bool DisableCopyLinkButtonInheritfromglobal
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(DisableCopyLinkButtonInheritfromglobal)), false);
            set => SetValue(nameof(DisableCopyLinkButtonInheritfromglobal), value);
        }


        /// <summary>
        /// Disable copy link button.
        /// </summary>
        [DatabaseField]
        public virtual bool DisableCopyLinkButton
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(DisableCopyLinkButton)), false);
            set => SetValue(nameof(DisableCopyLinkButton), value);
        }


        /// <summary>
        /// Showtooltip inheritfromglobal.
        /// </summary>
        [DatabaseField]
        public virtual bool ShowtooltipInheritfromglobal
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(ShowtooltipInheritfromglobal)), false);
            set => SetValue(nameof(ShowtooltipInheritfromglobal), value);
        }


        /// <summary>
        /// Showtooltip.
        /// </summary>
        [DatabaseField]
        public virtual bool Showtooltip
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(Showtooltip)), false);
            set => SetValue(nameof(Showtooltip), value);
        }


        /// <summary>
        /// Display emailinv card inheritfromglobal.
        /// </summary>
        [DatabaseField]
        public virtual bool DisplayEmailinvCardInheritfromglobal
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(DisplayEmailinvCardInheritfromglobal)), false);
            set => SetValue(nameof(DisplayEmailinvCardInheritfromglobal), value);
        }


        /// <summary>
        /// Display emailinv card.
        /// </summary>
        [DatabaseField]
        public virtual bool DisplayEmailinvCard
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(DisplayEmailinvCard)), false);
            set => SetValue(nameof(DisplayEmailinvCard), value);
        }


        /// <summary>
        /// Use page title to display person name inheritfromglobal.
        /// </summary>
        [DatabaseField]
        public virtual bool UsePageTitleToDisplayPersonNameInheritfromglobal
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(UsePageTitleToDisplayPersonNameInheritfromglobal)), false);
            set => SetValue(nameof(UsePageTitleToDisplayPersonNameInheritfromglobal), value);
        }


        /// <summary>
        /// Use page title to display person name.
        /// </summary>
        [DatabaseField]
        public virtual bool UsePageTitleToDisplayPersonName
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(UsePageTitleToDisplayPersonName)), false);
            set => SetValue(nameof(UsePageTitleToDisplayPersonName), value);
        }


        /// <summary>
        /// Enable cookiebot inheritfromglobal.
        /// </summary>
        [DatabaseField]
        public virtual bool EnableCookiebotInheritfromglobal
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(EnableCookiebotInheritfromglobal)), false);
            set => SetValue(nameof(EnableCookiebotInheritfromglobal), value);
        }


        /// <summary>
        /// Enable cookiebot.
        /// </summary>
        [DatabaseField]
        public virtual bool EnableCookiebot
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(EnableCookiebot)), false);
            set => SetValue(nameof(EnableCookiebot), value);
        }


        /// <summary>
        /// Cookie ID inheritfromglobal.
        /// </summary>
        [DatabaseField]
        public virtual bool CookieIDInheritfromglobal
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(CookieIDInheritfromglobal)), false);
            set => SetValue(nameof(CookieIDInheritfromglobal), value);
        }


        /// <summary>
        /// Cookie ID.
        /// </summary>
        [DatabaseField]
        public virtual string CookieID
        {
            get => ValidationHelper.GetString(GetValue(nameof(CookieID)), String.Empty);
            set => SetValue(nameof(CookieID), value, String.Empty);
        }


        /// <summary>
        /// Custom cookiebot script inheritfromglobal.
        /// </summary>
        [DatabaseField]
        public virtual bool CustomCookiebotScriptInheritfromglobal
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(CustomCookiebotScriptInheritfromglobal)), false);
            set => SetValue(nameof(CustomCookiebotScriptInheritfromglobal), value);
        }


        /// <summary>
        /// Custom cookiebot script.
        /// </summary>
        [DatabaseField]
        public virtual string CustomCookiebotScript
        {
            get => ValidationHelper.GetString(GetValue(nameof(CustomCookiebotScript)), String.Empty);
            set => SetValue(nameof(CustomCookiebotScript), value, String.Empty);
        }


        /// <summary>
        /// Show advanced widgets.
        /// </summary>
        [DatabaseField]
        public virtual bool ShowAdvancedWidgets
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(ShowAdvancedWidgets)), false);
            set => SetValue(nameof(ShowAdvancedWidgets), value);
        }


        /// <summary>
        /// Show advanced widgets inheritfromglobalsettings.
        /// </summary>
        [DatabaseField]
        public virtual bool ShowAdvancedWidgetsInheritfromglobalsettings
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(ShowAdvancedWidgetsInheritfromglobalsettings)), false);
            set => SetValue(nameof(ShowAdvancedWidgetsInheritfromglobalsettings), value);
        }


        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            Provider.Delete(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            Provider.Set(this);
        }


        /// <summary>
        /// Creates an empty instance of the <see cref="WebChannelSnippetInfo"/> class.
        /// </summary>
        public WebChannelSnippetInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="WebChannelSnippetInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public WebChannelSnippetInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}