using System;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(SavedQueryInfo), SavedQueryInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="SavedQueryInfo"/>.
    /// </summary>
    public partial class SavedQueryInfo : AbstractInfo<SavedQueryInfo, IInfoProvider<SavedQueryInfo>>, IInfoWithId, IInfoWithName, IInfoWithGuid
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "sqlbrowser.savedquery";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<SavedQueryInfo>), OBJECT_TYPE, "SqlBrowser.SavedQuery", "SqlBrowserSavedQueryId", null, "SqlBrowserSavedQueryGuid", "SqlBrowserSavedQueryName", null, null, null, null)
        {
            TouchCacheDependencies = true,
        };


        /// <summary>
        /// Sql browser saved query id.
        /// </summary>
        [DatabaseField]
        public virtual int SqlBrowserSavedQueryId
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(SqlBrowserSavedQueryId)), 0);
            set => SetValue(nameof(SqlBrowserSavedQueryId), value);
        }


        /// <summary>
        /// Sql browser saved query guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid SqlBrowserSavedQueryGuid
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(SqlBrowserSavedQueryGuid)), Guid.Empty);
            set => SetValue(nameof(SqlBrowserSavedQueryGuid), value);
        }


        /// <summary>
        /// Sql browser saved query name.
        /// </summary>
        [DatabaseField]
        public virtual string SqlBrowserSavedQueryName
        {
            get => ValidationHelper.GetString(GetValue(nameof(SqlBrowserSavedQueryName)), String.Empty);
            set => SetValue(nameof(SqlBrowserSavedQueryName), value);
        }


        /// <summary>
        /// Sql browser saved query text.
        /// </summary>
        [DatabaseField]
        public virtual string SqlBrowserSavedQueryText
        {
            get => ValidationHelper.GetString(GetValue(nameof(SqlBrowserSavedQueryText)), String.Empty);
            set => SetValue(nameof(SqlBrowserSavedQueryText), value);
        }


        /// <summary>
        /// Sql browser saved query order.
        /// </summary>
        [DatabaseField]
        public virtual int SqlBrowserSavedQueryOrder
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(SqlBrowserSavedQueryOrder)), 0);
            set => SetValue(nameof(SqlBrowserSavedQueryOrder), value);
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
        /// Creates an empty instance of the <see cref="SavedQueryInfo"/> class.
        /// </summary>
        public SavedQueryInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="SavedQueryInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public SavedQueryInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}