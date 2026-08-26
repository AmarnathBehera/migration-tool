using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(GDPRSettingsInfo), GDPRSettingsInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="GDPRSettingsInfo"/>.
    /// </summary>
    public partial class GDPRSettingsInfo : AbstractInfo<GDPRSettingsInfo, IInfoProvider<GDPRSettingsInfo>>, IInfoWithId
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdo.gdprsettings";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<GDPRSettingsInfo>), OBJECT_TYPE, "BDO.GDPRSettings", "GDPRSettingsID", null, null, null, null, null, null, null)
        {
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("GDPRWebChannelSettingID", "bdo.webchannelsettings", ObjectDependencyEnum.Required),
            },
        };


        /// <summary>
        /// GDPR settings ID.
        /// </summary>
        [DatabaseField]
        public virtual int GDPRSettingsID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(GDPRSettingsID)), 0);
            set => SetValue(nameof(GDPRSettingsID), value);
        }


        /// <summary>
        /// GDPR web channel setting ID.
        /// </summary>
        [DatabaseField]
        public virtual int GDPRWebChannelSettingID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(GDPRWebChannelSettingID)), 0);
            set => SetValue(nameof(GDPRWebChannelSettingID), value);
        }


        /// <summary>
        /// Text.
        /// </summary>
        [DatabaseField]
        public virtual string Text
        {
            get => ValidationHelper.GetString(GetValue(nameof(Text)), String.Empty);
            set => SetValue(nameof(Text), value, String.Empty);
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
        /// Creates an empty instance of the <see cref="GDPRSettingsInfo"/> class.
        /// </summary>
        public GDPRSettingsInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="GDPRSettingsInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public GDPRSettingsInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}