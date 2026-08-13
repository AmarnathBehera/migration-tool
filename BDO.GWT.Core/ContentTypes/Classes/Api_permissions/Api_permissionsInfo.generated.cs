using System;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(Api_permissionsInfo), Api_permissionsInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="Api_permissionsInfo"/>.
    /// </summary>
    public partial class Api_permissionsInfo : AbstractInfo<Api_permissionsInfo, IInfoProvider<Api_permissionsInfo>>, IInfoWithId
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdo.api_permissions";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<Api_permissionsInfo>), OBJECT_TYPE, "bdo.api_permissions", "Api_permissionsID", null, null, null, null, null, null, null)
        {
            TouchCacheDependencies = true,
        };


        /// <summary>
        /// Api ID.
        /// </summary>
        [DatabaseField]
        public virtual int Api_permissionsID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(Api_permissionsID)), 0);
            set => SetValue(nameof(Api_permissionsID), value);
        }


        /// <summary>
        /// API permission web channel setting ID.
        /// </summary>
        [DatabaseField]
        public virtual string APIPermissionWebChannelSettingID
        {
            get => ValidationHelper.GetString(GetValue(nameof(APIPermissionWebChannelSettingID)), String.Empty);
            set => SetValue(nameof(APIPermissionWebChannelSettingID), value);
        }


        /// <summary>
        /// Method group.
        /// </summary>
        [DatabaseField]
        public virtual string MethodGroup
        {
            get => ValidationHelper.GetString(GetValue(nameof(MethodGroup)), String.Empty);
            set => SetValue(nameof(MethodGroup), value, String.Empty);
        }


        /// <summary>
        /// User.
        /// </summary>
        [DatabaseField]
        public virtual string User
        {
            get => ValidationHelper.GetString(GetValue(nameof(User)), String.Empty);
            set => SetValue(nameof(User), value, String.Empty);
        }


        /// <summary>
        /// Read.
        /// </summary>
        [DatabaseField]
        public virtual bool Read
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(Read)), false);
            set => SetValue(nameof(Read), value);
        }


        /// <summary>
        /// Manage.
        /// </summary>
        [DatabaseField]
        public virtual bool Manage
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(Manage)), false);
            set => SetValue(nameof(Manage), value);
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
        /// Creates an empty instance of the <see cref="Api_permissionsInfo"/> class.
        /// </summary>
        public Api_permissionsInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="Api_permissionsInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public Api_permissionsInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}