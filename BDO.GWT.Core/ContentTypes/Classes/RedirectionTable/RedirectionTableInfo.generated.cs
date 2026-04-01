using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(RedirectionTableInfo), RedirectionTableInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="RedirectionTableInfo"/>.
    /// </summary>
    public partial class RedirectionTableInfo : AbstractInfo<RedirectionTableInfo, IInfoProvider<RedirectionTableInfo>>, IInfoWithId, IInfoWithGuid
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "urlredirection.redirectiontable";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<RedirectionTableInfo>), OBJECT_TYPE, "URLRedirection.RedirectionTable", "RedirectionTableID", "RedirectionTableLastModified", "RedirectionTableGuid", null, null, null, null, null)
        {
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("RedirectionSiteID", "cms.site", ObjectDependencyEnum.Required),
            },
        };


        /// <summary>
        /// Redirection table ID.
        /// </summary>
        [DatabaseField]
        public virtual int RedirectionTableID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(RedirectionTableID)), 0);
            set => SetValue(nameof(RedirectionTableID), value);
        }


        /// <summary>
        /// Redirection enabled.
        /// </summary>
        [DatabaseField]
        public virtual bool RedirectionEnabled
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(RedirectionEnabled)), true);
            set => SetValue(nameof(RedirectionEnabled), value);
        }


        /// <summary>
        /// Redirection original URL.
        /// </summary>
        [DatabaseField]
        public virtual string RedirectionOriginalURL
        {
            get => ValidationHelper.GetString(GetValue(nameof(RedirectionOriginalURL)), String.Empty);
            set => SetValue(nameof(RedirectionOriginalURL), value);
        }


        /// <summary>
        /// Redirection exact match.
        /// </summary>
        [DatabaseField]
        public virtual bool RedirectionExactMatch
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(RedirectionExactMatch)), false);
            set => SetValue(nameof(RedirectionExactMatch), value);
        }


        /// <summary>
        /// Redirection target URL.
        /// </summary>
        [DatabaseField]
        public virtual string RedirectionTargetURL
        {
            get => ValidationHelper.GetString(GetValue(nameof(RedirectionTargetURL)), String.Empty);
            set => SetValue(nameof(RedirectionTargetURL), value);
        }


        /// <summary>
        /// Redirection append query string.
        /// </summary>
        [DatabaseField]
        public virtual bool RedirectionAppendQueryString
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(RedirectionAppendQueryString)), true);
            set => SetValue(nameof(RedirectionAppendQueryString), value);
        }


        /// <summary>
        /// Redirection description.
        /// </summary>
        [DatabaseField]
        public virtual string RedirectionDescription
        {
            get => ValidationHelper.GetString(GetValue(nameof(RedirectionDescription)), String.Empty);
            set => SetValue(nameof(RedirectionDescription), value, String.Empty);
        }


        /// <summary>
        /// Redirection site ID.
        /// </summary>
        [DatabaseField]
        public virtual int RedirectionSiteID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(RedirectionSiteID)), 0);
            set => SetValue(nameof(RedirectionSiteID), value);
        }


        /// <summary>
        /// Redirection type.
        /// </summary>
        [DatabaseField]
        public virtual string RedirectionType
        {
            get => ValidationHelper.GetString(GetValue(nameof(RedirectionType)), "301");
            set => SetValue(nameof(RedirectionType), value);
        }


        /// <summary>
        /// Redirection cultures.
        /// </summary>
        [DatabaseField]
        public virtual string RedirectionCultures
        {
            get => ValidationHelper.GetString(GetValue(nameof(RedirectionCultures)), String.Empty);
            set => SetValue(nameof(RedirectionCultures), value, String.Empty);
        }


        /// <summary>
        /// Redirection culture override.
        /// </summary>
        [DatabaseField]
        public virtual string RedirectionCultureOverride
        {
            get => ValidationHelper.GetString(GetValue(nameof(RedirectionCultureOverride)), String.Empty);
            set => SetValue(nameof(RedirectionCultureOverride), value, String.Empty);
        }


        /// <summary>
        /// Redirection table guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid RedirectionTableGuid
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(RedirectionTableGuid)), Guid.Empty);
            set => SetValue(nameof(RedirectionTableGuid), value);
        }


        /// <summary>
        /// Redirection table last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime RedirectionTableLastModified
        {
            get => ValidationHelper.GetDateTime(GetValue(nameof(RedirectionTableLastModified)), DateTimeHelper.ZERO_TIME);
            set => SetValue(nameof(RedirectionTableLastModified), value);
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
        /// Creates an empty instance of the <see cref="RedirectionTableInfo"/> class.
        /// </summary>
        public RedirectionTableInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="RedirectionTableInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public RedirectionTableInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}