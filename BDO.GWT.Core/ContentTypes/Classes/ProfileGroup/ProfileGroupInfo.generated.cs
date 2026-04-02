using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(ProfileGroupInfo), ProfileGroupInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="ProfileGroupInfo"/>.
    /// </summary>
    public partial class ProfileGroupInfo : AbstractInfo<ProfileGroupInfo, IInfoProvider<ProfileGroupInfo>>, IInfoWithId, IInfoWithName, IInfoWithGuid
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdometadata.profilegroup";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<ProfileGroupInfo>), OBJECT_TYPE, "BDOMetadata.ProfileGroup", "ProfileGroupID", "ProfileGroupLastModified", "ProfileGroupGuid", "ProfileGroupCodeName", "ProfileGroupName", null, null, null)
        {
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("ProfileGroupSiteID", "cms.site", ObjectDependencyEnum.Required),
            },
        };


        /// <summary>
        /// Profile group ID.
        /// </summary>
        [DatabaseField]
        public virtual int ProfileGroupID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(ProfileGroupID)), 0);
            set => SetValue(nameof(ProfileGroupID), value);
        }


        /// <summary>
        /// Profile group name.
        /// </summary>
        [DatabaseField]
        public virtual string ProfileGroupName
        {
            get => ValidationHelper.GetString(GetValue(nameof(ProfileGroupName)), String.Empty);
            set => SetValue(nameof(ProfileGroupName), value);
        }


        /// <summary>
        /// Profile group code name.
        /// </summary>
        [DatabaseField]
        public virtual string ProfileGroupCodeName
        {
            get => ValidationHelper.GetString(GetValue(nameof(ProfileGroupCodeName)), String.Empty);
            set => SetValue(nameof(ProfileGroupCodeName), value);
        }


        /// <summary>
        /// Profile group culture code.
        /// </summary>
        [DatabaseField]
        public virtual string ProfileGroupCultureCode
        {
            get => ValidationHelper.GetString(GetValue(nameof(ProfileGroupCultureCode)), String.Empty);
            set => SetValue(nameof(ProfileGroupCultureCode), value);
        }


        /// <summary>
        /// Profile group site ID.
        /// </summary>
        [DatabaseField]
        public virtual int ProfileGroupSiteID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(ProfileGroupSiteID)), 0);
            set => SetValue(nameof(ProfileGroupSiteID), value);
        }


        /// <summary>
        /// Profile group guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid ProfileGroupGuid
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(ProfileGroupGuid)), Guid.Empty);
            set => SetValue(nameof(ProfileGroupGuid), value);
        }


        /// <summary>
        /// Profile group last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime ProfileGroupLastModified
        {
            get => ValidationHelper.GetDateTime(GetValue(nameof(ProfileGroupLastModified)), DateTimeHelper.ZERO_TIME);
            set => SetValue(nameof(ProfileGroupLastModified), value);
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
        /// Creates an empty instance of the <see cref="ProfileGroupInfo"/> class.
        /// </summary>
        public ProfileGroupInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="ProfileGroupInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public ProfileGroupInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}