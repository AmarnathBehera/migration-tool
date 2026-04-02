using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(RangeInfo), RangeInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="RangeInfo"/>.
    /// </summary>
    public partial class RangeInfo : AbstractInfo<RangeInfo, IInfoProvider<RangeInfo>>, IInfoWithId, IInfoWithName, IInfoWithGuid
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdodealcards.range";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<RangeInfo>), OBJECT_TYPE, "BDODealCards.Range", "RangeID", "RangeLastModified", "RangeGuid", "RangeCodeName", "RangeValue", null, null, null)
        {
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("RangeSiteID", "cms.site", ObjectDependencyEnum.Required),
            },
        };


        /// <summary>
        /// Range ID.
        /// </summary>
        [DatabaseField]
        public virtual int RangeID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(RangeID)), 0);
            set => SetValue(nameof(RangeID), value);
        }


        /// <summary>
        /// Range value.
        /// </summary>
        [DatabaseField]
        public virtual string RangeValue
        {
            get => ValidationHelper.GetString(GetValue(nameof(RangeValue)), String.Empty);
            set => SetValue(nameof(RangeValue), value);
        }


        /// <summary>
        /// Range code name.
        /// </summary>
        [DatabaseField]
        public virtual string RangeCodeName
        {
            get => ValidationHelper.GetString(GetValue(nameof(RangeCodeName)), String.Empty);
            set => SetValue(nameof(RangeCodeName), value);
        }


        /// <summary>
        /// Range order.
        /// </summary>
        [DatabaseField]
        public virtual int RangeOrder
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(RangeOrder)), 0);
            set => SetValue(nameof(RangeOrder), value, 0);
        }


        /// <summary>
        /// Range site ID.
        /// </summary>
        [DatabaseField]
        public virtual int RangeSiteID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(RangeSiteID)), 0);
            set => SetValue(nameof(RangeSiteID), value);
        }


        /// <summary>
        /// Range guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid RangeGuid
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(RangeGuid)), Guid.Empty);
            set => SetValue(nameof(RangeGuid), value);
        }


        /// <summary>
        /// Range last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime RangeLastModified
        {
            get => ValidationHelper.GetDateTime(GetValue(nameof(RangeLastModified)), DateTimeHelper.ZERO_TIME);
            set => SetValue(nameof(RangeLastModified), value);
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
        /// Creates an empty instance of the <see cref="RangeInfo"/> class.
        /// </summary>
        public RangeInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="RangeInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public RangeInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}