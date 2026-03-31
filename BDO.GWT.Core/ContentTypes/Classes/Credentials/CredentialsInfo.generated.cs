using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(CredentialsInfo), CredentialsInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="CredentialsInfo"/>.
    /// </summary>
    public partial class CredentialsInfo : AbstractInfo<CredentialsInfo, IInfoProvider<CredentialsInfo>>, IInfoWithId, IInfoWithName, IInfoWithGuid
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdometadata.credentials";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<CredentialsInfo>), OBJECT_TYPE, "BDOMetadata.Credentials", "CredentialsID", "CredentialsLastModified", "CredentialsGuid", "CredentialsCodeName", "CredentialsName", null, null, null)
        {
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("CredentialsSiteID", "cms.site", ObjectDependencyEnum.Required),
            },
        };


        /// <summary>
        /// Credentials ID.
        /// </summary>
        [DatabaseField]
        public virtual int CredentialsID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(CredentialsID)), 0);
            set => SetValue(nameof(CredentialsID), value);
        }


        /// <summary>
        /// Credentials name.
        /// </summary>
        [DatabaseField]
        public virtual string CredentialsName
        {
            get => ValidationHelper.GetString(GetValue(nameof(CredentialsName)), String.Empty);
            set => SetValue(nameof(CredentialsName), value);
        }


        /// <summary>
        /// Credentials code name.
        /// </summary>
        [DatabaseField]
        public virtual string CredentialsCodeName
        {
            get => ValidationHelper.GetString(GetValue(nameof(CredentialsCodeName)), String.Empty);
            set => SetValue(nameof(CredentialsCodeName), value);
        }


        /// <summary>
        /// Credentials culture code.
        /// </summary>
        [DatabaseField]
        public virtual string CredentialsCultureCode
        {
            get => ValidationHelper.GetString(GetValue(nameof(CredentialsCultureCode)), String.Empty);
            set => SetValue(nameof(CredentialsCultureCode), value);
        }


        /// <summary>
        /// Credentials site ID.
        /// </summary>
        [DatabaseField]
        public virtual int CredentialsSiteID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(CredentialsSiteID)), 0);
            set => SetValue(nameof(CredentialsSiteID), value);
        }


        /// <summary>
        /// Credentials guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid CredentialsGuid
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(CredentialsGuid)), Guid.Empty);
            set => SetValue(nameof(CredentialsGuid), value);
        }


        /// <summary>
        /// Credentials last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime CredentialsLastModified
        {
            get => ValidationHelper.GetDateTime(GetValue(nameof(CredentialsLastModified)), DateTimeHelper.ZERO_TIME);
            set => SetValue(nameof(CredentialsLastModified), value);
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
        /// Creates an empty instance of the <see cref="CredentialsInfo"/> class.
        /// </summary>
        public CredentialsInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="CredentialsInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public CredentialsInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}