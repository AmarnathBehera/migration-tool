using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(SubIndustryInfo), SubIndustryInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="SubIndustryInfo"/>.
    /// </summary>
    public partial class SubIndustryInfo : AbstractInfo<SubIndustryInfo, IInfoProvider<SubIndustryInfo>>, IInfoWithId, IInfoWithName, IInfoWithGuid
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdodealcards.subindustry";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<SubIndustryInfo>), OBJECT_TYPE, "BDODealCards.SubIndustry", "SubIndustryID", "SubIndustryLastModified", "SubIndustryGuid", "SubIndustryCodeName", "SubIndustryName", null, null, null)
        {
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("SubIndustrySiteID", "cms.site", ObjectDependencyEnum.Required),
            },
        };


        /// <summary>
        /// Sub industry ID.
        /// </summary>
        [DatabaseField]
        public virtual int SubIndustryID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(SubIndustryID)), 0);
            set => SetValue(nameof(SubIndustryID), value);
        }


        /// <summary>
        /// Sub industry name.
        /// </summary>
        [DatabaseField]
        public virtual string SubIndustryName
        {
            get => ValidationHelper.GetString(GetValue(nameof(SubIndustryName)), String.Empty);
            set => SetValue(nameof(SubIndustryName), value);
        }


        /// <summary>
        /// Sub industry code name.
        /// </summary>
        [DatabaseField]
        public virtual string SubIndustryCodeName
        {
            get => ValidationHelper.GetString(GetValue(nameof(SubIndustryCodeName)), String.Empty);
            set => SetValue(nameof(SubIndustryCodeName), value);
        }


        /// <summary>
        /// Sub industry culture code.
        /// </summary>
        [DatabaseField]
        public virtual string SubIndustryCultureCode
        {
            get => ValidationHelper.GetString(GetValue(nameof(SubIndustryCultureCode)), String.Empty);
            set => SetValue(nameof(SubIndustryCultureCode), value);
        }


        /// <summary>
        /// Sub industry related.
        /// </summary>
        [DatabaseField]
        public virtual string SubIndustryRelated
        {
            get => ValidationHelper.GetString(GetValue(nameof(SubIndustryRelated)), String.Empty);
            set => SetValue(nameof(SubIndustryRelated), value);
        }


        /// <summary>
        /// Sub industry site ID.
        /// </summary>
        [DatabaseField]
        public virtual int SubIndustrySiteID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(SubIndustrySiteID)), 0);
            set => SetValue(nameof(SubIndustrySiteID), value);
        }


        /// <summary>
        /// Sub industry guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid SubIndustryGuid
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(SubIndustryGuid)), Guid.Empty);
            set => SetValue(nameof(SubIndustryGuid), value);
        }


        /// <summary>
        /// Sub industry last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime SubIndustryLastModified
        {
            get => ValidationHelper.GetDateTime(GetValue(nameof(SubIndustryLastModified)), DateTimeHelper.ZERO_TIME);
            set => SetValue(nameof(SubIndustryLastModified), value);
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
        /// Creates an empty instance of the <see cref="SubIndustryInfo"/> class.
        /// </summary>
        public SubIndustryInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="SubIndustryInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public SubIndustryInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}