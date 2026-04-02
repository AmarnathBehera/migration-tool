using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(UserApiPermissionInfo), UserApiPermissionInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="UserApiPermissionInfo"/>.
    /// </summary>
    public partial class UserApiPermissionInfo : AbstractInfo<UserApiPermissionInfo, IInfoProvider<UserApiPermissionInfo>>, IInfoWithId, IInfoWithGuid
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdoapipermissions.userapipermission";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<UserApiPermissionInfo>), OBJECT_TYPE, "BDOAPIPermissions.UserApiPermission", "UserApiPermissionID", "UserApiPermissionLastModified", "UserApiPermissionGuid", null, null, null, null, null)
        {
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("UserID", "cms.user", ObjectDependencyEnum.Required),
                new ObjectDependency("SiteID", "cms.site", ObjectDependencyEnum.Required),
            },
        };


        /// <summary>
        /// User api permission ID.
        /// </summary>
        [DatabaseField]
        public virtual int UserApiPermissionID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(UserApiPermissionID)), 0);
            set => SetValue(nameof(UserApiPermissionID), value);
        }


        /// <summary>
        /// User ID.
        /// </summary>
        [DatabaseField]
        public virtual int UserID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(UserID)), 0);
            set => SetValue(nameof(UserID), value);
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
        /// Api method group ID.
        /// </summary>
        [DatabaseField]
        public virtual int ApiMethodGroupID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(ApiMethodGroupID)), 0);
            set => SetValue(nameof(ApiMethodGroupID), value, 0);
        }


        /// <summary>
        /// Allow read.
        /// </summary>
        [DatabaseField]
        public virtual bool AllowRead
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(AllowRead)), false);
            set => SetValue(nameof(AllowRead), value);
        }


        /// <summary>
        /// Allow manage.
        /// </summary>
        [DatabaseField]
        public virtual bool AllowManage
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(AllowManage)), false);
            set => SetValue(nameof(AllowManage), value);
        }


        /// <summary>
        /// User api permission guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid UserApiPermissionGuid
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(UserApiPermissionGuid)), Guid.Empty);
            set => SetValue(nameof(UserApiPermissionGuid), value);
        }


        /// <summary>
        /// User api permission last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime UserApiPermissionLastModified
        {
            get => ValidationHelper.GetDateTime(GetValue(nameof(UserApiPermissionLastModified)), DateTimeHelper.ZERO_TIME);
            set => SetValue(nameof(UserApiPermissionLastModified), value);
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
        /// Creates an empty instance of the <see cref="UserApiPermissionInfo"/> class.
        /// </summary>
        public UserApiPermissionInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="UserApiPermissionInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public UserApiPermissionInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}