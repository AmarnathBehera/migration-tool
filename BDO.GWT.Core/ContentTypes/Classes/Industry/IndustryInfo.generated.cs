using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(IndustryInfo), IndustryInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="IndustryInfo"/>.
    /// </summary>
    public partial class IndustryInfo : AbstractInfo<IndustryInfo, IInfoProvider<IndustryInfo>>, IInfoWithId, IInfoWithName, IInfoWithGuid
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdodealcards.industry";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<IndustryInfo>), OBJECT_TYPE, "BDODealCards.Industry", "IndustryID", "IndustryLastModified", "IndustryGuid", "IndustryCodeName", "IndustryName", null, null, null)
        {
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("IndustrySiteID", "cms.site", ObjectDependencyEnum.Required),
            },
        };


        /// <summary>
        /// Industry ID.
        /// </summary>
        [DatabaseField]
        public virtual int IndustryID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(IndustryID)), 0);
            set => SetValue(nameof(IndustryID), value);
        }


        /// <summary>
        /// Industry name.
        /// </summary>
        [DatabaseField]
        public virtual string IndustryName
        {
            get => ValidationHelper.GetString(GetValue(nameof(IndustryName)), String.Empty);
            set => SetValue(nameof(IndustryName), value);
        }


        /// <summary>
        /// Industry code name.
        /// </summary>
        [DatabaseField]
        public virtual string IndustryCodeName
        {
            get => ValidationHelper.GetString(GetValue(nameof(IndustryCodeName)), String.Empty);
            set => SetValue(nameof(IndustryCodeName), value);
        }


        /// <summary>
        /// Industry culture code.
        /// </summary>
        [DatabaseField]
        public virtual string IndustryCultureCode
        {
            get => ValidationHelper.GetString(GetValue(nameof(IndustryCultureCode)), String.Empty);
            set => SetValue(nameof(IndustryCultureCode), value);
        }


        /// <summary>
        /// Industry site ID.
        /// </summary>
        [DatabaseField]
        public virtual int IndustrySiteID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(IndustrySiteID)), 0);
            set => SetValue(nameof(IndustrySiteID), value);
        }


        /// <summary>
        /// Industry guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid IndustryGuid
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(IndustryGuid)), Guid.Empty);
            set => SetValue(nameof(IndustryGuid), value);
        }


        /// <summary>
        /// Industry last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime IndustryLastModified
        {
            get => ValidationHelper.GetDateTime(GetValue(nameof(IndustryLastModified)), DateTimeHelper.ZERO_TIME);
            set => SetValue(nameof(IndustryLastModified), value);
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
        /// Creates an empty instance of the <see cref="IndustryInfo"/> class.
        /// </summary>
        public IndustryInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="IndustryInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public IndustryInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}