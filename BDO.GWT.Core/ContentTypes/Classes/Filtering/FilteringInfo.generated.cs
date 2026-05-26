using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(FilteringInfo), FilteringInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="FilteringInfo"/>.
    /// </summary>
    public partial class FilteringInfo : AbstractInfo<FilteringInfo, IInfoProvider<FilteringInfo>>, IInfoWithId
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdo.filtering";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<FilteringInfo>), OBJECT_TYPE, "BDO.Filtering", "FilteringID", null, null, null, null, null, null, null)
        {
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("FilteringSettingsWebChannelSettingID", "bdo.webchannelsettings", ObjectDependencyEnum.Required),
            },
        };


        /// <summary>
        /// Filtering ID.
        /// </summary>
        [DatabaseField]
        public virtual int FilteringID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(FilteringID)), 0);
            set => SetValue(nameof(FilteringID), value);
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
        /// Filtering settings web channel setting ID.
        /// </summary>
        [DatabaseField]
        public virtual int FilteringSettingsWebChannelSettingID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(FilteringSettingsWebChannelSettingID)), 0);
            set => SetValue(nameof(FilteringSettingsWebChannelSettingID), value);
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
        /// Creates an empty instance of the <see cref="FilteringInfo"/> class.
        /// </summary>
        public FilteringInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="FilteringInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public FilteringInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}