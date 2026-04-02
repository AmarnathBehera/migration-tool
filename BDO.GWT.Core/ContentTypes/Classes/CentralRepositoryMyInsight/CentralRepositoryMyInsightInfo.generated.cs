using System;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(CentralRepositoryMyInsightInfo), CentralRepositoryMyInsightInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="CentralRepositoryMyInsightInfo"/>.
    /// </summary>
    public partial class CentralRepositoryMyInsightInfo : AbstractInfo<CentralRepositoryMyInsightInfo, IInfoProvider<CentralRepositoryMyInsightInfo>>, IInfoWithId, IInfoWithName, IInfoWithGuid
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdo.centralrepository_myinsight";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<CentralRepositoryMyInsightInfo>), OBJECT_TYPE, "BDO.CentralRepositoryMyInsight", "MyInsightID", "MyInsightLastModified", "MyInsightGuid", "SiteName", null, null, null, null)
        {
            TouchCacheDependencies = true,
        };


        /// <summary>
        /// My insight ID.
        /// </summary>
        [DatabaseField]
        public virtual int MyInsightID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(MyInsightID)), 0);
            set => SetValue(nameof(MyInsightID), value);
        }


        /// <summary>
        /// My insight guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid MyInsightGuid
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(MyInsightGuid)), Guid.Empty);
            set => SetValue(nameof(MyInsightGuid), value);
        }


        /// <summary>
        /// My insight last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime MyInsightLastModified
        {
            get => ValidationHelper.GetDateTime(GetValue(nameof(MyInsightLastModified)), DateTimeHelper.ZERO_TIME);
            set => SetValue(nameof(MyInsightLastModified), value);
        }


        /// <summary>
        /// Insight ID.
        /// </summary>
        [DatabaseField]
        public virtual int InsightID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(InsightID)), 0);
            set => SetValue(nameof(InsightID), value);
        }


        /// <summary>
        /// Site name.
        /// </summary>
        [DatabaseField]
        public virtual string SiteName
        {
            get => ValidationHelper.GetString(GetValue(nameof(SiteName)), String.Empty);
            set => SetValue(nameof(SiteName), value, String.Empty);
        }


        /// <summary>
        /// Site guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid SiteGuid
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(SiteGuid)), Guid.Empty);
            set => SetValue(nameof(SiteGuid), value, Guid.Empty);
        }


        /// <summary>
        /// Created by.
        /// </summary>
        [DatabaseField]
        public virtual string CreatedBy
        {
            get => ValidationHelper.GetString(GetValue(nameof(CreatedBy)), String.Empty);
            set => SetValue(nameof(CreatedBy), value, String.Empty);
        }


        /// <summary>
        /// Publication status.
        /// </summary>
        [DatabaseField]
        public virtual string PublicationStatus
        {
            get => ValidationHelper.GetString(GetValue(nameof(PublicationStatus)), String.Empty);
            set => SetValue(nameof(PublicationStatus), value, String.Empty);
        }


        /// <summary>
        /// Culture.
        /// </summary>
        [DatabaseField]
        public virtual string Culture
        {
            get => ValidationHelper.GetString(GetValue(nameof(Culture)), String.Empty);
            set => SetValue(nameof(Culture), value, String.Empty);
        }


        /// <summary>
        /// Last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime LastModified
        {
            get => ValidationHelper.GetDateTime(GetValue(nameof(LastModified)), DateTimeHelper.ZERO_TIME);
            set => SetValue(nameof(LastModified), value, DateTimeHelper.ZERO_TIME);
        }


        /// <summary>
        /// Original insight title.
        /// </summary>
        [DatabaseField]
        public virtual string OriginalInsightTitle
        {
            get => ValidationHelper.GetString(GetValue(nameof(OriginalInsightTitle)), String.Empty);
            set => SetValue(nameof(OriginalInsightTitle), value, String.Empty);
        }


        /// <summary>
        /// Original document guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid OriginalDocumentGuid
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(OriginalDocumentGuid)), Guid.Empty);
            set => SetValue(nameof(OriginalDocumentGuid), value, Guid.Empty);
        }


        /// <summary>
        /// Original culture.
        /// </summary>
        [DatabaseField]
        public virtual string OriginalCulture
        {
            get => ValidationHelper.GetString(GetValue(nameof(OriginalCulture)), String.Empty);
            set => SetValue(nameof(OriginalCulture), value, String.Empty);
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
        /// Creates an empty instance of the <see cref="CentralRepositoryMyInsightInfo"/> class.
        /// </summary>
        public CentralRepositoryMyInsightInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="CentralRepositoryMyInsightInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public CentralRepositoryMyInsightInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}