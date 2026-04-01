using System;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(CentralRepositoryDeletedDocumentInfo), CentralRepositoryDeletedDocumentInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="CentralRepositoryDeletedDocumentInfo"/>.
    /// </summary>
    public partial class CentralRepositoryDeletedDocumentInfo : AbstractInfo<CentralRepositoryDeletedDocumentInfo, IInfoProvider<CentralRepositoryDeletedDocumentInfo>>, IInfoWithId, IInfoWithName, IInfoWithGuid
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdo.centralrepository_deleteddocument";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<CentralRepositoryDeletedDocumentInfo>), OBJECT_TYPE, "BDO.CentralRepositoryDeletedDocument", "DeletedDocumentID", "DeletedDocumentLastModified", "DeletedDocumentGuid", "DeletedDocumentSiteName", null, null, null, null)
        {
            TouchCacheDependencies = true,
        };


        /// <summary>
        /// Deleted document ID.
        /// </summary>
        [DatabaseField]
        public virtual int DeletedDocumentID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(DeletedDocumentID)), 0);
            set => SetValue(nameof(DeletedDocumentID), value);
        }


        /// <summary>
        /// Deleted document guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid DeletedDocumentGuid
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(DeletedDocumentGuid)), Guid.Empty);
            set => SetValue(nameof(DeletedDocumentGuid), value, Guid.Empty);
        }


        /// <summary>
        /// Deleted document page type.
        /// </summary>
        [DatabaseField]
        public virtual string DeletedDocumentPageType
        {
            get => ValidationHelper.GetString(GetValue(nameof(DeletedDocumentPageType)), String.Empty);
            set => SetValue(nameof(DeletedDocumentPageType), value);
        }


        /// <summary>
        /// Deleted document last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime DeletedDocumentLastModified
        {
            get => ValidationHelper.GetDateTime(GetValue(nameof(DeletedDocumentLastModified)), DateTimeHelper.ZERO_TIME);
            set => SetValue(nameof(DeletedDocumentLastModified), value);
        }


        /// <summary>
        /// Deleted document site ID.
        /// </summary>
        [DatabaseField]
        public virtual int DeletedDocumentSiteID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(DeletedDocumentSiteID)), 0);
            set => SetValue(nameof(DeletedDocumentSiteID), value);
        }


        /// <summary>
        /// Deleted document site name.
        /// </summary>
        [DatabaseField]
        public virtual string DeletedDocumentSiteName
        {
            get => ValidationHelper.GetString(GetValue(nameof(DeletedDocumentSiteName)), String.Empty);
            set => SetValue(nameof(DeletedDocumentSiteName), value);
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
        /// Creates an empty instance of the <see cref="CentralRepositoryDeletedDocumentInfo"/> class.
        /// </summary>
        public CentralRepositoryDeletedDocumentInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="CentralRepositoryDeletedDocumentInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public CentralRepositoryDeletedDocumentInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}