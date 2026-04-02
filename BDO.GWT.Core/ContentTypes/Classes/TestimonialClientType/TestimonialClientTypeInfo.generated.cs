using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(TestimonialClientTypeInfo), TestimonialClientTypeInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="TestimonialClientTypeInfo"/>.
    /// </summary>
    public partial class TestimonialClientTypeInfo : AbstractInfo<TestimonialClientTypeInfo, IInfoProvider<TestimonialClientTypeInfo>>, IInfoWithId, IInfoWithName, IInfoWithGuid
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdometadata.testimonialclienttype";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<TestimonialClientTypeInfo>), OBJECT_TYPE, "BDOMetadata.TestimonialClientType", "TestimonialClientTypeID", "TestimonialClientTypeLastModified", "TestimonialClientTypeGuid", "ClientTypeCodeName", "ClientTypeName", null, null, null)
        {
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("TestimonialClientTypeSiteID", "cms.site", ObjectDependencyEnum.Required),
            },
        };


        /// <summary>
        /// Testimonial client type ID.
        /// </summary>
        [DatabaseField]
        public virtual int TestimonialClientTypeID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(TestimonialClientTypeID)), 0);
            set => SetValue(nameof(TestimonialClientTypeID), value);
        }


        /// <summary>
        /// Client type name.
        /// </summary>
        [DatabaseField]
        public virtual string ClientTypeName
        {
            get => ValidationHelper.GetString(GetValue(nameof(ClientTypeName)), String.Empty);
            set => SetValue(nameof(ClientTypeName), value);
        }


        /// <summary>
        /// Client type code name.
        /// </summary>
        [DatabaseField]
        public virtual string ClientTypeCodeName
        {
            get => ValidationHelper.GetString(GetValue(nameof(ClientTypeCodeName)), String.Empty);
            set => SetValue(nameof(ClientTypeCodeName), value);
        }


        /// <summary>
        /// Client type culture code.
        /// </summary>
        [DatabaseField]
        public virtual string ClientTypeCultureCode
        {
            get => ValidationHelper.GetString(GetValue(nameof(ClientTypeCultureCode)), String.Empty);
            set => SetValue(nameof(ClientTypeCultureCode), value);
        }


        /// <summary>
        /// Testimonial client type site ID.
        /// </summary>
        [DatabaseField]
        public virtual int TestimonialClientTypeSiteID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(TestimonialClientTypeSiteID)), 0);
            set => SetValue(nameof(TestimonialClientTypeSiteID), value);
        }


        /// <summary>
        /// Testimonial client type guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid TestimonialClientTypeGuid
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(TestimonialClientTypeGuid)), Guid.Empty);
            set => SetValue(nameof(TestimonialClientTypeGuid), value);
        }


        /// <summary>
        /// Testimonial client type last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime TestimonialClientTypeLastModified
        {
            get => ValidationHelper.GetDateTime(GetValue(nameof(TestimonialClientTypeLastModified)), DateTimeHelper.ZERO_TIME);
            set => SetValue(nameof(TestimonialClientTypeLastModified), value);
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
        /// Creates an empty instance of the <see cref="TestimonialClientTypeInfo"/> class.
        /// </summary>
        public TestimonialClientTypeInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="TestimonialClientTypeInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public TestimonialClientTypeInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}