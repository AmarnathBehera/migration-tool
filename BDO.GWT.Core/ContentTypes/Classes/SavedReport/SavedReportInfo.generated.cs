using System;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(SavedReportInfo), SavedReportInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="SavedReportInfo"/>.
    /// </summary>
    public partial class SavedReportInfo : AbstractInfo<SavedReportInfo, IInfoProvider<SavedReportInfo>>, IInfoWithId, IInfoWithName, IInfoWithGuid
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "sqlbrowser.savedreport";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<SavedReportInfo>), OBJECT_TYPE, "SqlBrowser.SavedReport", "SqlBrowserSavedReportId", null, "SqlBrowserSavedReportGuid", "SqlBrowserSavedReportName", null, null, null, null)
        {
            TouchCacheDependencies = true,
        };


        /// <summary>
        /// Sql browser saved report id.
        /// </summary>
        [DatabaseField]
        public virtual int SqlBrowserSavedReportId
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(SqlBrowserSavedReportId)), 0);
            set => SetValue(nameof(SqlBrowserSavedReportId), value);
        }


        /// <summary>
        /// Sql browser saved report guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid SqlBrowserSavedReportGuid
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(SqlBrowserSavedReportGuid)), Guid.Empty);
            set => SetValue(nameof(SqlBrowserSavedReportGuid), value);
        }


        /// <summary>
        /// Sql browser saved report name.
        /// </summary>
        [DatabaseField]
        public virtual string SqlBrowserSavedReportName
        {
            get => ValidationHelper.GetString(GetValue(nameof(SqlBrowserSavedReportName)), String.Empty);
            set => SetValue(nameof(SqlBrowserSavedReportName), value);
        }


        /// <summary>
        /// Sql browser saved report query id.
        /// </summary>
        [DatabaseField]
        public virtual int SqlBrowserSavedReportQueryId
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(SqlBrowserSavedReportQueryId)), 0);
            set => SetValue(nameof(SqlBrowserSavedReportQueryId), value);
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
        /// Creates an empty instance of the <see cref="SavedReportInfo"/> class.
        /// </summary>
        public SavedReportInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="SavedReportInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public SavedReportInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}