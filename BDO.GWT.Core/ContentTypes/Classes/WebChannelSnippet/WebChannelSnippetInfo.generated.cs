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
        /// Secretkeyre CAPTCH av 3 inheritfromglobal.
        /// </summary>
        [DatabaseField]
        public virtual bool SecretkeyreCAPTCHAv3Inheritfromglobal
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(SecretkeyreCAPTCHAv3Inheritfromglobal)), false);
            set => SetValue(nameof(SecretkeyreCAPTCHAv3Inheritfromglobal), value);
        }


        /// <summary>
        /// Secretkeyre CAPTCH av 2 inheritfromglobal.
        /// </summary>
        [DatabaseField]
        public virtual bool SecretkeyreCAPTCHAv2Inheritfromglobal
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(SecretkeyreCAPTCHAv2Inheritfromglobal)), false);
            set => SetValue(nameof(SecretkeyreCAPTCHAv2Inheritfromglobal), value);
        }


        /// <summary>
        /// Sitekeyre CAPTCH av 3 inheritfromglobal.
        /// </summary>
        [DatabaseField]
        public virtual bool SitekeyreCAPTCHAv3Inheritfromglobal
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(SitekeyreCAPTCHAv3Inheritfromglobal)), false);
            set => SetValue(nameof(SitekeyreCAPTCHAv3Inheritfromglobal), value);
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