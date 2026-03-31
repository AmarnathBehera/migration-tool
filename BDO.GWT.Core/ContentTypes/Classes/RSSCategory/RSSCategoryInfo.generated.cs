using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(RSSCategoryInfo), RSSCategoryInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="RSSCategoryInfo"/>.
    /// </summary>
    public partial class RSSCategoryInfo : AbstractInfo<RSSCategoryInfo, IInfoProvider<RSSCategoryInfo>>, IInfoWithId, IInfoWithName, IInfoWithGuid
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdometadata.rsscategory";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<RSSCategoryInfo>), OBJECT_TYPE, "BDOMetadata.RSSCategory", "RSSCategoryID", "RSSCategoryLastModified", "RSSCategoryGuid", "RSSCategoryCodeName", "RSSCategoryName", null, null, null)
        {
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("RSSCategorySiteID", "cms.site", ObjectDependencyEnum.Required),
            },
        };


        /// <summary>
        /// RSS category ID.
        /// </summary>
        [DatabaseField]
        public virtual int RSSCategoryID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(RSSCategoryID)), 0);
            set => SetValue(nameof(RSSCategoryID), value);
        }


        /// <summary>
        /// RSS category name.
        /// </summary>
        [DatabaseField]
        public virtual string RSSCategoryName
        {
            get => ValidationHelper.GetString(GetValue(nameof(RSSCategoryName)), String.Empty);
            set => SetValue(nameof(RSSCategoryName), value);
        }


        /// <summary>
        /// RSS category code name.
        /// </summary>
        [DatabaseField]
        public virtual string RSSCategoryCodeName
        {
            get => ValidationHelper.GetString(GetValue(nameof(RSSCategoryCodeName)), String.Empty);
            set => SetValue(nameof(RSSCategoryCodeName), value);
        }


        /// <summary>
        /// RSS category culture code.
        /// </summary>
        [DatabaseField]
        public virtual string RSSCategoryCultureCode
        {
            get => ValidationHelper.GetString(GetValue(nameof(RSSCategoryCultureCode)), String.Empty);
            set => SetValue(nameof(RSSCategoryCultureCode), value);
        }


        /// <summary>
        /// RSS category site ID.
        /// </summary>
        [DatabaseField]
        public virtual int RSSCategorySiteID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(RSSCategorySiteID)), 0);
            set => SetValue(nameof(RSSCategorySiteID), value);
        }


        /// <summary>
        /// RSS category guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid RSSCategoryGuid
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(RSSCategoryGuid)), Guid.Empty);
            set => SetValue(nameof(RSSCategoryGuid), value);
        }


        /// <summary>
        /// RSS category last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime RSSCategoryLastModified
        {
            get => ValidationHelper.GetDateTime(GetValue(nameof(RSSCategoryLastModified)), DateTimeHelper.ZERO_TIME);
            set => SetValue(nameof(RSSCategoryLastModified), value);
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
        /// Creates an empty instance of the <see cref="RSSCategoryInfo"/> class.
        /// </summary>
        public RSSCategoryInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="RSSCategoryInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public RSSCategoryInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}