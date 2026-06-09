using System;
using System.Data;

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