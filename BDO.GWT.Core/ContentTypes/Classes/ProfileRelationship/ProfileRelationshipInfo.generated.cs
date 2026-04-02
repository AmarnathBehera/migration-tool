using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(ProfileRelationshipInfo), ProfileRelationshipInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="ProfileRelationshipInfo"/>.
    /// </summary>
    public partial class ProfileRelationshipInfo : AbstractInfo<ProfileRelationshipInfo, IInfoProvider<ProfileRelationshipInfo>>, IInfoWithId, IInfoWithName, IInfoWithGuid
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdometadata.profilerelationship";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<ProfileRelationshipInfo>), OBJECT_TYPE, "BDOMetadata.ProfileRelationship", "ProfileRelationshipID", "ProfileRelationshipLastModified", "ProfileRelationshipGuid", "ProfileRelationshipCodeName", "ProfileRelationshipName", null, null, null)
        {
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("ProfileRelationshipSiteID", "cms.site", ObjectDependencyEnum.Required),
            },
        };


        /// <summary>
        /// Profile relationship ID.
        /// </summary>
        [DatabaseField]
        public virtual int ProfileRelationshipID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(ProfileRelationshipID)), 0);
            set => SetValue(nameof(ProfileRelationshipID), value);
        }


        /// <summary>
        /// Profile relationship name.
        /// </summary>
        [DatabaseField]
        public virtual string ProfileRelationshipName
        {
            get => ValidationHelper.GetString(GetValue(nameof(ProfileRelationshipName)), String.Empty);
            set => SetValue(nameof(ProfileRelationshipName), value);
        }


        /// <summary>
        /// Profile relationship code name.
        /// </summary>
        [DatabaseField]
        public virtual string ProfileRelationshipCodeName
        {
            get => ValidationHelper.GetString(GetValue(nameof(ProfileRelationshipCodeName)), String.Empty);
            set => SetValue(nameof(ProfileRelationshipCodeName), value);
        }


        /// <summary>
        /// Profile relationship culture code.
        /// </summary>
        [DatabaseField]
        public virtual string ProfileRelationshipCultureCode
        {
            get => ValidationHelper.GetString(GetValue(nameof(ProfileRelationshipCultureCode)), String.Empty);
            set => SetValue(nameof(ProfileRelationshipCultureCode), value);
        }


        /// <summary>
        /// Profile relationship site ID.
        /// </summary>
        [DatabaseField]
        public virtual int ProfileRelationshipSiteID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(ProfileRelationshipSiteID)), 0);
            set => SetValue(nameof(ProfileRelationshipSiteID), value);
        }


        /// <summary>
        /// Profile relationship guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid ProfileRelationshipGuid
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(ProfileRelationshipGuid)), Guid.Empty);
            set => SetValue(nameof(ProfileRelationshipGuid), value);
        }


        /// <summary>
        /// Profile relationship last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime ProfileRelationshipLastModified
        {
            get => ValidationHelper.GetDateTime(GetValue(nameof(ProfileRelationshipLastModified)), DateTimeHelper.ZERO_TIME);
            set => SetValue(nameof(ProfileRelationshipLastModified), value);
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
        /// Creates an empty instance of the <see cref="ProfileRelationshipInfo"/> class.
        /// </summary>
        public ProfileRelationshipInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="ProfileRelationshipInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public ProfileRelationshipInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}