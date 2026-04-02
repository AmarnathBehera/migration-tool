using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(ContentTypeInfo), ContentTypeInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="ContentTypeInfo"/>.
    /// </summary>
    public partial class ContentTypeInfo : AbstractInfo<ContentTypeInfo, IInfoProvider<ContentTypeInfo>>, IInfoWithId, IInfoWithName, IInfoWithGuid
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdometadata.contenttype";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<ContentTypeInfo>), OBJECT_TYPE, "BDOMetadata.ContentType", "ContentTypeID", "ContentTypeLastModified", "ContentTypeGuid", "ContentTypeCodeName", "ContentTypeName", null, null, null)
        {
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("ContentTypeSiteID", "cms.site", ObjectDependencyEnum.Required),
            },
        };


        /// <summary>
        /// Content type ID.
        /// </summary>
        [DatabaseField]
        public virtual int ContentTypeID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(ContentTypeID)), 0);
            set => SetValue(nameof(ContentTypeID), value);
        }


        /// <summary>
        /// Content type name.
        /// </summary>
        [DatabaseField]
        public virtual string ContentTypeName
        {
            get => ValidationHelper.GetString(GetValue(nameof(ContentTypeName)), String.Empty);
            set => SetValue(nameof(ContentTypeName), value);
        }


        /// <summary>
        /// Content type code name.
        /// </summary>
        [DatabaseField]
        public virtual string ContentTypeCodeName
        {
            get => ValidationHelper.GetString(GetValue(nameof(ContentTypeCodeName)), String.Empty);
            set => SetValue(nameof(ContentTypeCodeName), value);
        }


        /// <summary>
        /// Content type culture code.
        /// </summary>
        [DatabaseField]
        public virtual string ContentTypeCultureCode
        {
            get => ValidationHelper.GetString(GetValue(nameof(ContentTypeCultureCode)), String.Empty);
            set => SetValue(nameof(ContentTypeCultureCode), value);
        }


        /// <summary>
        /// Content type see all.
        /// </summary>
        [DatabaseField]
        public virtual string ContentTypeSeeAll
        {
            get => ValidationHelper.GetString(GetValue(nameof(ContentTypeSeeAll)), String.Empty);
            set => SetValue(nameof(ContentTypeSeeAll), value, String.Empty);
        }


        /// <summary>
        /// Content type show see all.
        /// </summary>
        [DatabaseField]
        public virtual bool ContentTypeShowSeeAll
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(ContentTypeShowSeeAll)), true);
            set => SetValue(nameof(ContentTypeShowSeeAll), value);
        }


        /// <summary>
        /// Content type site ID.
        /// </summary>
        [DatabaseField]
        public virtual int ContentTypeSiteID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(ContentTypeSiteID)), 0);
            set => SetValue(nameof(ContentTypeSiteID), value);
        }


        /// <summary>
        /// Content type guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid ContentTypeGuid
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(ContentTypeGuid)), Guid.Empty);
            set => SetValue(nameof(ContentTypeGuid), value);
        }


        /// <summary>
        /// Content type last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime ContentTypeLastModified
        {
            get => ValidationHelper.GetDateTime(GetValue(nameof(ContentTypeLastModified)), DateTimeHelper.ZERO_TIME);
            set => SetValue(nameof(ContentTypeLastModified), value);
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
        /// Creates an empty instance of the <see cref="ContentTypeInfo"/> class.
        /// </summary>
        public ContentTypeInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="ContentTypeInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public ContentTypeInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}