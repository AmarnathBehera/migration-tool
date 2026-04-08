using System;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(CentralRepositoryInsightInfo), CentralRepositoryInsightInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="CentralRepositoryInsightInfo"/>.
    /// </summary>
    public partial class CentralRepositoryInsightInfo : AbstractInfo<CentralRepositoryInsightInfo, IInfoProvider<CentralRepositoryInsightInfo>>, IInfoWithId, IInfoWithName, IInfoWithGuid
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdo.centralrepository_insight";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<CentralRepositoryInsightInfo>), OBJECT_TYPE, "BDO.CentralRepositoryInsight", "InsightID", "InsightLastModified", "InsightGuid", "OriginalDocumentOwnerName", null, null, null, null)
        {
            TouchCacheDependencies = true,
        };


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
        /// Title.
        /// </summary>
        [DatabaseField]
        public virtual string Title
        {
            get => ValidationHelper.GetString(GetValue(nameof(Title)), String.Empty);
            set => SetValue(nameof(Title), value, String.Empty);
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
        /// Date first shared.
        /// </summary>
        [DatabaseField]
        public virtual DateTime DateFirstShared
        {
            get => ValidationHelper.GetDateTime(GetValue(nameof(DateFirstShared)), DateTimeHelper.ZERO_TIME);
            set => SetValue(nameof(DateFirstShared), value, DateTimeHelper.ZERO_TIME);
        }


        /// <summary>
        /// Country involved.
        /// </summary>
        [DatabaseField]
        public virtual string CountryInvolved
        {
            get => ValidationHelper.GetString(GetValue(nameof(CountryInvolved)), String.Empty);
            set => SetValue(nameof(CountryInvolved), value, String.Empty);
        }


        /// <summary>
        /// Service lines.
        /// </summary>
        [DatabaseField]
        public virtual string ServiceLines
        {
            get => ValidationHelper.GetString(GetValue(nameof(ServiceLines)), String.Empty);
            set => SetValue(nameof(ServiceLines), value, String.Empty);
        }


        /// <summary>
        /// Industries.
        /// </summary>
        [DatabaseField]
        public virtual string Industries
        {
            get => ValidationHelper.GetString(GetValue(nameof(Industries)), String.Empty);
            set => SetValue(nameof(Industries), value, String.Empty);
        }


        /// <summary>
        /// Original site guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid OriginalSiteGuid
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(OriginalSiteGuid)), Guid.Empty);
            set => SetValue(nameof(OriginalSiteGuid), value, Guid.Empty);
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
        /// Original document owner name.
        /// </summary>
        [DatabaseField]
        public virtual string OriginalDocumentOwnerName
        {
            get => ValidationHelper.GetString(GetValue(nameof(OriginalDocumentOwnerName)), String.Empty);
            set => SetValue(nameof(OriginalDocumentOwnerName), value, String.Empty);
        }


        /// <summary>
        /// Original document owner email.
        /// </summary>
        [DatabaseField]
        public virtual string OriginalDocumentOwnerEmail
        {
            get => ValidationHelper.GetString(GetValue(nameof(OriginalDocumentOwnerEmail)), String.Empty);
            set => SetValue(nameof(OriginalDocumentOwnerEmail), value, String.Empty);
        }


        /// <summary>
        /// Original document owner opt out.
        /// </summary>
        [DatabaseField]
        public virtual bool OriginalDocumentOwnerOptOut
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(OriginalDocumentOwnerOptOut)), false);
            set => SetValue(nameof(OriginalDocumentOwnerOptOut), value);
        }


        /// <summary>
        /// Source site guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid SourceSiteGuid
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(SourceSiteGuid)), Guid.Empty);
            set => SetValue(nameof(SourceSiteGuid), value, Guid.Empty);
        }


        /// <summary>
        /// Source site name.
        /// </summary>
        [DatabaseField]
        public virtual string SourceSiteName
        {
            get => ValidationHelper.GetString(GetValue(nameof(SourceSiteName)), String.Empty);
            set => SetValue(nameof(SourceSiteName), value, String.Empty);
        }


        /// <summary>
        /// Source document guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid SourceDocumentGuid
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(SourceDocumentGuid)), Guid.Empty);
            set => SetValue(nameof(SourceDocumentGuid), value, Guid.Empty);
        }


        /// <summary>
        /// Source last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime SourceLastModified
        {
            get => ValidationHelper.GetDateTime(GetValue(nameof(SourceLastModified)), DateTimeHelper.ZERO_TIME);
            set => SetValue(nameof(SourceLastModified), value, DateTimeHelper.ZERO_TIME);
        }


        /// <summary>
        /// Url.
        /// </summary>
        [DatabaseField]
        public virtual string Url
        {
            get => ValidationHelper.GetString(GetValue(nameof(Url)), String.Empty);
            set => SetValue(nameof(Url), value, String.Empty);
        }


        /// <summary>
        /// Insights code name.
        /// </summary>
        [DatabaseField]
        public virtual string InsightsCodeName
        {
            get => ValidationHelper.GetString(GetValue(nameof(InsightsCodeName)), String.Empty);
            set => SetValue(nameof(InsightsCodeName), value, String.Empty);
        }


        /// <summary>
        /// Description.
        /// </summary>
        [DatabaseField]
        public virtual string Description
        {
            get => ValidationHelper.GetString(GetValue(nameof(Description)), String.Empty);
            set => SetValue(nameof(Description), value, String.Empty);
        }


        /// <summary>
        /// Original insight is not published.
        /// </summary>
        [DatabaseField]
        public virtual bool OriginalInsightIsNotPublished
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(OriginalInsightIsNotPublished)), false);
            set => SetValue(nameof(OriginalInsightIsNotPublished), value);
        }


        /// <summary>
        /// Insight guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid InsightGuid
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(InsightGuid)), Guid.Empty);
            set => SetValue(nameof(InsightGuid), value);
        }


        /// <summary>
        /// Insight last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime InsightLastModified
        {
            get => ValidationHelper.GetDateTime(GetValue(nameof(InsightLastModified)), DateTimeHelper.ZERO_TIME);
            set => SetValue(nameof(InsightLastModified), value);
        }


        /// <summary>
        /// Sharing instructions.
        /// </summary>
        [DatabaseField]
        public virtual string SharingInstructions
        {
            get => ValidationHelper.GetString(GetValue(nameof(SharingInstructions)), String.Empty);
            set => SetValue(nameof(SharingInstructions), value, String.Empty);
        }


        /// <summary>
        /// Sharing headline.
        /// </summary>
        [DatabaseField]
        public virtual bool SharingHeadline
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(SharingHeadline)), false);
            set => SetValue(nameof(SharingHeadline), value);
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
        /// Creates an empty instance of the <see cref="CentralRepositoryInsightInfo"/> class.
        /// </summary>
        public CentralRepositoryInsightInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="CentralRepositoryInsightInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public CentralRepositoryInsightInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}