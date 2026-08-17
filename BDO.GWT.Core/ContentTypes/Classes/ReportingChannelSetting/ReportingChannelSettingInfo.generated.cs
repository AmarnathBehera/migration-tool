using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(ReportingChannelSettingInfo), ReportingChannelSettingInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="ReportingChannelSettingInfo"/>.
    /// </summary>
    public partial class ReportingChannelSettingInfo : AbstractInfo<ReportingChannelSettingInfo, IInfoProvider<ReportingChannelSettingInfo>>, IInfoWithId, IInfoWithGuid
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdo.reportingchannelsetting";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<ReportingChannelSettingInfo>), OBJECT_TYPE, "BDO.ReportingChannelSetting", "ReportingChannelSettingID", null, "ReportingChannelSettingsGUID", null, "ReportingChannelSettingsDisplayName", null, null, null)
        {
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("ReportingChannelSettingsUId", "cms.channel", ObjectDependencyEnum.Required),
            },
        };


        /// <summary>
        /// Reporting channel setting ID.
        /// </summary>
        [DatabaseField]
        public virtual int ReportingChannelSettingID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(ReportingChannelSettingID)), 0);
            set => SetValue(nameof(ReportingChannelSettingID), value);
        }


        /// <summary>
        /// Reporting channel settings display name.
        /// </summary>
        [DatabaseField]
        public virtual string ReportingChannelSettingsDisplayName
        {
            get => ValidationHelper.GetString(GetValue(nameof(ReportingChannelSettingsDisplayName)), String.Empty);
            set => SetValue(nameof(ReportingChannelSettingsDisplayName), value);
        }


        /// <summary>
        /// Reporting channel settings U id.
        /// </summary>
        [DatabaseField]
        public virtual int ReportingChannelSettingsUId
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(ReportingChannelSettingsUId)), 0);
            set => SetValue(nameof(ReportingChannelSettingsUId), value);
        }


        /// <summary>
        /// Reporting channel settings GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid ReportingChannelSettingsGUID
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(ReportingChannelSettingsGUID)), Guid.Empty);
            set => SetValue(nameof(ReportingChannelSettingsGUID), value);
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
        /// Creates an empty instance of the <see cref="ReportingChannelSettingInfo"/> class.
        /// </summary>
        public ReportingChannelSettingInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="ReportingChannelSettingInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public ReportingChannelSettingInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}