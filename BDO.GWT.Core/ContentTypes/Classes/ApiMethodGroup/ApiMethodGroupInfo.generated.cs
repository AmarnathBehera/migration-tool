using System;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(ApiMethodGroupInfo), ApiMethodGroupInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="ApiMethodGroupInfo"/>.
    /// </summary>
    public partial class ApiMethodGroupInfo : AbstractInfo<ApiMethodGroupInfo, IInfoProvider<ApiMethodGroupInfo>>, IInfoWithId
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdoapipermissions.apimethodgroup";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<ApiMethodGroupInfo>), OBJECT_TYPE, "BDOAPIPermissions.ApiMethodGroup", "ApiMethodGroupID", null, null, null, null, null, null, null)
        {
            TouchCacheDependencies = true,
        };


        /// <summary>
        /// Api method group ID.
        /// </summary>
        [DatabaseField]
        public virtual int ApiMethodGroupID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(ApiMethodGroupID)), 0);
            set => SetValue(nameof(ApiMethodGroupID), value);
        }


        /// <summary>
        /// Api method group guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid ApiMethodGroupGuid
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(ApiMethodGroupGuid)), Guid.Empty);
            set => SetValue(nameof(ApiMethodGroupGuid), value);
        }


        /// <summary>
        /// Name.
        /// </summary>
        [DatabaseField]
        public virtual string Name
        {
            get => ValidationHelper.GetString(GetValue(nameof(Name)), String.Empty);
            set => SetValue(nameof(Name), value);
        }


        /// <summary>
        /// Display name.
        /// </summary>
        [DatabaseField]
        public virtual string DisplayName
        {
            get => ValidationHelper.GetString(GetValue(nameof(DisplayName)), String.Empty);
            set => SetValue(nameof(DisplayName), value);
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
        /// Only for global admins.
        /// </summary>
        [DatabaseField]
        public virtual bool OnlyForGlobalAdmins
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(OnlyForGlobalAdmins)), false);
            set => SetValue(nameof(OnlyForGlobalAdmins), value);
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
        /// Creates an empty instance of the <see cref="ApiMethodGroupInfo"/> class.
        /// </summary>
        public ApiMethodGroupInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="ApiMethodGroupInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public ApiMethodGroupInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}