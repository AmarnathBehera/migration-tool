using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(GeneralSettingInfo), GeneralSettingInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="GeneralSettingInfo"/>.
    /// </summary>
    public partial class GeneralSettingInfo : AbstractInfo<GeneralSettingInfo, IInfoProvider<GeneralSettingInfo>>, IInfoWithId, IInfoWithName
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdo.generalsetting";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<GeneralSettingInfo>), OBJECT_TYPE, "BDO.GeneralSetting", "GeneralID", null, null, "CodeName", "CodeName", null, null, null)
        {
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("GeneralSettingsWebChannelSettingID", "bdo.webchannelsettings", ObjectDependencyEnum.Required),
            },
        };


        /// <summary>
        /// General ID.
        /// </summary>
        [DatabaseField]
        public virtual int GeneralID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(GeneralID)), 0);
            set => SetValue(nameof(GeneralID), value);
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
        /// General settings web channel setting ID.
        /// </summary>
        [DatabaseField]
        public virtual int GeneralSettingsWebChannelSettingID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(GeneralSettingsWebChannelSettingID)), 0);
            set => SetValue(nameof(GeneralSettingsWebChannelSettingID), value);
        }


        /// <summary>
        /// Code name.
        /// </summary>
        [DatabaseField]
        public virtual string CodeName
        {
            get => ValidationHelper.GetString(GetValue(nameof(CodeName)), String.Empty);
            set => SetValue(nameof(CodeName), value);
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
        /// Creates an empty instance of the <see cref="GeneralSettingInfo"/> class.
        /// </summary>
        public GeneralSettingInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="GeneralSettingInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public GeneralSettingInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}