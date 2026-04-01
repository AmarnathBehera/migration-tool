using System;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(GDPRDeleteLogInfo), GDPRDeleteLogInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="GDPRDeleteLogInfo"/>.
    /// </summary>
    public partial class GDPRDeleteLogInfo : AbstractInfo<GDPRDeleteLogInfo, IInfoProvider<GDPRDeleteLogInfo>>, IInfoWithId, IInfoWithGuid
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdo.gdpr_deletelog";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<GDPRDeleteLogInfo>), OBJECT_TYPE, "BDO.GDPRDeleteLog", "GDPRDeleteLogID", "GDPRDeleteLogLastModified", "GDPRDeleteLogGuid", null, null, null, null, null)
        {
            TouchCacheDependencies = true,
        };


        /// <summary>
        /// GDPR delete log ID.
        /// </summary>
        [DatabaseField]
        public virtual int GDPRDeleteLogID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(GDPRDeleteLogID)), 0);
            set => SetValue(nameof(GDPRDeleteLogID), value);
        }


        /// <summary>
        /// GDPR delete log guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid GDPRDeleteLogGuid
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(GDPRDeleteLogGuid)), Guid.Empty);
            set => SetValue(nameof(GDPRDeleteLogGuid), value);
        }


        /// <summary>
        /// GDPR delete log last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime GDPRDeleteLogLastModified
        {
            get => ValidationHelper.GetDateTime(GetValue(nameof(GDPRDeleteLogLastModified)), DateTimeHelper.ZERO_TIME);
            set => SetValue(nameof(GDPRDeleteLogLastModified), value);
        }


        /// <summary>
        /// Site ID.
        /// </summary>
        [DatabaseField]
        public virtual int SiteID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(SiteID)), 0);
            set => SetValue(nameof(SiteID), value);
        }


        /// <summary>
        /// Sanitize count.
        /// </summary>
        [DatabaseField]
        public virtual int SanitizeCount
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(SanitizeCount)), 0);
            set => SetValue(nameof(SanitizeCount), value, 0);
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
        /// Creates an empty instance of the <see cref="GDPRDeleteLogInfo"/> class.
        /// </summary>
        public GDPRDeleteLogInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="GDPRDeleteLogInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public GDPRDeleteLogInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}