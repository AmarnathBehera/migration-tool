using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(HostedFileInfo), HostedFileInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="HostedFileInfo"/>.
    /// </summary>
    public partial class HostedFileInfo : AbstractInfo<HostedFileInfo, IInfoProvider<HostedFileInfo>>, IInfoWithId
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdohosting.hostedfile";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<HostedFileInfo>), OBJECT_TYPE, "BDOHosting.HostedFile", "HostedFileID", null, null, null, null, null, null, null)
        {
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("HostedFileSiteID", "cms.site", ObjectDependencyEnum.Required),
            },
        };


        /// <summary>
        /// Hosted file ID.
        /// </summary>
        [DatabaseField]
        public virtual int HostedFileID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(HostedFileID)), 0);
            set => SetValue(nameof(HostedFileID), value);
        }


        /// <summary>
        /// Hosted file relative path.
        /// </summary>
        [DatabaseField]
        public virtual string HostedFileRelativePath
        {
            get => ValidationHelper.GetString(GetValue(nameof(HostedFileRelativePath)), String.Empty);
            set => SetValue(nameof(HostedFileRelativePath), value);
        }


        /// <summary>
        /// Hosted file content type.
        /// </summary>
        [DatabaseField]
        public virtual string HostedFileContentType
        {
            get => ValidationHelper.GetString(GetValue(nameof(HostedFileContentType)), String.Empty);
            set => SetValue(nameof(HostedFileContentType), value, String.Empty);
        }


        /// <summary>
        /// Hosted file content charset.
        /// </summary>
        [DatabaseField]
        public virtual string HostedFileContentCharset
        {
            get => ValidationHelper.GetString(GetValue(nameof(HostedFileContentCharset)), String.Empty);
            set => SetValue(nameof(HostedFileContentCharset), value, String.Empty);
        }


        /// <summary>
        /// Hosted file content disposition.
        /// </summary>
        [DatabaseField]
        public virtual string HostedFileContentDisposition
        {
            get => ValidationHelper.GetString(GetValue(nameof(HostedFileContentDisposition)), String.Empty);
            set => SetValue(nameof(HostedFileContentDisposition), value, String.Empty);
        }


        /// <summary>
        /// Hosted file content body.
        /// </summary>
        [DatabaseField]
        public virtual string HostedFileContentBody
        {
            get => ValidationHelper.GetString(GetValue(nameof(HostedFileContentBody)), String.Empty);
            set => SetValue(nameof(HostedFileContentBody), value);
        }


        /// <summary>
        /// Hosted file site ID.
        /// </summary>
        [DatabaseField]
        public virtual int HostedFileSiteID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(HostedFileSiteID)), 0);
            set => SetValue(nameof(HostedFileSiteID), value);
        }


        /// <summary>
        /// Hosted file guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid HostedFileGuid
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(HostedFileGuid)), Guid.Empty);
            set => SetValue(nameof(HostedFileGuid), value);
        }


        /// <summary>
        /// Hosted file last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime HostedFileLastModified
        {
            get => ValidationHelper.GetDateTime(GetValue(nameof(HostedFileLastModified)), DateTimeHelper.ZERO_TIME);
            set => SetValue(nameof(HostedFileLastModified), value);
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
        /// Creates an empty instance of the <see cref="HostedFileInfo"/> class.
        /// </summary>
        public HostedFileInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="HostedFileInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public HostedFileInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}