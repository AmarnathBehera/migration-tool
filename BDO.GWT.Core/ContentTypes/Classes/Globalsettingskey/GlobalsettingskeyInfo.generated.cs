using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(GlobalsettingskeyInfo), GlobalsettingskeyInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="GlobalsettingskeyInfo"/>.
    /// </summary>
    public partial class GlobalsettingskeyInfo : AbstractInfo<GlobalsettingskeyInfo, IInfoProvider<GlobalsettingskeyInfo>>, IInfoWithId
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdo.globalsettingskey";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<GlobalsettingskeyInfo>), OBJECT_TYPE, "BDO.Globalsettingskey", "GlobalsettingskeyID", null, null, null, "GlobalSettingsKeyDisplayName", null, null, null)
        {
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>()
            {
            },
        };


        /// <summary>
        /// Globalsettingskey ID.
        /// </summary>
        [DatabaseField]
        public virtual int GlobalsettingskeyID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(GlobalsettingskeyID)), 0);
            set => SetValue(nameof(GlobalsettingskeyID), value);
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
        /// Enable cookiebot.
        /// </summary>
        [DatabaseField]
        public virtual bool EnableCookiebot
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(EnableCookiebot)), false);
            set => SetValue(nameof(EnableCookiebot), value);
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
        /// Custom cookiebot script.
        /// </summary>
        [DatabaseField]
        public virtual string CustomCookiebotScript
        {
            get => ValidationHelper.GetString(GetValue(nameof(CustomCookiebotScript)), String.Empty);
            set => SetValue(nameof(CustomCookiebotScript), value, String.Empty);
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
        /// Displaylocationtitleinasidebar.
        /// </summary>
        [DatabaseField]
        public virtual bool Displaylocationtitleinasidebar
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(Displaylocationtitleinasidebar)), false);
            set => SetValue(nameof(Displaylocationtitleinasidebar), value);
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
        /// Display name.
        /// </summary>
        [DatabaseField]
        public virtual string DisplayName
        {
            get => ValidationHelper.GetString(GetValue(nameof(DisplayName)), "Email");
            set => SetValue(nameof(DisplayName), value, String.Empty);
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
        /// Enableexternallinkwarning.
        /// </summary>
        [DatabaseField]
        public virtual bool Enableexternallinkwarning
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(Enableexternallinkwarning)), false);
            set => SetValue(nameof(Enableexternallinkwarning), value);
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
        /// Seo settings security.
        /// </summary>
        [DatabaseField]
        public virtual string SeoSettingsSecurity
        {
            get => ValidationHelper.GetString(GetValue(nameof(SeoSettingsSecurity)), String.Empty);
            set => SetValue(nameof(SeoSettingsSecurity), value, String.Empty);
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
        /// Disable print.
        /// </summary>
        [DatabaseField]
        public virtual bool DisablePrint
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(DisablePrint)), false);
            set => SetValue(nameof(DisablePrint), value);
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
        /// Disable twitter.
        /// </summary>
        [DatabaseField]
        public virtual bool DisableTwitter
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(DisableTwitter)), false);
            set => SetValue(nameof(DisableTwitter), value);
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
        /// Disable email.
        /// </summary>
        [DatabaseField]
        public virtual bool DisableEmail
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(DisableEmail)), false);
            set => SetValue(nameof(DisableEmail), value);
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
        /// Showtooltip.
        /// </summary>
        [DatabaseField]
        public virtual bool Showtooltip
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(Showtooltip)), false);
            set => SetValue(nameof(Showtooltip), value);
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
        /// Use page title to display person name.
        /// </summary>
        [DatabaseField]
        public virtual bool UsePageTitleToDisplayPersonName
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(UsePageTitleToDisplayPersonName)), false);
            set => SetValue(nameof(UsePageTitleToDisplayPersonName), value);
        }


        /// <summary>
        /// Global locations title.
        /// </summary>
        [DatabaseField]
        public virtual string GlobalLocationsTitle
        {
            get => ValidationHelper.GetString(GetValue(nameof(GlobalLocationsTitle)), String.Empty);
            set => SetValue(nameof(GlobalLocationsTitle), value, String.Empty);
        }


        /// <summary>
        /// Global locations description.
        /// </summary>
        [DatabaseField]
        public virtual string GlobalLocationsDescription
        {
            get => ValidationHelper.GetString(GetValue(nameof(GlobalLocationsDescription)), String.Empty);
            set => SetValue(nameof(GlobalLocationsDescription), value, String.Empty);
        }


        /// <summary>
        /// Global locations background image.
        /// </summary>
        [DatabaseField(ValueType = typeof(string))]
        public IEnumerable<global::CMS.ContentEngine.ContentItemReference> GlobalLocationsBackgroundImage
        {
            get => global::CMS.DataEngine.Internal.JsonDataTypeConverter.ConvertToModels<global::CMS.ContentEngine.ContentItemReference>(GetValue(nameof(GlobalLocationsBackgroundImage), String.Empty));
            set => SetValue(nameof(GlobalLocationsBackgroundImage), global::CMS.DataEngine.Internal.JsonDataTypeConverter.ConvertToString<IEnumerable<global::CMS.ContentEngine.ContentItemReference>>(value, Enumerable.Empty<global::CMS.ContentEngine.ContentItemReference>(), null));
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
        /// Creates an empty instance of the <see cref="GlobalsettingskeyInfo"/> class.
        /// </summary>
        public GlobalsettingskeyInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="GlobalsettingskeyInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public GlobalsettingskeyInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}