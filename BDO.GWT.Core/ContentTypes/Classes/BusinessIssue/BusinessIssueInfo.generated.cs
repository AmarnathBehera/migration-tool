using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(BusinessIssueInfo), BusinessIssueInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="BusinessIssueInfo"/>.
    /// </summary>
    public partial class BusinessIssueInfo : AbstractInfo<BusinessIssueInfo, IInfoProvider<BusinessIssueInfo>>, IInfoWithId, IInfoWithName, IInfoWithGuid
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdometadata.businessissue";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<BusinessIssueInfo>), OBJECT_TYPE, "BDOMetadata.BusinessIssue", "BusinessIssueID", "BusinessIssueLastModified", "BusinessIssueGuid", "BusinessIssueCodeName", "BusinessIssueName", null, null, null)
        {
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("BusinessIssueSiteID", "cms.site", ObjectDependencyEnum.Required),
            },
        };


        /// <summary>
        /// Business issue ID.
        /// </summary>
        [DatabaseField]
        public virtual int BusinessIssueID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(BusinessIssueID)), 0);
            set => SetValue(nameof(BusinessIssueID), value);
        }


        /// <summary>
        /// Business issue name.
        /// </summary>
        [DatabaseField]
        public virtual string BusinessIssueName
        {
            get => ValidationHelper.GetString(GetValue(nameof(BusinessIssueName)), String.Empty);
            set => SetValue(nameof(BusinessIssueName), value);
        }


        /// <summary>
        /// Business issue code name.
        /// </summary>
        [DatabaseField]
        public virtual string BusinessIssueCodeName
        {
            get => ValidationHelper.GetString(GetValue(nameof(BusinessIssueCodeName)), String.Empty);
            set => SetValue(nameof(BusinessIssueCodeName), value);
        }


        /// <summary>
        /// Business issue culture code.
        /// </summary>
        [DatabaseField]
        public virtual string BusinessIssueCultureCode
        {
            get => ValidationHelper.GetString(GetValue(nameof(BusinessIssueCultureCode)), String.Empty);
            set => SetValue(nameof(BusinessIssueCultureCode), value);
        }


        /// <summary>
        /// Business issue site ID.
        /// </summary>
        [DatabaseField]
        public virtual int BusinessIssueSiteID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(BusinessIssueSiteID)), 0);
            set => SetValue(nameof(BusinessIssueSiteID), value);
        }


        /// <summary>
        /// Business issue guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid BusinessIssueGuid
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(BusinessIssueGuid)), Guid.Empty);
            set => SetValue(nameof(BusinessIssueGuid), value);
        }


        /// <summary>
        /// Business issue last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime BusinessIssueLastModified
        {
            get => ValidationHelper.GetDateTime(GetValue(nameof(BusinessIssueLastModified)), DateTimeHelper.ZERO_TIME);
            set => SetValue(nameof(BusinessIssueLastModified), value);
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
        /// Creates an empty instance of the <see cref="BusinessIssueInfo"/> class.
        /// </summary>
        public BusinessIssueInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="BusinessIssueInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public BusinessIssueInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}