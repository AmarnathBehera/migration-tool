using System;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(ReportingReportInfo), ReportingReportInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="ReportingReportInfo"/>.
    /// </summary>
    public partial class ReportingReportInfo : AbstractInfo<ReportingReportInfo, IInfoProvider<ReportingReportInfo>>, IInfoWithId, IInfoWithName, IInfoWithGuid
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdo.reportingreport";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<ReportingReportInfo>), OBJECT_TYPE, "BDO.ReportingReport", "ReportingReportID", null, "ReportingReportGUID", "ReportingReportCodeName", "ReportingReportDisplayName", null, null, null)
        {
            TouchCacheDependencies = true,
        };


        /// <summary>
        /// Reporting report ID.
        /// </summary>
        [DatabaseField]
        public virtual int ReportingReportID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(ReportingReportID)), 0);
            set => SetValue(nameof(ReportingReportID), value);
        }


        /// <summary>
        /// Reporting report display name.
        /// </summary>
        [DatabaseField]
        public virtual string ReportingReportDisplayName
        {
            get => ValidationHelper.GetString(GetValue(nameof(ReportingReportDisplayName)), String.Empty);
            set => SetValue(nameof(ReportingReportDisplayName), value);
        }


        /// <summary>
        /// Reporting report code name.
        /// </summary>
        [DatabaseField]
        public virtual string ReportingReportCodeName
        {
            get => ValidationHelper.GetString(GetValue(nameof(ReportingReportCodeName)), String.Empty);
            set => SetValue(nameof(ReportingReportCodeName), value);
        }


        /// <summary>
        /// Reporting report channel settings ID.
        /// </summary>
        [DatabaseField]
        public virtual int ReportingReportChannelSettingsID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(ReportingReportChannelSettingsID)), 0);
            set => SetValue(nameof(ReportingReportChannelSettingsID), value);
        }


        /// <summary>
        /// Reporting report query.
        /// </summary>
        [DatabaseField]
        public virtual string ReportingReportQuery
        {
            get => ValidationHelper.GetString(GetValue(nameof(ReportingReportQuery)), String.Empty);
            set => SetValue(nameof(ReportingReportQuery), value);
        }


        /// <summary>
        /// Reporting report GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid ReportingReportGUID
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(ReportingReportGUID)), Guid.Empty);
            set => SetValue(nameof(ReportingReportGUID), value);
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
        /// Creates an empty instance of the <see cref="ReportingReportInfo"/> class.
        /// </summary>
        public ReportingReportInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="ReportingReportInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public ReportingReportInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}